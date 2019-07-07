﻿namespace Mono.Data.Tds.Protocol
{
    public class TdsBigDecimal
    {
        #region Fields

        bool isNegative;
        byte precision;
        byte scale;
        int[] data;

        #endregion // Fields

        #region Constructors

        public TdsBigDecimal(byte precision, byte scale, bool isNegative, int[] data)
        {
            this.isNegative = isNegative;
            this.precision = precision;
            this.scale = scale;
            this.data = data;
        }

        #endregion // Constructors

        #region Properties

        public int[] Data
        {
            get { return data; }
        }

        public byte Precision
        {
            get { return precision; }
        }

        public byte Scale
        {
            get { return scale; }
        }

        public bool IsNegative
        {
            get { return isNegative; }
        }

        #endregion // Properties
    }
}
