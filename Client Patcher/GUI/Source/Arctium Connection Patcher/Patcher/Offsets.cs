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
            Movement     = 0x3634B7,
            Movement2    = 0x3634C4,
            Movement3    = 0x3634D4,
            Movement4    = 0x3634E1,
            Legacy       = 0x3634B2,
            Email        = 0x6179D0,
            User         = 0x23F779
        }

        public enum x64
        {
            Movement     = 0x55FF25,
            Movement2    = 0x55FF32,
            Movement3    = 0x55FF41,
            Movement4    = 0x55FF4F,
            Legacy       = 0x55FF1E,
            Email        = 0xA0640D,
            User         = 0x399F10
        }
    }
}
