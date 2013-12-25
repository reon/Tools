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
using Awps.Misc;
using Microsoft.Win32.SafeHandles;

namespace Awps
{
    class Helper
    {
        public static void InitializeConsole()
        {
            Native.AllocConsole();

            var stdHandle = Native.GetStdHandle(-11);
            var safeFileHandle = new SafeFileHandle(stdHandle, true);
            var fileStream = new FileStream(safeFileHandle, FileAccess.Write);
            var standardOutput = new StreamWriter(fileStream, Encoding.UTF8);

            standardOutput.AutoFlush = true;

            Console.SetOut(standardOutput);
        }

        public static uint GetUnixTime()
        {
            var baseDate = new DateTime(1970, 1, 1);
            var ts = DateTime.Now - baseDate;

            return (uint)ts.TotalSeconds;
        }

        public static long SearchOffset(byte[] binary, byte[] pattern)
        {
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
    }
}
