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
    class PacketWriter
    {
        public byte[] Data { get; set; }

        BinaryWriter stream;

        #region Bit Variables
        byte bitPosition = 8;
        byte bitValue;
        #endregion

        public PacketWriter()
        {
            stream = new BinaryWriter(new MemoryStream());
        }

        public PacketWriter(uint message)
        {
            stream = new BinaryWriter(new MemoryStream());

            Write(message);
        }

        public void Finish()
        {
            Data = new byte[stream.BaseStream.Length];

            stream.BaseStream.Seek(0, SeekOrigin.Begin);

            for (int i = 0; i < Data.Length; i++)
                Data[i] = (byte)stream.BaseStream.ReadByte();

            stream.Dispose();
        }

        public void Write(bool value)
        {
            stream.Write(value);
        }

        public void Write(sbyte value)
        {
            stream.Write(value);
        }

        public void Write(byte value)
        {
            stream.Write(value);
        }

        public void Write(short value)
        {
            stream.Write(value);
        }

        public void Write(ushort value)
        {
            stream.Write(value);
        }

        public void Write(int value)
        {
            stream.Write(value);
        }

        public void Write(uint value)
        {
            stream.Write(value);
        }

        public void Write(float value)
        {
            stream.Write(value);
        }

        public void Write(long value)
        {
            stream.Write(value);
        }

        public void Write(ulong value)
        {
            stream.Write(value);
        }

        public void Write(byte[] value)
        {
            stream.Write(value);
        }

        public void WriteBytes(byte[] data, int count = 0)
        {
            if (count == 0)
                stream.Write(data);
            else
                stream.Write(data, 0, count);
        }

        public void Write(SmartGuid guid)
        {
            byte loLength, hiLength, wLoLength, wHiLength;

            var loGuid = GetPackedGuid(guid.Low, out loLength, out wLoLength);
            var hiGuid = GetPackedGuid(guid.High, out hiLength, out wHiLength);

            if (guid.Low == 0 || guid.High == 0)
            {
                Write((byte)0);
                Write((byte)0);
            }
            else
            {
                Write(loLength);
                Write(hiLength);
                WriteBytes(loGuid, wLoLength);
                WriteBytes(hiGuid, wHiLength);
            }
        }

        public static byte[] GetPackedGuid(ulong guid, out byte gLength, out byte written)
        {
            var packedGuid = new byte[8];
            byte gLen = 0;
            byte length = 0;

            for (byte i = 0; guid != 0; i++)
            {
                if ((guid & 0xFF) != 0)
                {
                    gLen |= (byte)(1 << i);
                    packedGuid[length] = (byte)(guid & 0xFF);
                    ++length;
                }

                guid >>= 8;
            }

            gLength = gLen;
            written = length;

            return packedGuid;
        }

        public void PutBit<T>(T bit)
        {
            --bitPosition;

            if (Convert.ToBoolean(bit))
                bitValue |= (byte)(1 << (bitPosition));

            if (bitPosition == 0)
            {
                Write(bitValue);

                bitPosition = 8;
                bitValue = 0;
            }
        }

        public void PutBits<T>(T bit, int count)
        {
            checked
            {
                for (int i = count - 1; i >= 0; --i)
                    PutBit((T)Convert.ChangeType(((Convert.ToInt32(bit) >> i) & 1), typeof(T)));
            }
        }

        public void Flush()
        {
            if (bitPosition == 8)
                return;

            Write(bitValue);

            bitValue = 0;
            bitPosition = 8;
        }
    }
}
