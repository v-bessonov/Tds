using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Odbc
{
    partial class OdbcFactory
    {
        public override CodeAccessPermission CreatePermission(PermissionState state) =>
            new OdbcPermission(state);
    }
}
