namespace Mono.Data.Tds.Protocol
{
    public enum TdsColumnType
    {
        Binary = 0x2d,      // SYBBINARY
        Bit = 0x32,     // SYBBIT
        Char = 0x2f,        // SYBCHAR
        DateTime = 0x3d,    // SYBDATETIME
        DateTime4 = 0x3a,   // SYBDATETIME4
        DateTime2 = 0x2a,   // SYBMSDATETIME2
        DateTimeOffset = 0x2b,  // SYBMSDATETIMEOFFSET
        DateTimeN = 0x6f,   // SYBDATETIMN
        Decimal = 0x6a,     // SYBDECIMAL
        Real = 0x3b,        // SYBREAL
        Float8 = 0x3e,      // SYBFLT8
        FloatN = 0x6d,      // SYBFLTN
        Image = 0x22,       // SYBIMAGE
        Int1 = 0x30,        // SYBINT1
        Int2 = 0x34,        // SYBINT2
        Int4 = 0x38,        // SYBINT4
        IntN = 0x26,        // SYBINTN
        Void = 0x1f,        // SYBVOID
        Text = 0x23,        // SYBTEXT
        UniqueIdentifier = 0x24,// SYBUNIQUE
        VarBinary = 0x25,   // SYBVARBINARY
        VarChar = 0x27,     // SYBVARCHAR
        Money = 0x3c,       // SYBMONEY
        NText = 0x63,       // SYBNTEXT
        NVarChar = 0x67,    // SYBNVARCHAR
        BitN = 0x68,        // SYBBITN
        Numeric = 0x6c,     // SYBNUMERIC
        MoneyN = 0x6e,      // SYBMONEYN
        Money4 = 0x70,
        NChar = 0xef,       // XSYBNCHAR
        BigBinary = 0xad,   // XSYBBINARY
        BigVarBinary = 0xa5,    // XSYBVARBINARY
        BigVarChar = 0xa7,  // XSYBVARCHAR
        BigNVarChar = 0xe7, // XSYBNVARCHAR
        BigChar = 0xaf,     // XSYBCHAR
        SmallMoney = 0x7a,  // SYBMONEY4
        Variant = 0x62,     // SYBVARIANT
        BigInt = 0x7F       // SYBINT8
    }
}
