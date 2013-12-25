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
using System.Runtime.InteropServices;
using Awps.Log;
using Awps.Structures;

namespace Awps
{
    class ReceiveHook
    {
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate uint ClientReceiveDummy(IntPtr ptr, IntPtr arg, ref CDataStore dataStore, IntPtr arg2);

        static ClientReceiveDummy originalDelegate = Marshal.GetDelegateForFunctionPointer(new IntPtr((uint)Globals.ReceiveAddress + (uint)Memory.BaseAddress), typeof(ClientReceiveDummy)) as ClientReceiveDummy;
        static ClientReceiveDummy hookDelegate = new ClientReceiveDummy(ClientReceive);

        #region Detour
        static IntPtr originalFunction;
        static IntPtr hookFunction;

        // Length = 5
        static byte[] originalInstruction = new byte[5];
        static byte[] hookInstruction = new byte[5];
        #endregion

        public ReceiveHook()
        {
            Console.WriteLine("Initialize Receive hook...");

            // Assign function pointers
            originalFunction = Marshal.GetFunctionPointerForDelegate(originalDelegate);
            hookFunction = Marshal.GetFunctionPointerForDelegate(hookDelegate);

            // Store original & hook instructions
            Buffer.BlockCopy(Memory.Read(originalFunction, 5), 0, originalInstruction, 0, 5);

            var hookOffset = hookFunction.ToInt64() - (originalFunction.ToInt64() + 5);

            Buffer.BlockCopy(BitConverter.GetBytes((uint)hookOffset), 0, hookInstruction, 1, 4);

            hookInstruction[0] = 0xE9;

            Memory.Write(originalFunction, hookInstruction);

            Console.WriteLine("Receive hook successfully initialized");
        }

        public static uint ClientReceive(IntPtr ptr, IntPtr arg, ref CDataStore dataStore, IntPtr arg2)
        {
            var ds = dataStore.Clone();
            var pkt = new Packet(ds);

            PacketLog.Write(pkt, "ServerMessage");

            Memory.Write(originalFunction, originalInstruction);

            var ret = (uint)originalDelegate.DynamicInvoke(new object[] { ptr, arg, dataStore, arg2 });

            Memory.Write(originalFunction, hookInstruction);

            return (uint)ret;
        }
    }
}
