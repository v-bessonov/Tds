using System;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Data.Tds.Protocol
{
    public class Type2Message : MessageBase
    {

        private byte[] _nonce;
        private string _targetName;
        private byte[] _targetInfo;

        public Type2Message() : base(2)
        {
            _nonce = new byte[8];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(_nonce);
            // default values
            Flags = (NtlmFlags)0x8201;
        }

        public Type2Message(byte[] message) : base(2)
        {
            _nonce = new byte[8];
            Decode(message);
        }

        ~Type2Message()
        {
            if (_nonce != null)
                Array.Clear(_nonce, 0, _nonce.Length);
        }

        // properties

        public byte[] Nonce
        {
            get { return (byte[])_nonce.Clone(); }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Nonce");
                if (value.Length != 8)
                {
                    string msg = Locale.GetText("Invalid Nonce Length (should be 8 bytes).");
                    throw new ArgumentException(msg, "Nonce");
                }
                _nonce = (byte[])value.Clone();
            }
        }

        public string TargetName
        {
            get { return _targetName; }
        }

        public byte[] TargetInfo
        {
            get { return (byte[])_targetInfo.Clone(); }
        }

        // methods

        protected override void Decode(byte[] message)
        {
            base.Decode(message);

            Flags = (NtlmFlags)BitConverterLE.ToUInt32(message, 20);

            Buffer.BlockCopy(message, 24, _nonce, 0, 8);

            var tname_len = BitConverterLE.ToUInt16(message, 12);
            var tname_off = BitConverterLE.ToUInt16(message, 16);
            if (tname_len > 0)
            {
                if ((Flags & NtlmFlags.NegotiateOem) != 0)
                    _targetName = Encoding.ASCII.GetString(message, tname_off, tname_len);
                else
                    _targetName = Encoding.Unicode.GetString(message, tname_off, tname_len);
            }

            // The Target Info block is optional.
            if (message.Length >= 48)
            {
                var tinfo_len = BitConverterLE.ToUInt16(message, 40);
                var tinfo_off = BitConverterLE.ToUInt16(message, 44);
                if (tinfo_len > 0)
                {
                    _targetInfo = new byte[tinfo_len];
                    Buffer.BlockCopy(message, tinfo_off, _targetInfo, 0, tinfo_len);
                }
            }
        }

        public override byte[] GetBytes()
        {
            byte[] data = PrepareMessage(40);

            // message length
            short msg_len = (short)data.Length;
            data[16] = (byte)msg_len;
            data[17] = (byte)(msg_len >> 8);

            // flags
            data[20] = (byte)Flags;
            data[21] = (byte)((uint)Flags >> 8);
            data[22] = (byte)((uint)Flags >> 16);
            data[23] = (byte)((uint)Flags >> 24);

            Buffer.BlockCopy(_nonce, 0, data, 24, _nonce.Length);
            return data;
        }
    }
}
