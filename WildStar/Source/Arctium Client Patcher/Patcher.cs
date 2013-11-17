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

namespace Arctium_Client_Patcher
{
    class Patcher
    {
        public bool Initialized { get; set; }

        string binary;

        public Patcher(string file)
        {
            Initialized = false;

            binary = file.Replace(".exe", "") + "_Patched.exe";
            File.Copy(file, binary, true);

            Initialized = true;
        }

        public bool Patch(long offset, params byte[] bytes)
        {
            using (var bw = new BinaryWriter(File.Open(binary, FileMode.Open, FileAccess.ReadWrite)))
            {
                try
                {
                    bw.BaseStream.Seek(offset, SeekOrigin.Begin);
                    bw.Write(bytes);

                    Console.ForegroundColor = ConsoleColor.Green;

                    Console.WriteLine("Successfully patched!");
                    Console.WriteLine("Patched binary: {0}", binary);

                    Console.ForegroundColor = ConsoleColor.Gray;

                    bw.Dispose();

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("{0}", ex.Message);
                }
            }

            if (File.Exists(binary))
                File.Delete(binary);

            return false;
        }

        public bool CheckAddress(long offset, params byte[] bytes)
        {
            try
            {
                using (var br = new BinaryReader(File.Open(binary, FileMode.Open, FileAccess.Read)))
                {
                    br.BaseStream.Seek(offset, SeekOrigin.Begin);

                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if (br.ReadByte() == bytes[i])
                        {
                            Console.WriteLine("Binary already patched!");

                            br.Dispose();

                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.Message);
            }

            if (File.Exists(binary))
                File.Delete(binary);

            return false;
        }
    }
}
