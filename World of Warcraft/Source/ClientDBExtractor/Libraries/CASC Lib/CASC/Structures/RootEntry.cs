using CASC_Lib.CASC.Constants;

namespace CASC_Lib.CASC.Structures
{
    public struct RootEntry
    {
        public byte[] MD5 { get; set; }
        public ulong Hash { get; set; }
        public Locales Locales { get; set; }
    }
}
