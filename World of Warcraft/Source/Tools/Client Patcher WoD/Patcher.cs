/*
 * Copyright (C) 2012-2015 Arctium Emulation <http://www.arctium-emulation.com>
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
using Connection_Patcher.Constants;

namespace Connection_Patcher
{
    class Patcher : IDisposable
    {
        public string Binary { get; set; }
        public bool Initialized { get; private set; }
        public BinaryTypes Type { get; private set; }

        public byte[] binary;
        bool success;

        public Patcher(string file)
        {
            Initialized = false;
            success = false;

            using (var stream = new MemoryStream(File.ReadAllBytes(file)))
            {
                Binary = file;
                binary = stream.ToArray();

                if (binary != null)
                {
                    Type = Helper.GetBinaryType(binary);

                    Initialized = true;
                }
            }
        }

        public void Patch(byte[] bytes, byte[] pattern, long address = 0)
        {
            if (Initialized && (address != 0 || binary.Length >= pattern.Length))
            {
                var offset = pattern == null ? address : SearchOffset(pattern);

                if (offset != 0 && binary.Length >= bytes.Length)
                {
                    try
                    {
                        for (int i = 0; i < bytes.Length; i++)
                            binary[offset + i] = bytes[i];

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("> Patch done at 0x{0:X}", offset);
                    }
                    catch (Exception ex)
                    {
                        throw new NotSupportedException(ex.Message);
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("! Wrong patch (invalid pattern?)");
            }
        }

        long SearchOffset(byte[] pattern)
        {
            long matches;

            for (long i = 0; i < binary.Length; i++)
            {
                if (pattern.Length > (binary.Length - i))
                    return 0;

                for (matches = 0; matches < pattern.Length; matches++)
                {
                    if ((pattern[matches] != 0) && (binary[i + matches] != pattern[matches]))
                        break;
                }

                if (matches == pattern.Length)
                    return i;
            }

            return 0;
        }

        public void Finish()
        {
            success = true;
        }

        public void Dispose()
        {
            if (File.Exists(Binary))
                File.Delete(Binary);

            if (success)
                File.WriteAllBytes(Binary, binary);

            binary = null;
        }
    }
}
