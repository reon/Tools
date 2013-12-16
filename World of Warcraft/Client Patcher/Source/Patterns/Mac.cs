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

namespace Connection_Patcher.Patterns
{
    class Mac
    {
        public static class x86
        {
            public static byte[] Send  = { 0x8B, 0x4D, 0xE0, 0x8B, 0x45, 0xD4, 0x89 };
            public static byte[] Email = { 0x74, 0x05, 0xBE, 0x01, 0x00, 0x00, 0x00, 0xA1 };
            public static byte[] User  = { 0x40, 0x00, 0x00, 0x00, 0xE8, 0xD8, 0x33, 0xD5, 0x00 };
            public static byte[] RaF   = { 0xFF, 0x00, 0x0C, 0x83, 0xEC, 0x04, 0x8B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xDB };
        }

        public static class x64
        {
            public static byte[] Send  = { };
            public static byte[] Email = { };
            public static byte[] User  = { };
            public static byte[] RaF   = { };
        }
    }
}
