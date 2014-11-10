/*
 * Copyright (C) 2012-2013 Arctium <http://arctium.org>
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
using System.Text;

namespace Arctium_Injector
{
    public class Functions : Native
    {
        public static void Inject(Process process, string dll)
        {
            if (process == null)
                throw new InvalidOperationException("Process doesn't exist.");

            var loadLibPtr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            byte[] dllPathAsBytes = Encoding.ASCII.GetBytes(dll);

            if (loadLibPtr == IntPtr.Zero)
                throw new InvalidOperationException("Can't get ptr for LoadLibraryA.");

            IntPtr lpAddress = VirtualAllocEx(process.Handle, (IntPtr)null, (uint)dll.Length + 1, MemCommit, PageExecuteReadWrite);
            
            if (lpAddress == IntPtr.Zero)
                throw new InvalidOperationException("VirtualAllocEx failed.");

            if (WriteProcessMemory(process.Handle, lpAddress, dllPathAsBytes, (uint)dllPathAsBytes.Length, 0) != 0)
                if (CreateRemoteThread(process.Handle, IntPtr.Zero, 0, loadLibPtr, lpAddress, 0, IntPtr.Zero) == IntPtr.Zero)
                    throw new InvalidOperationException("creating remote thread failed.");
        }

        public static bool IsProcessAlreadyInjected(Process process, string moduleName)
        {
            ProcessModuleCollection theModules = process.Modules;
            foreach (ProcessModule module in theModules)
            {
                if (module.FileName.Contains(moduleName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
