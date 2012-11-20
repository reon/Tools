namespace ArctiumConnectionPatcher
{
    enum Offsets : int
    {
        // Client movement packets
        SendOffset               = 0x40B9F9,
        SendOffset2              = 0x40BA09,
        SendOffset3              = 0x40BA16,

        // Client packets
        LegacyRoutingTableOffset = 0xA481F0,

        // Some server packets
        CommsHandlerOffset       = 0x40B3CF,

        // Allow login with email addresses
        emailOffset              = 0x7C40D6
    }
}
