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
    public class BNetPacket
    {
        public uint TimeStamp { get; set; }
        public uint Message { get; set; }
        public bool HasChannel { get; set; }
        public byte Channel { get; set; }
        public byte[] Data { get; set; }
        public int ProcessedBytes { get; set; }

        BinaryReader stream;
        byte bytePart;
        int count;

        public BNetPacket(byte[] buffer, int size)
        {
            TimeStamp = Helper.GetUnixTime();

            if (buffer != null)
            {
                stream = new BinaryReader(new MemoryStream(buffer));

                Message = Read<uint>(6);
                HasChannel = Read<bool>(1);

                if (HasChannel)
                    Channel = Read<byte>(4);

                Data = buffer;
            }
        }

        public T Read<T>(int bits)
        {
            ulong value = 0;
            var bitsToRead = 0;

            while (bits != 0)
            {
                if ((count % 8) == 0)
                {
                    bytePart = stream.ReadByte();

                    ProcessedBytes += 1;
                }

                var shiftedBits = count & 7;
                bitsToRead = 8 - shiftedBits;

                if (bitsToRead >= bits)
                    bitsToRead = bits;

                bits -= bitsToRead;

                value |= (uint)((bytePart >> shiftedBits) & ((byte)(1 << bitsToRead) - 1)) << bits;

                count += bitsToRead;
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
