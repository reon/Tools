// Copyright (c) Arctium Emulation.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using DataExtractor.Maps.Defines;

namespace DataExtractor
{
    class MapReader
    {
        BinaryReader streamReader;
        BinaryWriter streamWriter;
        byte[] fileData;
        byte position, bitValue;

        public MapReader()
        {
            streamWriter = new BinaryWriter(new MemoryStream());

            position = 0;
            bitValue = 0;
        }

        public void Initialize(byte[] mapData)
        {
            fileData = mapData;

            streamReader = new BinaryReader(new MemoryStream(fileData));
        }

        public void Read(Map map, int x, int y)
        {
            var index = (ushort)(64 * x + y);
            var offset = Helper.SearchOffset(fileData, streamReader.BaseStream.Position, new byte[] { 0x4B, 0x4E, 0x43, 0x4D });

            while (offset != 0)
            {
                streamReader.BaseStream.Position = offset + 12;

                var chunk = new MapChunk();

                chunk.IndexX = (byte)streamReader.ReadUInt32();
                chunk.IndexY = (byte)streamReader.ReadUInt32();

                streamReader.BaseStream.Position += 40;

                chunk.AreaId = (ushort)streamReader.ReadUInt32();

                streamReader.BaseStream.Position += 72;

                if (map.Tiles.ContainsKey(index))
                    map.Tiles[index].Add(chunk);
                else
                    map.Tiles.TryAdd(index, new List<MapChunk>() { chunk });

                offset = Helper.SearchOffset(fileData, streamReader.BaseStream.Position, new byte[] { 0x4B, 0x4E, 0x43, 0x4D });
            }
        }

        public MemoryStream Finish(Map map)
        {
            Write(map.Tiles.Count, 32);

            foreach (var kp in map.Tiles)
            {
                Write(kp.Key, 13);

                kp.Value.ForEach(c =>
                {
                    Write(c.IndexX, 5);
                    Write(c.IndexY, 5);
                    Write(c.AreaId, 12);
                });
            }

            Flush();

            return (MemoryStream)streamWriter.BaseStream;
        }

        public void Write(byte[] data) => streamWriter.Write(data);

        public void Write(object value, int count)
        {
            for (int i = 0; i != count; i++)
            {
                var val = (Convert.ToUInt64(value) >> i) & 1;

                if (val != 0)
                    bitValue |= (byte)(1 << position);

                ++position;

                if (position == 8)
                {
                    streamWriter.Write(bitValue);

                    bitValue = 0;
                    position = 0;
                }
            }
        }

        public void Flush()
        {
            if (position > 0)
            {
                streamWriter.Write(bitValue);

                bitValue = 0;
                position = 0;
            }
        }
    }
}
