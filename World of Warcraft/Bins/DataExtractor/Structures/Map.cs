using DataExtractor;

namespace DataExtractor.Structures
{
    public class Map
    {
        public int Id;
        public string Directory;
        public int InstanceType;
        public uint[] Flags = new uint[2];
        public int MapType;
        public string MapNameLang;
        public int AreaTableId;
        public Unused MapDescription0Lang; // string
        public Unused MapDescription1Lang; // string
        public Unused LoadingScreenID;
        public Unused MinimapIconScale;
        public int CorpseMapID;
        public float[] Corpse = new float[2];
        public Unused TimeOfDayOverride;
        public int ExpansionID;
        public Unused RaidOffset;
        public int MaxPlayers;
        public int ParentMapId;
        public int CosmeticParentMapId;
        public Unused TimeOffset;
    }
}
