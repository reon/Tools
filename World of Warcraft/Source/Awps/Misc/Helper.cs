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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Awps.Misc;
using Microsoft.Win32.SafeHandles;

namespace Awps
{
    class Helper
    {
        public static void InitializeConsole()
        {
            NativeMethods.AllocConsole();

            var stdHandle      = NativeMethods.GetStdHandle(-11);
            var safeFileHandle = new SafeFileHandle(stdHandle, true);
            var fileStream     = new FileStream(safeFileHandle, FileAccess.Write);
            var standardOutput = new StreamWriter(fileStream, Encoding.UTF8);

            standardOutput.AutoFlush = true;

            Console.SetOut(standardOutput);
        }

        public static uint GetUnixTime()
        {
            var baseDate = new DateTime(1970, 1, 1);
            var ts       = DateTime.Now - baseDate;

            return (uint)ts.TotalSeconds;
        }

        public static string GetCustomTimeFormat(string format)
        {
            var baseDate = DateTime.Now;

            return baseDate.ToString(format);
        }

        public static BinaryTypes GetBinaryType(byte[] data)
        {
            BinaryTypes type = 0u;

            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var magic = (uint)reader.ReadUInt16();

                // Check MS-DOS magic
                if (magic == 0x5A4D)
                {
                    reader.BaseStream.Seek(0x3C, SeekOrigin.Begin);

                    // Read PE start offset
                    var peOffset = reader.ReadUInt32();

                    reader.BaseStream.Seek(peOffset, SeekOrigin.Begin);

                    var peMagic = reader.ReadUInt32();

                    // Check PE magic
                    if (peMagic != 0x4550)
                        throw new NotSupportedException("Not a PE file!");

                    type = (BinaryTypes)reader.ReadUInt16();
                }
                else
                {
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);

                    type = (BinaryTypes)reader.ReadUInt32();
                }
            }

            return type;
        }

        public static long SearchOffset(byte[] binary, byte[] pattern)
        {
            if (pattern.Length == 0)
                return 0;

            var matches = 0;

            for (long i = 0; i < binary.Length; i++)
            {
                matches = 0;

                for (int j = 0; j < pattern.Length; j++)
                {
                    if (pattern.Length > (binary.Length - i))
                        return 0;

                    if (pattern[j] == 0)
                    {
                        matches++;
                        continue;
                    }

                    if (binary[i + j] != pattern[j])
                        break;

                    matches++;
                }

                if (matches == pattern.Length)
                    return i;
            }

            return 0;
        }

        // Format: {expansion}.{patch}.{subpatch}.{build} -> field value: {3}.{2}.{1}.{any other}
        public static int GetVersionValueFromClient(byte field = 0)
        {
            Process process = Process.GetCurrentProcess();
            string filename = process.Modules[0].FileName;
            var versInfo    = FileVersionInfo.GetVersionInfo(filename);

            int value;

            switch (field)
	        {
		        case 1 : // SubPatch
                    value = versInfo.FileBuildPart;
		        break;
		        case 2 : // Patch value
                    value = versInfo.FileMinorPart;
		        break;
		        case 3 : // Expansion value
                    value = versInfo.FileMajorPart;
		        break;
		        default: // buildNumber value
                    value = versInfo.FilePrivatePart;
		        break;
	        }
	
            return value;
        }

        public static long GetPatternInProgram(byte[] pattern)
        {
            Process process = Process.GetCurrentProcess();

            string filename = process.MainModule.FileName;
            long offset = 0;

            using (var stream = new MemoryStream(File.ReadAllBytes(filename)))
            {
                byte[] binary = stream.ToArray();

                if (binary != null)
                {
                    offset = SearchOffset(binary, pattern);
                }
            }

            if (offset != 0)
                offset = offset + 0x0C00; // get rid of file header

            return offset;
        }
        
        public static long GetSendHookOffet()
        {
            var expansion  = GetVersionValueFromClient(3);
            var build      = GetVersionValueFromClient(0);

            long sendOffset = 0;

            if (!Environment.Is64BitProcess)
            {
                if (expansion == 6)
                {
                    if (build > 19032)
                    {
                        sendOffset = GetPatternInProgram(Patterns.x86.patternSend601);
                    }
                }
            }
            else
            {
                if (expansion == 6)
                {
                    if (build > 19032)
                    {
                        sendOffset = GetPatternInProgram(Patterns.x64.patternSend601);
                    }
                }
            }

            return sendOffset;
        }
        
        public static long GetReceiveHookOffet()
        {
            var expansion  = GetVersionValueFromClient(3);
            var build      = GetVersionValueFromClient(0);

            long receiveOffset = 0;

            if (!Environment.Is64BitProcess)
            {
                if (expansion == 6)
                {
                    if (build > 19032)
                    {
                        receiveOffset = GetPatternInProgram(Patterns.x86.patternReceive19034);
                    }
                }
            }
            else
            {
                if (expansion == 6)
                {
                    if (build > 19032)
                    {
                        receiveOffset = GetPatternInProgram(Patterns.x64.patternReceive19034);
                    }
                }
            }

            return receiveOffset;
        }
    }
}
