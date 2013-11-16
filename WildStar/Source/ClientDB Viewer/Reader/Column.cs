/*
 * Copyright (C) 2012-2013 Arctium Emulation <http://arctium.org>
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

namespace Arctium_ClientDB_Viewer.Reader
{
    public class Column
    {
        public string Name      { get; set; }
        public uint NameLength  { get; set; }
        public uint Unknown     { get; set; }
        public ulong NameOffset { get; set; }
        public ushort DataType  { get; set; }
        public ushort Unknown2  { get; set; }
        public uint Unknown3    { get; set; }
    }
}
