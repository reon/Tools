using DataExtractor;

namespace DataExtractor.Structures
{
    public class SkillLine
    {
        public Unused DisplayNameLang;   // string
        public Unused DescriptionLang;   // string
        public Unused AlternateVerbLang; // string
        public UnusedShort SpellIconID;
        public ushort Flags;
        public byte CategoryID;
        public byte CanLink;
        public byte ParentSkillLineID;
    }
}
