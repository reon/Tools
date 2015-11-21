using DataExtractor;

namespace DataExtractor.Structures
{
    public class Item
    {
        public int Id;
        public int FileDataId;             // FileData.dbc, Icon
        public byte Class;
        public byte SubClass;
        public byte SoundOverrideSubClassId;
        public byte Material;
        public byte InventoryType;
        public byte SheatheType;
    }
}
