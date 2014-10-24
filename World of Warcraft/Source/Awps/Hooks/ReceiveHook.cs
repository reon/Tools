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
using Awps.Log;
using Awps.Structures;

namespace Awps
{
    class ReceiveHook
    {
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate uint ClientReceiveDummy(IntPtr ptr, IntPtr arg, IntPtr arg2, IntPtr dataPtr);
        delegate uint ClientReceiveDummyx64(IntPtr ptr, IntPtr arg, IntPtr arg2, IntPtr dataPtr);

        static ClientReceiveDummy    originalDelegate;
        static ClientReceiveDummyx64 originalDelegatex64;
        static ClientReceiveDummy    hookDelegate    = new ClientReceiveDummy(ClientReceive);
        static ClientReceiveDummyx64 hookDelegatex64 = new ClientReceiveDummyx64(ClientReceivex64);

        static IntPtr originalFunction;
        static IntPtr hookFunction;

        int instructionLength;

        static byte[] originalInstruction;
        static byte[] hookInstruction;

        public ReceiveHook()
        {
            long address = Helper.GetReceiveHookOffet();

            if (address == 0)
            {
                Console.WriteLine("Can't find Receive address!");
            }
            else
            {
                if (Environment.Is64BitProcess)
                {
                    instructionLength = 12;

                    originalInstruction = new byte[instructionLength];
                    hookInstruction     = new byte[instructionLength];
                
                    hookInstruction[0]  = 0x48;
                    hookInstruction[1]  = 0xB8;
                    hookInstruction[10] = 0xFF;
                    hookInstruction[11] = 0xE0;
                }
                else
                {
                    instructionLength = 5;

                    originalInstruction = new byte[instructionLength];
                    hookInstruction     = new byte[instructionLength];

                    hookInstruction[0] = 0xE9;
                }

                Console.Write("Initialize Receive hook at 0x{0:X8}... ", address);

                // Assign function pointers
                if (Environment.Is64BitProcess)
                {
                    originalDelegatex64 = Marshal.GetDelegateForFunctionPointer(new IntPtr(address + Memory.BaseAddress), typeof(ClientReceiveDummyx64)) as ClientReceiveDummyx64;
                    originalFunction    = Marshal.GetFunctionPointerForDelegate(originalDelegatex64);
                    hookFunction        = Marshal.GetFunctionPointerForDelegate(hookDelegatex64);
                }
                else
                {
                    originalDelegate = Marshal.GetDelegateForFunctionPointer(new IntPtr(address + Memory.BaseAddress), typeof(ClientReceiveDummy)) as ClientReceiveDummy;
                    originalFunction = Marshal.GetFunctionPointerForDelegate(originalDelegate);
                    hookFunction     = Marshal.GetFunctionPointerForDelegate(hookDelegate);
                }

                // Store original & hook instructions
                Buffer.BlockCopy(Memory.Read(originalFunction, instructionLength), 0, originalInstruction, 0, instructionLength);

                if (Environment.Is64BitProcess)
                    Buffer.BlockCopy(BitConverter.GetBytes(hookFunction.ToInt64()), 0, hookInstruction, 2, 8);
                else
                {
                    var hookOffset = hookFunction.ToInt64() - (originalFunction.ToInt64() + instructionLength);

                    Buffer.BlockCopy(BitConverter.GetBytes((uint)hookOffset), 0, hookInstruction, 1, 4);
                }

                Memory.Write(originalFunction, hookInstruction);

                Console.WriteLine("Receive hook successfully initialized!");
            }
        }

        public static uint ClientReceive(IntPtr ptr, IntPtr arg, IntPtr arg2, IntPtr dataPtr)
        {
            var size = BitConverter.ToUInt32(Memory.Read(dataPtr + 8, 4), 0);

            var bufferPtr = BitConverter.ToUInt32(Memory.Read(dataPtr, 4), 0);

            var pkt = new Packet((IntPtr)bufferPtr, (int)size);

            PacketLog.Write(pkt, "ServerMessage");

            Memory.Write(originalFunction, originalInstruction);

            var ret = (uint)originalDelegate.DynamicInvoke(new object[] { ptr, arg, arg2, dataPtr });

            Memory.Write(originalFunction, hookInstruction);

            return ret;
        }


        public static uint ClientReceivex64(IntPtr ptr, IntPtr arg, IntPtr arg2, IntPtr dataPtr)
        {
            var size = BitConverter.ToUInt32(Memory.Read(dataPtr + 16, 4), 0);

            var bufferPtr = BitConverter.ToUInt64(Memory.Read(dataPtr, 8), 0);

            var pkt = new Packet((IntPtr)bufferPtr, (int)size);

            PacketLog.Write(pkt, "ServerMessage");

            Memory.Write(originalFunction, originalInstruction);

            var ret = (uint)originalDelegatex64.DynamicInvoke(new object[] { ptr, arg, arg2, dataPtr });

            Memory.Write(originalFunction, hookInstruction);

            return ret;
        }

        public void Start()
        {
            Memory.Write(originalFunction, hookInstruction);
        }

        public void Remove()
        {
            Memory.Write(originalFunction, originalInstruction);
        }
    }
}
