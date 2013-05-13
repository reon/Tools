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
        public static void Inject(Process p, string dll)
        {
            if (p == null)
                throw new InvalidOperationException("Process doesn't exist.");

            var procdessId = p.Id;
            var loadLibPtr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            var dllPathAsBytes = Encoding.ASCII.GetBytes(dll);

            if (loadLibPtr == IntPtr.Zero)
                throw new InvalidOperationException("Can't get ptr for LoadLibraryA.");

            IntPtr lpAddress = VirtualAllocEx(p.Handle, (IntPtr)null,(uint) dll.Length, 0x1000, 0x40);
            
            if (lpAddress == IntPtr.Zero)
                throw new InvalidOperationException("VirtualAllocEx failed.");

            if (WriteProcessMemory(p.Handle, lpAddress, dllPathAsBytes, (uint)dllPathAsBytes.Length, 0) != 0)
                if (CreateRemoteThread(p.Handle, IntPtr.Zero, 0, loadLibPtr, lpAddress, 0, IntPtr.Zero) == IntPtr.Zero)
                    throw new InvalidOperationException("creating remote thread failed.");
        }
    }
}
