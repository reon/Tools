/*
 * Copyright (C) 2012-2014 Arctium Emulation <http://arctium.org>
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
using System.IO;

namespace Awps.Structures
{
    class Packet : IDisposable
    {
        public uint TimeStamp { get; set; }
        public uint Message { get; set; }
        public byte[] Data { get; set; }

        BinaryReader stream;

        public Packet(CDataStore dataStore, bool other = false)
        {
            TimeStamp = Helper.GetUnixTime();

            var buffer = Memory.Read(dataStore.Data, (int)dataStore.Size);

            if (buffer != null)
            {
                if (!other)
                {
                    stream = new BinaryReader(new MemoryStream(buffer));

                    Message = stream.ReadUInt32();

                    var size = (int)dataStore.Size - 4;

                    Data = stream.ReadBytes(size);
                }
                else
                {
                    stream = new BinaryReader(new MemoryStream(buffer));

                    var size = (int)dataStore.Size;

                    Data = stream.ReadBytes(size);
                }
            }
        }

        public Packet(IntPtr bufferPtr, int size)
        {
            TimeStamp = Helper.GetUnixTime();

            var buffer = Memory.Read(bufferPtr, size);

            if (buffer != null)
            {
                stream = new BinaryReader(new MemoryStream(buffer));

                Message = stream.ReadUInt32();

                Data = stream.ReadBytes(size - 4);
            }
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}
