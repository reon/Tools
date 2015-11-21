using DataExtractor;

namespace DataExtractor.Structures
{
    public class ChrRaces
    {
        public uint Index;
        public uint Id;
        public uint Flags;
        public Unused ClientPrefix;                  // string
        public Unused ClientFileString;              // string
        public Unused NameLang;                      // string
        public Unused NameFemaleLang;                // string
        public Unused NameMaleLang;                  // string
        public Unused MaleFacialHairCustomization;   // string
        public Unused FemaleFacialHairCustomization; // string
        public Unused HairCustomization;             // string
        public Unused CreateScreenFileDataId;
        public Unused SelectScreenFileDataId;
        public Unused[] MaleCustomizeOffset = new Unused[3];   // float
        public Unused[] FemaleCustomizeOffset = new Unused[3]; // float
        public Unused LowResScreenFileDataId;
        public ushort FactionId;
        public ushort ExplorationSoundId;
        public ushort MaleDisplayId;
        public ushort FemaleDisplayId;
        public UnusedShort ResSicknessSpellId;
        public UnusedShort SplashSoundId;
        public ushort CinematicSequenceId;
        public UnusedShort UaMaleCreatureSoundDataId;
        public UnusedShort UaFemaleCreatureSoundDataId;
        public UnusedShort HighResMaleDisplayId;
        public UnusedShort HighResFemaleDisplayId;
        public UnusedShort Unknown;
        public byte BaseLanguage;
        public UnusedByte CreatureType;
        public byte Alliance;
        public UnusedByte RaceRelated;
        public UnusedByte UnalteredVisualRaceId;
        public UnusedByte CharComponentTextureLayoutId;
        public UnusedByte DefaultClassId;
        public UnusedByte NeutralRaceId;
        public UnusedByte CharComponentTexLayoutHiResId;
    }
}
