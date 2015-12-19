﻿/*
 * Copyright (C) 2012-2015 Arctium Emulation <http://www.arctium-emulation.com>
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

namespace Connection_Patcher.Patches
{
    class Windows
    {
        public static class x86
        {
            public static byte[] BNet      = { 0xC7, 0x41, 0x0C, 0xD5, 0xF8, 0x7F, 0x82, 0x5F };
            public static byte[] Connect   = { 0xEB };
            public static byte[] Password  = { 0x75 };
            public static byte[] Signature = { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0xEB };
        }

        public static class x64
        {
            public static byte[] BNet      = { 0xB8, 0xD5, 0xF8, 0x7F, 0x82, 0x89, 0x41, 0x0C };
            public static byte[] Connect   = { 0xEB };
            public static byte[] Password  = { 0x75 };
            public static byte[] Signature = { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0xE9 };
        }
    }
}
