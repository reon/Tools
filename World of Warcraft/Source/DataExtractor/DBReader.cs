// Copyright (c) Arctium Emulation.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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

        // WDB3
        public uint Hash         { get; set; }
        public uint Build        { get; set; }
        public uint Unknown      { get; set; }
        public int Min           { get; set; }
        public int Max           { get; set; }
        public int Locale        { get; set; }
        public int ReferenceDataSize { get; set; }
        public byte[] Data       { get; set; } = new byte[0];
        public byte[] IndexData  { get; set; } = new byte[0];
        public byte[] StringData { get; set; } = new byte[0];
        public byte[] ReferenceDataBlock { get; set; } = new byte[0];
        public Dictionary<uint, ushort> DataBlockOffsets { get; set; } = new Dictionary<uint, ushort>();

        // Signature checks
        public bool IsValidDbcFile { get { return Signature == "WDBC"; } }
        public bool IsValidDb3File { get { return Signature == "WDB3"; } }
    }

    class DBReader
    {
        public static Tuple<DataTable, DataTable> Read(MemoryStream dbStream, Type type)
        {
            var table = new DataTable();
            var refData = new DataTable();

            try
            {
                var dbReader = new BinaryReader(dbStream);

                var header = new DBHeader
                {
                    Signature       = dbReader.ReadString(4),
                    RecordCount     = dbReader.Read<uint>(),
                    FieldCount      = dbReader.Read<uint>(),
                    RecordSize      = dbReader.Read<uint>(),
                    StringBlockSize = dbReader.Read<uint>()
                };

                var hasDataOffsetBlock = false;
                var hasAutoField = false;
                var hasIndex = false;
                var recordSizeList = new List<ushort>();

                if (header.IsValidDb3File)
                {
                    header.Hash = dbReader.Read<uint>();
                    header.Build = dbReader.Read<uint>();
                    header.Unknown = dbReader.Read<uint>();

                    header.Min = dbReader.Read<int>();
                    header.Max = dbReader.Read<int>();
                    header.Locale = dbReader.Read<int>();
                    header.ReferenceDataSize = dbReader.Read<int>();

                    var dataSize = header.RecordCount * header.RecordSize;
                    var indexDataSize = header.RecordCount * 4;

                    hasDataOffsetBlock = dbReader.BaseStream.Length > dataSize + indexDataSize + header.StringBlockSize + header.ReferenceDataSize + 48;

                    if (hasDataOffsetBlock)
                    {
                        var dataStart = dbReader.ReadUInt32();

                        dbReader.BaseStream.Position -= 4;

                        while (header.DataBlockOffsets.Count < header.RecordCount)
                        {
                            var offset = dbReader.ReadUInt32();
                            var size = dbReader.ReadUInt16();

                            if (offset > 0 && !header.DataBlockOffsets.ContainsKey(offset))
                                header.DataBlockOffsets.Add(offset, size);
                        }

                        var dataBlockWriter = new BinaryWriter(new MemoryStream());

                        foreach (var dataBlockOffset in header.DataBlockOffsets)
                        {
                            dbReader.BaseStream.Position = dataBlockOffset.Key;

                            dataBlockWriter.Write(dbReader.ReadBytes(dataBlockOffset.Value));

                            recordSizeList.Add(dataBlockOffset.Value);
                        }

                        header.Data = (dataBlockWriter.BaseStream as MemoryStream).ToArray();

                        dbReader.BaseStream.Position = dataStart + header.Data.Length;
                    }
                    else
                        header.Data = dbReader.ReadBytes((int)dataSize);

                    hasIndex = dbReader.BaseStream.Position + header.StringBlockSize + header.ReferenceDataSize < dbReader.BaseStream.Length;

                    header.StringData = dbReader.ReadBytes((int)header.StringBlockSize);

                    if (hasIndex)
                        header.IndexData = dbReader.ReadBytes((int)indexDataSize);

                    if (dbReader.BaseStream.Position != dbReader.BaseStream.Length)
                        header.ReferenceDataBlock = dbReader.ReadBytes(header.ReferenceDataSize);

                    var data = new BinaryWriter(new MemoryStream());
                    var dataReader = new BinaryReader(new MemoryStream(header.Data));
                    var indexDataReader = new BinaryReader(new MemoryStream(header.IndexData));

                    if (!hasIndex)
                    {
                        for (var i = 0; i < header.RecordCount; i++)
                            data.Write(dataReader.ReadBytes((int)header.RecordSize));
                    }
                    else
                    {
                        hasAutoField = true;

                        if (hasDataOffsetBlock)
                        {
                            for (var i = 0; i < header.RecordCount; i++)
                            {
                                data.Write(indexDataReader.ReadBytes(4));
                                data.Write(dataReader.ReadBytes(recordSizeList[i]));
                            }
                        }
                        else
                        {
                            for (var i = 0; i < header.RecordCount; i++)
                            {
                                data.Write(indexDataReader.ReadBytes(4));
                                data.Write(dataReader.ReadBytes((int)header.RecordSize));
                            }
                        }
                    }

                    data.Write(header.StringData);

                    dataReader.Dispose();
                    indexDataReader.Dispose();

                    dbReader = new BinaryReader(data.BaseStream);
                    dbReader.BaseStream.Position = 0;
                }


                if (header.IsValidDbcFile || header.IsValidDb3File)
                {
                    var fields = type.GetFields();

                    if (hasAutoField)
                        fields = typeof(AutoId).GetFields().Concat(fields).ToArray();

                    var lastStringOffset = 0;
                    var lastString = "";
                    var headerSize = header.IsValidDbcFile ? 20 : 0;

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
                                table.Columns.Add(f.Name + i, arr.GetType().GetElementType());
                        }
                        else
                            table.Columns.Add(f.Name, f.FieldType);
                    }

                    var hasPadding = recordSizeList.Any(v => v > header.RecordSize);

                    for (int i = 0; i < header.RecordCount; i++)
                    {
                        var newObj = Activator.CreateInstance(type);
                        var row = table.NewRow();

                        if (!hasPadding)
                        {
                            dbReader.BaseStream.Position = i*header.RecordSize;

                            if (header.IsValidDbcFile)
                                dbReader.BaseStream.Position += 20;

                            if (hasIndex)
                                dbReader.BaseStream.Position += i*4;
                        }

                        var lastFieldType = "";

                        foreach (var f in fields)
                        {
                            // Check for remaining bytes after 8 bit fields.
                            if (header.IsValidDbcFile && f.FieldType.Name != "Byte" && lastFieldType == "Byte")
                            {
                                var padding = dbReader.BaseStream.Position % 4;

                                if (padding > 0)
                                    dbReader.BaseStream.Position +=  (4 - padding);
                            }

                            lastFieldType = f.FieldType.Name;

                            switch (lastFieldType)
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
                                case "Int16":
                                    row[f.Name] = dbReader.ReadInt16();
                                    break;
                                case "UInt16":
                                    row[f.Name] = dbReader.ReadUInt16();
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
                                case "Int16[]":
                                    length = ((short[])f.GetValue(newObj)).Length;

                                    for (var j = 0; j < length; j++)
                                        row[f.Name + j] = dbReader.ReadInt16();
                                    break;
                                case "UInt16[]":
                                    length = ((ushort[])f.GetValue(newObj)).Length;

                                    for (var j = 0; j < length; j++)
                                        row[f.Name + j] = dbReader.ReadUInt16();
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
                                case "String[]":
                                {
                                    length = ((string[])f.GetValue(newObj)).Length;

                                    for (var j = 0; j < length; j++)
                                    {
                                        if (hasDataOffsetBlock && header.StringBlockSize == 2)
                                        {
                                            row[f.Name + j] = dbReader.ReadCString();
                                        }
                                        else
                                        {
                                            var stringOffset = dbReader.ReadUInt32();

                                            if (stringOffset != lastStringOffset)
                                            {
                                                var currentPos = dbReader.BaseStream.Position;
                                                var stringStart = (header.RecordCount * header.RecordSize) + headerSize + stringOffset;

                                                if (header.IsValidDb3File && hasIndex)
                                                    stringStart += (header.RecordCount * 4);

                                                dbReader.BaseStream.Seek(stringStart, 0);

                                                row[f.Name + j] = lastString = dbReader.ReadCString();

                                                dbReader.BaseStream.Seek(currentPos, 0);
                                            }
                                            else
                                                row[f.Name + j] = lastString;
                                        }
                                    }

                                    break;
                                }
                                case "String":
                                {
                                    if (hasDataOffsetBlock && header.StringBlockSize == 2)
                                    {
                                        row[f.Name] = dbReader.ReadCString();
                                    }
                                    else
                                    {
                                        var stringOffset = dbReader.ReadUInt32();

                                        if (stringOffset != lastStringOffset)
                                        {
                                            var currentPos = dbReader.BaseStream.Position;
                                            var stringStart = (header.RecordCount * header.RecordSize) + headerSize + stringOffset;

                                            if (header.IsValidDb3File && hasIndex)
                                                stringStart += (header.RecordCount * 4);

                                            dbReader.BaseStream.Seek(stringStart, 0);

                                            row[f.Name] = lastString = dbReader.ReadCString();

                                            dbReader.BaseStream.Seek(currentPos, 0);
                                        }
                                        else
                                            row[f.Name] = lastString;
                                    }

                                    break;
                                }
                                default:
                                    dbReader.BaseStream.Position += 4;
                                    break;
                            }
                        }

                        // I have no idea...
                        if (hasPadding)
                            dbReader.BaseStream.Position += 3;

                        table.Rows.Add(row);
                    }

                    table.EndLoadData();
                }

                var refReader = new BinaryReader(new MemoryStream(header.ReferenceDataBlock));

                if (header.ReferenceDataBlock.Length > 0)
                {
                    refData.Columns.Add("Id", typeof(uint));
                    refData.Columns.Add("ReferenceId", typeof(uint));

                    refData.BeginLoadData();

                    while (refReader.BaseStream.Position != refReader.BaseStream.Length)
                    {
                        var row = refData.NewRow();

                        row["Id"] = refReader.ReadUInt32();
                        row["ReferenceId"] = refReader.ReadUInt32();

                        refData.Rows.Add(row);
                    }

                    refData.EndLoadData();
                }
            }
            catch
            {
                Console.WriteLine($"Error while loading {type.Name}");
            }

            return Tuple.Create(table, refData);
        }
    }
}
