namespace Mono.Data.Tds.Protocol
{
    public enum TdsPacketType
    {
        None = 0x0,
        Query = 0x1,
        Logon = 0x2,
        Proc = 0x3,
        Reply = 0x4,
        Cancel = 0x6,
        Bulk = 0x7,
        Logon70 = 0x10,
        SspAuth = 0x11,
        Logoff = 0x71,
        Normal = 0x0f,
        DBRPC = 0xe6,
        RPC = 0x3
    }
}
