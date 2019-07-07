namespace Mono.Data.Tds.Protocol
{
    public sealed class TdsInternalErrorMessageEventArgs : TdsInternalInfoMessageEventArgs
    {
        #region Constructors

        public TdsInternalErrorMessageEventArgs(TdsInternalError error)
            : base(error)
        {
        }

        #endregion // Constructors
    }
}
