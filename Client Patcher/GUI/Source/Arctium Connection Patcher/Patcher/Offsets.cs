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
            Movement     = 0x363001,
            Movement2    = 0x36300E,
            Movement3    = 0x36301E,
            Movement4    = 0x36302B,
            Legacy       = 0x362FFC,
            Email        = 0x6175C3,
            User         = 0x23F129
        }

        public enum x64
        {
            Movement     = 0x55F745,
            Movement2    = 0x55F752,
            Movement3    = 0x55F761,
            Movement4    = 0x55F76F,
            Legacy       = 0x55F73E,
            Email        = 0xA0635D,
            User         = 0x399290
        }
    }
}
