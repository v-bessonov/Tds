namespace Mono.Data.Tds.Protocol
{
    internal class TdsAsyncState
    {
        object _userState;
        bool _wantResults = false;

        public TdsAsyncState(object userState)
        {
            _userState = userState;
        }

        public object UserState
        {
            get { return _userState; }
            set { _userState = value; }
        }

        public bool WantResults
        {
            get { return _wantResults; }
            set { _wantResults = value; }
        }

    }
}
