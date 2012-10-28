namespace ArctiumConnectionPatcher
{
    enum Offsets : int
    {
        // Client movement packets
        Send2Offset              = 0x3F8F8A,
        Send2Offset2             = 0x3F8F97,
        Send2Offset3             = 0x3F8FA1,

        // Client packets
        LegacyRoutingTableOffset = 0xA075F0,

        // Some server packets
        CommsHandlerOffset       = 0x3F8948,

        // Allow login with email addresses
        emailOffset              = 0x78EB4D
    }
}
