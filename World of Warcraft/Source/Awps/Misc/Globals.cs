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

namespace Awps
{
    class Globals
    {
        // We'll use them in a future update
        public static byte[] Send = { 0 };
        public static byte[] Receive = { 0 };
        
        // { x86, x64 }
        public static long[] ReceiveAddresses = { 0x397CC3, 0x5AAA40 };
        public static long[] SendAddresses = { 0x799DCD, 0x5AE050 };

        public static string Version = "AWPS 1.3 for WoW Build 18291";
    }
}
