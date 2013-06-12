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
            Movement     = 0x363420,
            Movement2    = 0x36342D,
            Movement3    = 0x36343D,
            Movement4    = 0x36344A,
            Legacy       = 0x36341B,
            Email        = 0x617B36,
            User         = 0x23F779
        }

        public enum x64
        {
            Movement     = 0x560D65,
            Movement2    = 0x560D72,
            Movement3    = 0x560D81,
            Movement4    = 0x560D8F,
            Legacy       = 0x560D5E,
            Email        = 0xA0698D,
            User         = 0x39A8F0
        }
    }
}
