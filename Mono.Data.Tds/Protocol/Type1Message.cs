﻿using System;
using System.Globalization;
using System.Text;

namespace Mono.Data.Tds.Protocol
{
    public class Type1Message : MessageBase
    {

        private string _host;
        private string _domain;

        public Type1Message() : base(1)
        {
            // default values
            _domain = Environment.UserDomainName;
            _host = Environment.MachineName;
            Flags = (NtlmFlags)0xb207;
        }

        public Type1Message(byte[] message) : base(1)
        {
            Decode(message);
        }

        // properties

        public string Domain
        {
            get { return _domain; }
            set
            {
                if (value == null)
                    value = "";
                if (value == "")
                    Flags &= ~NtlmFlags.NegotiateDomainSupplied;
                else
                    Flags |= NtlmFlags.NegotiateDomainSupplied;

                _domain = value;
            }
        }

        public string Host
        {
            get { return _host; }
            set
            {
                if (value == null)
                    value = "";
                if (value == "")
                    Flags &= ~NtlmFlags.NegotiateWorkstationSupplied;
                else
                    Flags |= NtlmFlags.NegotiateWorkstationSupplied;

                _host = value;
            }
        }

        // methods

        protected override void Decode(byte[] message)
        {
            base.Decode(message);

            Flags = (NtlmFlags)BitConverterLE.ToUInt32(message, 12);

            int dom_len = BitConverterLE.ToUInt16(message, 16);
            int dom_off = BitConverterLE.ToUInt16(message, 20);
            _domain = Encoding.ASCII.GetString(message, dom_off, dom_len);

            int host_len = BitConverterLE.ToUInt16(message, 24);
            _host = Encoding.ASCII.GetString(message, 32, host_len);
        }

        public override byte[] GetBytes()
        {
            short dom_len = (short)_domain.Length;
            short host_len = (short)_host.Length;

            byte[] data = PrepareMessage(32 + dom_len + host_len);

            data[12] = (byte)Flags;
            data[13] = (byte)((uint)Flags >> 8);
            data[14] = (byte)((uint)Flags >> 16);
            data[15] = (byte)((uint)Flags >> 24);

            short dom_off = (short)(32 + host_len);

            data[16] = (byte)dom_len;
            data[17] = (byte)(dom_len >> 8);
            data[18] = data[16];
            data[19] = data[17];
            data[20] = (byte)dom_off;
            data[21] = (byte)(dom_off >> 8);

            data[24] = (byte)host_len;
            data[25] = (byte)(host_len >> 8);
            data[26] = data[24];
            data[27] = data[25];
            data[28] = 0x20;
            data[29] = 0x00;

            byte[] host = Encoding.ASCII.GetBytes(_host.ToUpper(CultureInfo.InvariantCulture));
            Buffer.BlockCopy(host, 0, data, 32, host.Length);

            byte[] domain = Encoding.ASCII.GetBytes(_domain.ToUpper(CultureInfo.InvariantCulture));
            Buffer.BlockCopy(domain, 0, data, dom_off, domain.Length);

            return data;
        }
    }
}
