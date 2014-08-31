using ClientDBExtractor;

namespace ClientDBExtractor.Structures
{
    public class SkillLine
    {
        public int ID;
        public int CategoryID;
        public Unused DisplayNameLang;
        public Unused DescriptionLang;
        public Unused SpellIconID;
        public Unused AlternateVerbLang;
        public int CanLink;
        public int ParentSkillLineID;
        public int Flags;
    }
}
