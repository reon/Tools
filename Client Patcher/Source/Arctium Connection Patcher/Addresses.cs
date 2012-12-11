namespace ArctiumConnectionPatcher
{
    enum Offsetsx32 : int
    {
        // Client movement packets
        SendOffset               = 0x40BA59,
        SendOffset2              = 0x40BA69,
        SendOffset3              = 0x40BA76,
        // Client packets
        LegacyRoutingTableOffset = 0xA48298,
        // Some server packets
        CommsHandlerOffset       = 0x40B42F,
        // Allow login with email addresses
        emailOffset              = 0x7C3F96
    }
    enum Offsetsx64 : int
    {
        // Client movement packets
        SendOffset               = 0x4D0AF6,
        SendOffset2              = 0x4D0B04,
        SendOffset3              = 0x4D0B11,
        // Client packets
        LegacyRoutingTableOffset = 0xC54110,
        // Some server packets
        CommsHandlerOffset       = 0x4D0226,
        // Allow login with email addresses
        emailOffset              = 0x95C42D
    }

}
