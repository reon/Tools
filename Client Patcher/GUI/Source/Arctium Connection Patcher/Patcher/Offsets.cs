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
        // 5.3.0.16965
        public enum x86
        {
            Movement     = 0x362C0C,
            Movement2    = 0x362C19,
            Movement3    = 0x362C29,
            Movement4    = 0x362C36,
            Legacy       = 0x362C07,
            Email        = 0x6170D3,
            User         = 0x23F156
        }

        // 5.2.0.16826
        public enum x64
        {
            Movement     = 0x52579A,
            Movement2    = 0x5257A7,
            Movement3    = 0x5257B6,
            Movement4    = 0x5257C4,
            Legacy       = 0x525796,
            Email        = 0x9B396D,
            User         = 0x379DA0
        }
    }
}
