// Copyright (c) Arctium Emulation.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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

        // DB3
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
                    Signature = dbReader.ReadString(4),
                    RecordCount = dbReader.Read<uint>(),
                    FieldCount = dbReader.Read<uint>(),
                    RecordSize = dbReader.Read<uint>(),
                    StringBlockSize = dbReader.Read<uint>()
                };

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

                    if (header.Min != 0 && header.Max != 0)
                    {
                        var recordSizeList = new List<ushort>();

                        // Use 0xFF for guessing that type of data blocks...
                        if (header.RecordSize > 0xFF)
                        {
                            var dataStart = dbReader.ReadUInt32();

                            dbReader.BaseStream.Position -= 4;

                            while (dbReader.BaseStream.Position != dataStart)
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

                        var hasIndex = dbReader.BaseStream.Position + header.StringBlockSize + header.ReferenceDataSize < dbReader.BaseStream.Length;

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
                            // Use 0xFF for guessing that type of data blocks...
                            if (header.RecordSize > 0xFF)
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
                }


                if (header.IsValidDbcFile || header.IsValidDb3File)
                {
                    var fields = type.GetFields();
                    var headerLength = dbReader.BaseStream.Position;
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
                                case "String":
                                {
                                    if (header.RecordSize > 0xFF)
                                    {
                                        row[f.Name] = dbReader.ReadCString();
                                    }
                                    else
                                    {

                                        var stringOffset = dbReader.ReadUInt32();

                                        if (stringOffset != lastStringOffset)
                                        {
                                            var currentPos = dbReader.BaseStream.Position;
                                            var stringStart = (header.RecordCount*header.RecordSize) + headerSize +
                                                              stringOffset;

                                            if (header.IsValidDb3File)
                                                stringStart += (header.RecordCount*4);

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

                        if (header.RecordSize <= 0xFF)
                        {
                            // Read remaining bytes if needed
                            var remainingBytes = (int) (dbReader.BaseStream.Position - headerLength) % 4;

                            if (remainingBytes > 0)
                                dbReader.ReadBytes(header.IsValidDb3File ? 4 - remainingBytes : remainingBytes);
                        }
                        else
                            // 3 taken from Item-sparse data block. Not sure if other files have the same...
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
            }

            return Tuple.Create(table, refData);
        }
    }
}
