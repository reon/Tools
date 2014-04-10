﻿/*
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

namespace Connection_Patcher.Patterns
{
    class Windows
    {
        public static class x86
        {
            public static byte[] BNet      = { 0x8B, 0x75, 0x08, 0x8D, 0x78, 0x0C };
            public static byte[] Send      = { 0x8B, 0x45, 0xF4, 0x48, 0x8B, 0xC8 };
            public static byte[] Password  = { 0x74, 0x89, 0x8B, 0x16, 0x8B, 0x42, 0x04 };
            public static byte[] Signature = { 0xE8, 0x00, 0x00, 0x00, 0x00, 0x84, 0xC0, 0x75, 0x5F, 0x33, 0xC0 };
            
        }

        public static class x64
        {
            public static byte[] BNet = { };
        }
    }
}