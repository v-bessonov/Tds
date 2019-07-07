namespace System.Data.SqlClient
{
    public enum SqlConnectionColumnEncryptionSetting
    {
        Disabled = 0,
        Enabled,
    }

    public enum SqlAuthenticationMethod
    {
        NotSpecified = 0,
        SqlPassword,
        ActiveDirectoryPassword,
        ActiveDirectoryIntegrated,
    }

    public enum SqlCommandColumnEncryptionSetting
    {
        UseConnectionSetting = 0,
        Enabled,
        ResultSetOnly,
        Disabled,
    }

    public enum PoolBlockingPeriod
    {
        Auto,
        AlwaysBlock,
        NeverBlock
    }
}
