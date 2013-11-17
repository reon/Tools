/*
 * Copyright (C) 2012-2013 Arctium Emulation <http://arctium.org>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Framework;

namespace Arctium_ClientDB_Viewer.Reader
{
    public class DBReader
    {
        public static void Read(string tblFile, Dispatcher dp, ProgressBar progressBar, DataGrid dg)
        {
            new Thread(() =>
            {
                var columns = new List<Column>();
                var table = new DataTable();

                try
                {
                    using (var dbReader = new BinaryReader(new MemoryStream(File.ReadAllBytes(tblFile))))
                    {
                        DBHeader header = new DBHeader();
                        {
                            header.Signature         = dbReader.ReadString(4, false);
                            header.Version           = dbReader.ReadUInt32();    // Not used
                            header.TableNameLength   = dbReader.ReadUInt64();
                            header.Unknown           = dbReader.ReadUInt64();    // Not used
                            header.RecordSize        = dbReader.ReadUInt64();
                            header.FieldCount        = dbReader.ReadUInt64();
                            header.DescriptionOffset = dbReader.ReadUInt64();
                            header.RecordCount       = dbReader.ReadUInt64();
                            header.FullRecordSize    = dbReader.ReadUInt64();    // Not used
                            header.EntryOffset       = dbReader.ReadUInt64();
                            header.MaxId             = dbReader.ReadUInt64();    // Not used
                            header.IDLookupOffset    = dbReader.ReadUInt64();    // Not used
                            header.Unknown2          = dbReader.ReadUInt64();    // Not used
                        }

                        if (header.IsValidTblFile)
                        {
                            var tableName = dbReader.ReadString((int)header.TableNameLength);

                            if (!Globals.DBTables.TryAdd(Globals.SelectedItem, null))
                            {
                                MessageBox.Show(tableName + " already loaded as table");

                                // Exit current reader
                                dbReader.Dispose();

                                return;
                            }

                            table.BeginLoadData();

                            for (uint i = 0; i < header.FieldCount; i++)
                            {
                                dbReader.BaseStream.Position = (long)(header.DescriptionOffset + 0x60 + (0x18 * i));

                                var column = new Column
                                {
                                    NameLength = dbReader.ReadUInt32(),
                                    Unknown = dbReader.ReadUInt32(),
                                    NameOffset = dbReader.ReadUInt64(),
                                    DataType = dbReader.ReadUInt16(),
                                    Unknown2 = dbReader.ReadUInt16(),
                                    Unknown3 = dbReader.ReadUInt32(),
                                };

                                var namePos = (long)(((0x60 + header.FieldCount * 0x18) + header.DescriptionOffset) + column.NameOffset);

                                // Read name for column
                                dbReader.BaseStream.Position = header.FieldCount % 2 == 0 ? namePos : namePos + 8;

                                column.Name = dbReader.ReadString((int)column.NameLength - 1);

                                table.Columns.Add(column.Name);
                                columns.Add(column);

                                switch (column.DataType)
                                {
                                    case 3:
                                        table.Columns[column.Name].DataType = typeof(uint);
                                        break;
                                    case 4:
                                        table.Columns[column.Name].DataType = typeof(float);
                                        break;
                                    case 11:
                                        table.Columns[column.Name].DataType = typeof(string);
                                        break;
                                    case 20:
                                        table.Columns[column.Name].DataType = typeof(ulong);
                                        break;
                                    case 130:
                                        table.Columns[column.Name].DataType = typeof(string);
                                        break;
                                    default:
                                        MessageBox.Show("Not supported data type '" + column.DataType + "'");
                                        break;
                                }
                            }

                            var firstEntryPos = header.EntryOffset + 0x60;
                            ulong ctr = 0;

                            for (uint i = 0; i < header.RecordCount; i++)
                            {
                                var row = table.NewRow();
                                var lastType = 0;
                                bool skip = false;

                                dbReader.BaseStream.Position = (long)((long)firstEntryPos + ((long)header.RecordSize * i));

                                for (int j = 0; j < (int)header.FieldCount; j++)
                                {
                                    var column = columns[j];

                                    if (skip && column.DataType != 130 && lastType == 130)
                                        dbReader.BaseStream.Position += 4;

                                    switch (column.DataType)
                                    {
                                        case 3:
                                            row[column.Name] = dbReader.ReadUInt32();
                                            break;
                                        case 4:
                                            row[column.Name] = dbReader.ReadSingle();
                                            break;
                                        case 11:
                                            row[column.Name] = Convert.ToBoolean(dbReader.ReadUInt32()).ToString();
                                            break;
                                        case 20:
                                            row[column.Name] = dbReader.ReadUInt64();
                                            break;
                                        case 130:
                                            var stringOffset = dbReader.ReadUInt32();
                                            var stringOffset2 = dbReader.ReadUInt32();

                                            if (stringOffset == 0)
                                                skip = true;

                                            var lastPosition = dbReader.BaseStream.Position;

                                            dbReader.BaseStream.Position = (long)((stringOffset > 0 ? stringOffset : stringOffset2) + firstEntryPos);

                                            row[column.Name] = dbReader.ReadWString();

                                            dbReader.BaseStream.Position = lastPosition;
                                            break;
                                        default:
                                            break;
                                    }

                                    lastType = column.DataType;
                                }

                                table.Rows.Add(row);

                                // Let's cheat a bit with our progressbar :)
                                if (i == (header.RecordCount / 100) * ctr)
                                    dp.Invoke(new Action(() => progressBar.Value = ctr++ - ctr * 0.01), DispatcherPriority.Send);
                            }

                            table.EndLoadData();

                            Globals.DBTables.TryUpdate(Globals.SelectedItem, table, null);
                        }

                        dp.Invoke(new Action(() => progressBar.Value = 100.0), DispatcherPriority.Send);
                        dp.Invoke(new Action(() => dg.ItemsSource = table.DefaultView), DispatcherPriority.Normal);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Error while loading {0}: {1}", tblFile, ex.Message));
                }
            }).Start();
        }
    }
}
