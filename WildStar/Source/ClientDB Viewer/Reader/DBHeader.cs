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
    public class DBHeader
    {
        public string Signature        { get; set; }
        public uint Version            { get; set; }
        public ulong TableNameLength   { get; set; }
        public ulong Unknown           { get; set; }
        public ulong RecordSize        { get; set; }
        public ulong FieldCount        { get; set; }
        public ulong DescriptionOffset { get; set; }
        public ulong RecordCount       { get; set; }
        public ulong FullRecordSize    { get; set; }
        public ulong EntryOffset       { get; set; }
        public ulong MaxId             { get; set; }
        public ulong IDLookupOffset    { get; set; }
        public ulong Unknown2          { get; set; }

        public bool IsValidTblFile { get { return Signature == "LBTD"; } }
    }
}
