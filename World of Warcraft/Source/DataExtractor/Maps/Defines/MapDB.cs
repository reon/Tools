// Copyright (c) Arctium Emulation.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DataExtractor.Maps.Defines
{
    class MapDB
    {
        public int Id = 0;
        public string Directory = "";
        public int InstanceType = 0;
        public uint Flags = 0;
        public int Unkown = 0;
        public int MapType = 0;
        public string MapNameLang = "";
        public int AreaTableId = 0;
        public string MapDescription0Lang = "";
        public string MapDescription1Lang = "";
        public int LoadingScreenID = 0;
        public float MinimapIconScale = 0;
        public int CorpseMapID = 0;
        public float[] Corpse = new float[2];
        public int TimeOfDayOverride = 0;
        public int ExpansionID = 0;
        public int RaidOffset = 0;
        public int MaxPlayers = 0;
        public int ParentMapId = 0;
        public int CosmeticParentMapId = 0;
        public int TimeOffset = 0;
    }
}
