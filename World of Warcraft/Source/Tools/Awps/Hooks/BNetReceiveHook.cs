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
using Awps.Structures;

namespace Awps.Hooks
{
    public class BNetReceiveHook
    {
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate uint ClientReceiveDummy(IntPtr ptr, IntPtr arg2, IntPtr arg3, IntPtr packetPtr);

        static ClientReceiveDummy originalDelegate;
        static ClientReceiveDummy hookDelegate = new ClientReceiveDummy(ClientReceive);

        static IntPtr originalFunction;
        static IntPtr hookFunction;

        int instructionLength;

        static byte[] originalInstruction;
        static byte[] hookInstruction;

        public BNetReceiveHook()
        {
            var address = Helper.GetBNetReceiveHookOffet();

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
                    hookInstruction = new byte[instructionLength];

                    hookInstruction[0] = 0x48;
                    hookInstruction[1] = 0xB8;
                    hookInstruction[10] = 0xFF;
                    hookInstruction[11] = 0xE0;
                }
                else
                {
                    instructionLength = 5;

                    originalInstruction = new byte[instructionLength];
                    hookInstruction = new byte[instructionLength];

                    hookInstruction[0] = 0xE9;
                }

                Console.Write("Initialize Receive hook at 0x{0:X8}... ", address);

                // Assign function pointers
                originalDelegate = Marshal.GetDelegateForFunctionPointer(new IntPtr(address + Memory.BaseAddress), typeof(ClientReceiveDummy)) as ClientReceiveDummy;
                originalFunction = Marshal.GetFunctionPointerForDelegate(originalDelegate);
                hookFunction = Marshal.GetFunctionPointerForDelegate(hookDelegate);

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

        public static uint ClientReceive(IntPtr ptr, IntPtr arg2, IntPtr arg3, IntPtr packetPtr)
        {
            var pktSize = BitConverter.ToInt32(Memory.Read(packetPtr + (IntPtr.Size << 1), 4), 0);
            var pktBufPtr = Memory.Read(packetPtr);
            var buffer = Memory.Read(pktBufPtr, pktSize);
            var pkt = new BNetPacket(buffer, pktSize);

            Awps.bnetLogger.Write(pkt, "Server");

            Memory.Write(originalFunction, originalInstruction);

            var ret = (uint)originalDelegate.DynamicInvoke(new object[] { ptr, arg2, arg3, packetPtr });

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
