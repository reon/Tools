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
            Movement     = 0x447F75,
            Movement2    = 0x447F82,
            Movement3    = 0x447F92,
            Movement4    = 0x447F9F,
            Legacy       = 0x447F70,
            Email        = 0x800766,
            User         = 0x2E56D5
        }

        public enum x64
        {
            Movement     = 0x5247BA,
            Movement2    = 0x5247C7,
            Movement3    = 0x5247D6,
            Movement4    = 0x5247E4,
            Legacy       = 0x5247B6,
            Email        = 0x9B2E3D,
            User         = 0x379310 
        }
    }
}
