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
using Awps.Misc;

namespace Awps
{
    class Memory
    {
        public static Int64 BaseAddress { get; set; }
        static Process currentProcess;
        static IntPtr currentHandle;
        public static bool IsInitialized;

        public static void Initialize()
        {
            Process.EnterDebugMode();

            if (IsInitialized)
                throw new InvalidOperationException("Memory reader already initialized");

            var process = Process.GetCurrentProcess();

            if (process== null)
                throw new InvalidOperationException("No valid process found.");

            currentProcess = process;
            currentHandle = process.Handle;

            BaseAddress = currentProcess.MainModule.BaseAddress.ToInt64();
            IsInitialized = true;
        }

        public static byte[] Read(IntPtr address, int size)
        {
            try
            {
                var buffer = new byte[size];

                NativeMethods.ReadProcessMemory(currentHandle, address, buffer, size);

                return buffer;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
            }

            return new byte[0];
        }

        public static void Write(IntPtr address, byte[] data)
        {
            try
            {
                uint oldProtect;

                NativeMethods.VirtualProtect(address, (uint)data.Length, 0x80, out oldProtect);

                var realAddress = new IntPtr((long)address);

                NativeMethods.WriteProcessMemory(currentProcess.Handle, realAddress, data, data.Length);

                NativeMethods.FlushInstructionCache(currentHandle, address, (uint)data.Length);
                NativeMethods.VirtualProtect(address, (uint)data.Length, oldProtect, out oldProtect);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
            }
        }

        public static void Dispose()
        {
            currentProcess = null;
            BaseAddress = 0;
            IsInitialized = false;
        }
    }
}
