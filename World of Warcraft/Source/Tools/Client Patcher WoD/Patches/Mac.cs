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
    class Mac
    {
        public static class x64
        {
            public static byte[] BNet      = { 0xB8, 0xD5, 0xF8, 0x7F, 0x82, 0x89, 0x47, 0x0C, 0x5D, 0xC3, 0x90, 0x90, 0x90 };
            public static byte[] Connect   = { 0xEB };
            public static byte[] Password  = { 0x0F, 0x85 };
            public static byte[] Signature = { 0x31, 0xC0, 0xFF, 0xC0, 0xC3, 0x90 };      
        }
    }
}
