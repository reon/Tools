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
using System.Runtime.InteropServices;
using System.Threading;
using Awps.Structures;

namespace Awps
{
    class SendCDataStore
    {
        static long sendOffset = Helper.GetSendHookOffet(); // NetClient::Send(CDataStore *,CONNECTION_ID) 
        static IntPtr curConn;
        static IntPtr vTable;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate uint SendCDataStoreDummy(IntPtr ptr, ref CDataStore dataStore, int args);

        static SendCDataStoreDummy originalDelegate;

        public static void Initialize()
        {
            while (curConn == IntPtr.Zero)
            {
                Console.WriteLine("Waiting for CurrentConnection initialization.");

                Thread.Sleep(100);

                curConn = Memory.Read((IntPtr)(0x1625A70 + Memory.BaseAddress)); // ClientServices::s_currentConnection
            }

            Console.WriteLine("CurrentConnection initialized.");

            while (curConn == IntPtr.Zero)
            {
                Console.WriteLine("Waiting for `vtable for'CDataStore initialization.");

                Thread.Sleep(100);

                vTable = new IntPtr(0xEA32C0 + Memory.BaseAddress); // `vtable for'CDataStore
            }

            Console.WriteLine("`vtable for'CDataStore initialized.");

            originalDelegate = Marshal.GetDelegateForFunctionPointer(new IntPtr(sendOffset + Memory.BaseAddress), typeof(SendCDataStoreDummy)) as SendCDataStoreDummy;

            Console.WriteLine("SendCDataStoreDummy successfully initialized.");
        }

        public static void Send(PacketWriter pkt)
        {
            pkt.Finish();

            var size = pkt.Data.Length;
            var ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(pkt.Data, 0, ptr, size);

            var cDataStore = new CDataStore
            {
                VTable = vTable,
                Size = (uint)size,
                Data = ptr,
                Alloc = (uint)size,
                Base = 0,
                Read = 0
            };

            originalDelegate.DynamicInvoke(new object[] { curConn, cDataStore, 2 });

            Marshal.FreeHGlobal(ptr);
        }
    }
}
