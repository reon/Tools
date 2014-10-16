using ClientDBExtractor;

namespace ClientDBExtractor.Structures
{
    public class CharStartOutfit
    {
        public int Id;
        public byte RaceId;
        public byte ClassId;
        public byte SexId;
        public byte OutfitId;
        public uint[] ItemId = new uint[24];
        public uint PetDisplayId;
        public uint PetFamilyId;
    }
}
