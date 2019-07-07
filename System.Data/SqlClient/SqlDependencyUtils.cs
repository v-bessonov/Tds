namespace System.Data.SqlClient
{
    partial class SqlDependencyPerAppDomainDispatcher
    {
        private void SubscribeToAppDomainUnload()
        {
            //override corefx behavior with no-op.
        }
    }
}
