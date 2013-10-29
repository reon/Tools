/*
 * Copyright (C) 2012-2013 Arctium <http://arctium.org>
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

namespace Arctium_Connection_Patcher.Patcher
{
    public class Offset
    {
        public enum x86
        {
            Movement  = 0x38E64E,
            Movement2 = 0x38E655,
            Movement3 = 0x38E660,
            Movement4 = 0x38E66A,
            Legacy    = 0x38E64A,
            Email     = 0x6534F4,
            User      = 0x240D48,
            RaF       = 0x65341C
        }
        public enum x64
        {
            Movement  = 0x59B0A1,
            Movement2 = 0x59B0B7,
            Movement3 = 0x59B0C3,
            Legacy    = 0x59B09A,
            Email     = 0xA5F89D,
            User      = 0x39A480,
            RaF       = 0xA5F6B9
        }
    }
}
