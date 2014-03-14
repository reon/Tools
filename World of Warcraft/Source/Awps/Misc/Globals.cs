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
        public static byte[] Send = { 0 };
        public static byte[] Receive = { 0 };
        
        // { x86, x64 }
        public static long[] ReceiveAddresses = { 0x3986FB, 0x5AB880 };
        public static long[] SendAddresses = { 0x39A8E3, 0x5AEF00 };

        public static string Version = "AWPS 1.2 for WoW Build 18019";
    }
}
