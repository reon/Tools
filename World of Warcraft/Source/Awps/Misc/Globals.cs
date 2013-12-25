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

namespace Awps
{
    class Globals
    {
        // We'll use them in a future update
        public static byte[] Send = { 0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x10, 0x53, 0x56, 0x8B, 0xF1, 0x8D, 0x8E };
        public static byte[] Receive = { 0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x18, 0xFF, 0x05, 0x00, 0x00, 0x00, 0x00, 0x53 };

        public static IntPtr ReceiveAddress = (IntPtr)0x3965BB;
        public static IntPtr SendAddress = (IntPtr)0x3988D7;

        public static string Version = "AWPS 1.0 for WoW Build 17688";
    }
}
