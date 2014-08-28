using ClientDBExtractor;

namespace ClientDBExtractor.Structures
{
    public class ChrRaces
    {
        public int Id;
        public int Flags;
        public int FactionId;
        public int ExplorationSoundId;
        public int MaleDisplayId;
        public int FemaleDisplayId;
        public Unused ClientPrefix;
        public int BaseLanguage;
        public Unused CreatureType;
        public Unused ResSicknessSpellId;
        public Unused SplashSoundId;
        public Unused ClientFileString;
        public int CinematicSequenceId;
        public int Alliance;
        public Unused NameLang;
        public Unused NameFemaleLang;
        public Unused NameMaleLang;
        public Unused MaleFacialHairCustomization;
        public Unused FemaleFacialHairCustomization;
        public Unused HairCustomization;
        public Unused RaceRelated;
        public Unused UnalteredVisualRaceId;
        public Unused UaMaleCreatureSoundDataId;
        public Unused UaFemaleCreatureSoundDataId;
        public Unused CharComponentTextureLayoutId;
        public Unused DefaultClassId;
        public Unused CreateScreenFileDataId;
        public Unused SelectScreenFileDataId;
        public Unused[] MaleCustomizeOffset = new Unused[3];
        public Unused[] FemaleCustomizeOffset = new Unused[3];
        public Unused NeutralRaceId;
        public Unused LowResScreenFileDataId;
        public Unused HighResMaleDisplayId;
        public Unused HighResFemaleDisplayId;
        public Unused CharComponentTexLayoutHiResId;
        public Unused Unknown;
    }
}
