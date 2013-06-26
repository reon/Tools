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
            Movement     = 0x363AFE,
            Movement2    = 0x363B0B,
            Movement3    = 0x363B1B,
            Movement4    = 0x363B28,
            Legacy       = 0x363AF9,
            Email        = 0x617E30,
            User         = 0x23FBB3
        }

        public enum x64
        {
            Movement     = 0x55FF55,
            Movement2    = 0x55FF62,
            Movement3    = 0x55FF71,
            Movement4    = 0x55FF7F,
            Legacy       = 0x55FF4E,
            Email        = 0xA06AFD,
            User         = 0x399B70
        }
    }
}
