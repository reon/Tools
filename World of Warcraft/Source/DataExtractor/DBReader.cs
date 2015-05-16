// Copyright (c) Arctium Emulation.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Data;
using System.IO;
using DataExtractor.Reader;

namespace DataExtractor
{
    public class DBHeader
    {
        public string Signature     { get; set; }
        public uint RecordCount     { get; set; }
        public uint FieldCount      { get; set; }
        public uint RecordSize      { get; set; }
        public uint StringBlockSize { get; set; }

        // DB2
        public uint Hash     { get; set; }
        public uint Build    { get; set; }
        public uint Unknown  { get; set; }
        public int Min       { get; set; }
        public int Max       { get; set; }
        public int Locale    { get; set; }
        public int Unknown2  { get; set; }
        public byte[] Bytes  { get; set; }
        public byte[] Bytes2 { get; set; }

        // Signature checks
        public bool IsValidDbcFile { get { return Signature == "WDBC"; } }
        public bool IsValidDb2File { get { return Signature == "WDB2"; } }
    }

    class DBReader
    {
        public static DataTable Read(MemoryStream dbStream, Type type)
        {
            var table = new DataTable();

            try
            {
                using (var dbReader = new BinaryReader(dbStream))
                {
                    var header = new DBHeader
                    {
                        Signature = dbReader.ReadString(4),
                        RecordCount = dbReader.Read<uint>(),
                        FieldCount = dbReader.Read<uint>(),
                        RecordSize = dbReader.Read<uint>(),
                        StringBlockSize = dbReader.Read<uint>()
                    };

                    if (header.IsValidDb2File)
                    {
                        header.Hash = dbReader.Read<uint>();
                        header.Build = dbReader.Read<uint>();
                        header.Unknown = dbReader.Read<uint>();
                        header.Min = dbReader.Read<int>();
                        header.Max = dbReader.Read<int>();
                        header.Locale = dbReader.Read<int>();
                        header.Unknown2 = dbReader.Read<int>();

                        if (header.Max != 0)
                        {
                            var diff = (header.Max - header.Min) + 1;

                            header.Bytes = dbReader.ReadBytes(diff * 4);
                            header.Bytes2 = dbReader.ReadBytes(diff * 2);
                        }
                    }

                    if (header.IsValidDbcFile || header.IsValidDb2File)
                    {
                        var fields = type.GetFields();
                        var headerLength = dbReader.BaseStream.Position;
                        var lastStringOffset = 0;
                        var lastString = "";
                        var stringOffsetAdd = header.IsValidDbcFile ? 20 : 48;

                        table.BeginLoadData();

                        foreach (var f in fields)
                        {
                            if (f.FieldType == typeof(Unused) || f.FieldType == typeof(UnusedByte) || f.FieldType == typeof(UnusedShort) || f.FieldType == typeof(UnusedLong) ||
                                f.FieldType == typeof(Unused[]) || f.FieldType == typeof(UnusedByte[]) || f.FieldType == typeof(UnusedShort[]) || f.FieldType == typeof(UnusedLong[]))
                                continue;

                                if (f.FieldType.IsArray)
                                {
                                    var arr = f.GetValue(Activator.CreateInstance(type)) as Array;

                                    for (var i = 0; i < arr.Length; i++)
                                        table.Columns.Add(f.Name + i, arr.GetValue(0).GetType());
                                }
                                else
                                    table.Columns.Add(f.Name, f.FieldType);
                            }

                        for (int i = 0; i < header.RecordCount; i++)
                        {
                            var newObj = Activator.CreateInstance(type);
                            var row = table.NewRow();

                            foreach (var f in fields)
                            {
                                switch (f.FieldType.Name)
                                {
                                    case "Unused":
                                        dbReader.BaseStream.Position += 4;
                                        break;
                                    case "UnusedByte":
                                        dbReader.BaseStream.Position += 1;
                                        break;
                                    case "UnusedShort":
                                        dbReader.BaseStream.Position += 2;
                                        break;
                                    case "UnusedLong":
                                        dbReader.BaseStream.Position += 8;
                                        break;
                                    case "SByte":
                                        row[f.Name] = dbReader.ReadSByte();
                                        break;
                                    case "Byte":
                                        row[f.Name] = dbReader.ReadByte();
                                        break;
                                    case "Int32":
                                        row[f.Name] = dbReader.ReadInt32();
                                        break;
                                    case "UInt32":
                                        row[f.Name] = dbReader.ReadUInt32();
                                        break;
                                    case "Int64":
                                        row[f.Name] = dbReader.ReadInt64();
                                        break;
                                    case "UInt64":
                                        row[f.Name] = dbReader.ReadUInt64();
                                        break;
                                    case "Single":
                                        row[f.Name] = dbReader.ReadSingle();
                                        break;
                                    case "Boolean":
                                        row[f.Name] = dbReader.ReadBoolean();
                                        break;
                                    case "Unused[]":
                                        var length = ((Unused[])f.GetValue(newObj)).Length;

                                        for (var j = 0; j < length; j++)
                                            dbReader.BaseStream.Position += 4;
                                        break;
                                    case "UnusedByte[]":
                                        length = ((Unused[])f.GetValue(newObj)).Length;

                                        for (var j = 0; j < length; j++)
                                            dbReader.BaseStream.Position += 4;
                                        break;
                                    case "UnusedLong[]":
                                        length = ((Unused[])f.GetValue(newObj)).Length;

                                        for (var j = 0; j < length; j++)
                                            dbReader.BaseStream.Position += 4;
                                        break;
                                    case "UnusedShort[]":
                                        length = ((Unused[])f.GetValue(newObj)).Length;

                                        for (var j = 0; j < length; j++)
                                            dbReader.BaseStream.Position += 4;
                                        break;
                                    case "SByte[]":
                                        length = ((sbyte[])f.GetValue(newObj)).Length;

                                        for (var j = 0; j < length; j++)
                                            row[f.Name + j] = dbReader.ReadSByte();
                                        break;
                                    case "Byte[]":
                                        length = ((byte[])f.GetValue(newObj)).Length;

                                        for (var j = 0; j < length; j++)
                                            row[f.Name + j] = dbReader.ReadByte();
                                        break;
                                    case "Int32[]":
                                        length = ((int[])f.GetValue(newObj)).Length;

                                        for (var j = 0; j < length; j++)
                                            row[f.Name + j] = dbReader.ReadInt32();
                                        break;
                                    case "UInt32[]":
                                        length = ((uint[])f.GetValue(newObj)).Length;

                                        for (var j = 0; j < length; j++)
                                            row[f.Name + j] = dbReader.ReadUInt32();
                                        break;
                                    case "Single[]":
                                        length = ((float[])f.GetValue(newObj)).Length;

                                        for (var j = 0; j < length; j++)
                                            row[f.Name + j] = dbReader.ReadSingle();
                                        break;
                                    case "Int64[]":
                                        length = ((long[])f.GetValue(newObj)).Length;

                                        for (var j = 0; j < length; j++)
                                            row[f.Name + j] = dbReader.ReadInt64();
                                        break;
                                    case "UInt64[]":
                                        length = ((ulong[])f.GetValue(newObj)).Length;

                                        for (var j = 0; j < length; j++)
                                            row[f.Name + j] = dbReader.ReadUInt64();
                                        break;
                                    case "String":
                                        {
                                            var stringOffset = dbReader.ReadUInt32();

                                            if (stringOffset != lastStringOffset)
                                            {
                                                var currentPos = dbReader.BaseStream.Position;
                                                var stringStart = (header.RecordCount * header.RecordSize) + stringOffsetAdd + stringOffset;
                                                dbReader.BaseStream.Seek(stringStart, 0);
                                                row[f.Name] = lastString = dbReader.ReadCString();

                                                dbReader.BaseStream.Seek(currentPos, 0);
                                            }
                                            else
                                                row[f.Name] = lastString;

                                            break;
                                        }
                                    default:
                                        dbReader.BaseStream.Position += 4;
                                        break;
                                }
                            }

                            // Read remaining bytes if needed
                            var remainingBytes = (int)(dbReader.BaseStream.Position - headerLength) % 4;

                            dbReader.ReadBytes(remainingBytes);

                            table.Rows.Add(row);
                        }

                        table.EndLoadData();
                    }
                }
            }
            catch
            {
            }

            return table;
        }
    }
}
