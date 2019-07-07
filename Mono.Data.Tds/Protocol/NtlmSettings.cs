namespace Mono.Data.Tds.Protocol
{
    public static class NtlmSettings
    {

        static NtlmAuthLevel defaultAuthLevel = NtlmAuthLevel.LM_and_NTLM_and_try_NTLMv2_Session;

        public static NtlmAuthLevel DefaultAuthLevel
        {
            get { return defaultAuthLevel; }
            set { defaultAuthLevel = value; }
        }
    }
}
