using ClientDBExtractor;

namespace ClientDBExtractor.Structures
{
    public class ItemModifiedAppearance
    {
        public int Id;
        public int ItemId;
        public int Mode;
        public int AppearanceId;
        public int FileDataId;   // FileData.dbc, Icon
        public int Version;
    }
}
