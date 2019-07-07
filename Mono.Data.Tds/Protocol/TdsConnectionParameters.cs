using System;
using System.Security;

namespace Mono.Data.Tds.Protocol
{
    public class TdsConnectionParameters
    {
        public string ApplicationName;
        public string Database;
        public string Charset;
        public string Hostname;
        public string Language;
        public string LibraryName;
        public SecureString Password;
        public bool PasswordSet;
        public string ProgName;
        public string User;
        public bool DomainLogin;
        public string DefaultDomain;
        public string AttachDBFileName;

        public TdsConnectionParameters()
        {
            Reset();
        }

        public void Reset()
        {
            ApplicationName = "Mono";
            Database = String.Empty;
            Charset = String.Empty;
            Hostname = System.Net.Dns.GetHostName();
            Language = String.Empty;
            LibraryName = "Mono";
            Password = new SecureString();
            PasswordSet = false;
            ProgName = "Mono";
            User = String.Empty;
            DomainLogin = false;
            DefaultDomain = String.Empty;
            AttachDBFileName = String.Empty;
        }
    }
}
