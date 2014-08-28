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
        public int[] ItemId = new int[24];
        public int PetDisplayId;
        public int PetFamilyId;
    }
}
