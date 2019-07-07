namespace Mono.Data.Tds.Protocol
{
    public enum TdsEnvPacketSubType
    {
        Database = 0x1,
        CharSet = 0x3,
        BlockSize = 0x4,
        Locale = 0x5,
        CollationInfo = 0x7
    }
}
