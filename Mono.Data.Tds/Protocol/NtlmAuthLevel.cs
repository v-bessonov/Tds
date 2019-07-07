namespace Mono.Data.Tds.Protocol
{
    public enum NtlmAuthLevel
    {
        /* Use LM and NTLM, never use NTLMv2 session security. */
        LM_and_NTLM,

        /* Use NTLMv2 session security if the server supports it,
		 * otherwise fall back to LM and NTLM. */
        LM_and_NTLM_and_try_NTLMv2_Session,

        /* Use NTLMv2 session security if the server supports it,
		 * otherwise fall back to NTLM.  Never use LM. */
        NTLM_only,

        /* Use NTLMv2 only. */
        NTLMv2_only,
    }
}
