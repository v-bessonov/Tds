using System;

namespace Mono.Data.Tds.Protocol
{
    public abstract class MessageBase
    {

        static private byte[] header = { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00 };

        private int _type;
        private NtlmFlags _flags;

        protected MessageBase(int messageType)
        {
            _type = messageType;
        }

        public NtlmFlags Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public int Type
        {
            get { return _type; }
        }

        protected byte[] PrepareMessage(int messageSize)
        {
            byte[] message = new byte[messageSize];
            Buffer.BlockCopy(header, 0, message, 0, 8);

            message[8] = (byte)_type;
            message[9] = (byte)(_type >> 8);
            message[10] = (byte)(_type >> 16);
            message[11] = (byte)(_type >> 24);

            return message;
        }

        protected virtual void Decode(byte[] message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            if (message.Length < 12)
            {
                string msg = Locale.GetText("Minimum message length is 12 bytes.");
                throw new ArgumentOutOfRangeException("message", message.Length, msg);
            }

            if (!CheckHeader(message))
            {
                string msg = String.Format(Locale.GetText("Invalid Type{0} message."), _type);
                throw new ArgumentException(msg, "message");
            }
        }


        protected bool CheckHeader(byte[] message)
        {
            for (int i = 0; i < header.Length; i++)
            {
                if (message[i] != header[i])
                    return false;
            }
            return (BitConverterLE.ToUInt32(message, 8) == _type);
        }

        public abstract byte[] GetBytes();
    }
}
