using DataExtractor;

namespace DataExtractor.Structures
{
    public class SkillLineAbility
    {
        public uint Spell;
        public int RaceMask;
        public int ClassMask;
        public uint SupercedesSpell;
        public ushort SkillLine;
        public ushort MinSkillLineRank;
        public ushort TrivialSkillLineRankHigh;
        public ushort TrivialSkillLineRankLow;
        public ushort UniqueBit;
        public ushort TradeSkillCategoryId;
        public byte AcquireMethod;
        public byte NumSkillUps;
    }
}
