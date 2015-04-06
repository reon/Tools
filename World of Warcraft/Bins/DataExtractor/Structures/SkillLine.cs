using DataExtractor;

namespace DataExtractor.Structures
{
    public class SkillLine
    {
        public uint ID;
        public uint CategoryID;
        public Unused DisplayNameLang;
        public Unused DescriptionLang;
        public Unused SpellIconID;
        public Unused AlternateVerbLang;
        public uint CanLink;
        public uint ParentSkillLineID;
        public uint Flags;
    }
}
