using System.Security.Cryptography;

namespace Mono.Data.Tds.Protocol
{
#if !INSIDE_CORLIB
    public
#endif
        abstract class MD4 : HashAlgorithm
    {

        protected MD4()
        {
            // MD4 hash length are 128 bits long
            HashSizeValue = 128;
        }

        public static new MD4 Create()
        {
#if FULL_AOT_RUNTIME
			return new MD4Managed ();
#else
            // for this to work we must register ourself with CryptoConfig
            return Create("MD4");
#endif
        }

        public static new MD4 Create(string hashName)
        {
            object o = CryptoConfig.CreateFromName(hashName);
            // in case machine.config isn't configured to use any MD4 implementation
            if (o == null)
            {
                o = new MD4Managed();
            }
            return (MD4)o;
        }
    }
}
