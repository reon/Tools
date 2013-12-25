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
    class SendHook
    {
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate uint ClientSendDummy(IntPtr ptr, ref CDataStore dataStore, int args);

        static ClientSendDummy originalDelegate = Marshal.GetDelegateForFunctionPointer(new IntPtr((uint)Globals.SendAddress + (uint)Memory.BaseAddress), typeof(ClientSendDummy)) as ClientSendDummy;
        static ClientSendDummy hookDelegate = new ClientSendDummy(ClientSend);

        #region Detour
        static IntPtr originalFunction;
        static IntPtr hookFunction;

        // Length = 5
        static byte[] originalInstruction = new byte[5];
        static byte[] hookInstruction = new byte[5];
        #endregion

        public SendHook()
        {
            Console.WriteLine("Initialize Send hook...");

            // Assign function pointers
            originalFunction = Marshal.GetFunctionPointerForDelegate(originalDelegate);
            hookFunction = Marshal.GetFunctionPointerForDelegate(hookDelegate);

            // Store original & hook instructions
            Buffer.BlockCopy(Memory.Read(originalFunction, 5), 0, originalInstruction, 0, 5);

            var hookOffset = hookFunction.ToInt64() - (originalFunction.ToInt64() + 5);

            Buffer.BlockCopy(BitConverter.GetBytes((uint)hookOffset), 0, hookInstruction, 1, 4);

            hookInstruction[0] = 0xE9;

            Memory.Write(originalFunction, hookInstruction);

            Console.WriteLine("Send hook successfully initialized");
        }

        public static uint ClientSend(IntPtr ptr, ref CDataStore dataStore, int args)
        {
            var ds = dataStore.Clone();
            var pkt = new Packet(ds);

            PacketLog.Write(pkt, "ClientMessage");

            Memory.Write(originalFunction, originalInstruction);
            

            var ret = (uint)originalDelegate.DynamicInvoke(new object[] { ptr, dataStore, args });

            Memory.Write(originalFunction, hookInstruction);

            return (uint)ret;
        }
    }
}
