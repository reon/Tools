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
            var process = Process.GetCurrentProcess();
            var versInfo = FileVersionInfo.GetVersionInfo(process.Modules[0].FileName);

            var value = 0;

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

        public static string GetClientBuildVersion()
        {
            return string.Format("{0}.{1}.{2}_{3}", 
                GetVersionValueFromClient(3), 
                GetVersionValueFromClient(2), 
                GetVersionValueFromClient(1), 
                GetVersionValueFromClient(0)
                );
        }

        public static long GetPatternInProgram(byte[] pattern)
        {
            var process = Process.GetCurrentProcess();
            var binary = File.ReadAllBytes(process.MainModule.FileName);
            var offset = 0L;

            if (binary != null)
            {
                offset = SearchOffset(binary, pattern);

                // get rid of file header
                if (offset != 0)
                    offset += 0x0C00;
            }

            return offset;
        }
        
        public static long GetSendHookOffet()
        {
            if (Environment.Is64BitProcess)
                return GetPatternInProgram(Patterns.x64.Send);
            else
                return GetPatternInProgram(Patterns.x86.Send);
        }

        public static long GetReceiveHookOffet()
        {
            if (Environment.Is64BitProcess)
                return GetPatternInProgram(Patterns.x64.Receive);
            else
                return GetPatternInProgram(Patterns.x86.Receive);
        }
    }
}
