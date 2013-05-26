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
            Movement     = 0x3636F4,
            Movement2    = 0x363701,
            Movement3    = 0x363711,
            Movement4    = 0x36371E,
            Legacy       = 0x3636EF,
            Email        = 0x617ADD,
            User         = 0x23F2C1
        }

        public enum x64
        {
            Movement     = 0x55F7F5,
            Movement2    = 0x55F802,
            Movement3    = 0x55F811,
            Movement4    = 0x55F81F,
            Legacy       = 0x55F7EE,
            Email        = 0xA061CD,
            User         = 0x399C30
        }
    }
}
