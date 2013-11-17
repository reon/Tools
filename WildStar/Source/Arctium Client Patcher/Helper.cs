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
using System.Reflection;
using Arctium_Client_Patcher.Constants;

namespace Arctium_Client_Patcher
{
    class Helper
    {
        public static bool IsAMD64(string file)
        {
            try
            {
                using (var br = new BinaryReader(new MemoryStream(File.ReadAllBytes(file))))
                {
                    br.BaseStream.Seek((long)PEHeader.Start, SeekOrigin.Begin);

                    var startOffset = br.ReadInt32();

                    br.BaseStream.Seek(startOffset, SeekOrigin.Begin);

                    var validation = br.ReadUInt32();

                    if (validation != 0x00004550)
                        throw new InvalidOperationException("No valid PE Header found!");

                    var type = (PEHeader)br.ReadUInt16();

                    return type == PEHeader.AMD64;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.Message);
                Console.ReadKey(true);

                Environment.Exit(0);

                return false;
            }
        }
    }
}
