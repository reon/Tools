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
using System.IO;
using System.Text;

namespace Arctium_ClientDB_Viewer.Reader
{
    public static class DBReaderExtensions
    {
        public static string ReadWString(this BinaryReader br)
        {
            var bytes = new byte[0];
            var lastByte = br.ReadByte();

            while (lastByte != 0)
            {
                br.BaseStream.Position -= 1;

                bytes = bytes.Combine(br.ReadBytes(2));

                lastByte = br.ReadByte();
            }

            return Encoding.Unicode.GetString(bytes);
        }

        public static string ReadString(this BinaryReader br, int count, bool unicode = true)
        {
            var stringArray = br.ReadBytes(unicode ? count << 1 : count);

            return unicode ? Encoding.Unicode.GetString(stringArray) : Encoding.ASCII.GetString(stringArray);
        }

        public static byte[] Combine(this byte[] data, byte[] data2)
        {
            var combined = new byte[data.Length + data2.Length];

            Buffer.BlockCopy(data, 0, combined, 0, data.Length);
            Buffer.BlockCopy(data2, 0, combined, data.Length, data2.Length);

            return combined;
        }
    }
}
