﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Mono.Data.Tds.Protocol
{
    public class Tds70 : Tds
    {
        #region Fields

        //public readonly static TdsVersion Version = TdsVersion.tds70;
        static readonly decimal SMALLMONEY_MIN = -214748.3648m;
        static readonly decimal SMALLMONEY_MAX = 214748.3647m;

        #endregion // Fields

        #region Constructors

        [Obsolete("Use the constructor that receives a lifetime parameter")]
        public Tds70(string server, int port)
            : this(server, port, 512, 15, 0)
        {
        }

        [Obsolete("Use the constructor that receives a lifetime parameter")]
        public Tds70(string server, int port, int packetSize, int timeout)
            : this(server, port, packetSize, timeout, 0, TdsVersion.tds70)
        {
        }

        [Obsolete("Use the constructor that receives a lifetime parameter")]
        public Tds70(string server, int port, int packetSize, int timeout, TdsVersion version)
            : this(server, port, packetSize, timeout, 0, version)
        {
        }

        public Tds70(string server, int port, int lifetime)
            : this(server, port, 512, 15, lifetime)
        {
        }

        public Tds70(string server, int port, int packetSize, int timeout, int lifeTime)
            : base(server, port, packetSize, timeout, lifeTime, TdsVersion.tds70)
        {
        }

        public Tds70(string server, int port, int packetSize, int timeout, int lifeTime, TdsVersion version)
            : base(server, port, packetSize, timeout, lifeTime, version)
        {
        }

        #endregion // Constructors

        #region Properties

        protected virtual byte[] ClientVersion
        {
            get { return new byte[] { 0x00, 0x0, 0x0, 0x70 }; }
        }

        // Default precision is 28 for a 7.0 server. Unless and 
        // otherwise the server is started with /p option - which would be 38
        protected virtual byte Precision
        {
            get { return 28; }
        }

        #endregion // Properties

        #region Methods

        protected string BuildExec(string sql)
        {
            string esql = sql.Replace("'", "''"); // escape single quote
            if (Parameters != null && Parameters.Count > 0)
                return BuildProcedureCall(String.Format("sp_executesql N'{0}', N'{1}', ", esql, BuildPreparedParameters()));
            else
                return BuildProcedureCall(String.Format("sp_executesql N'{0}'", esql));
        }

        private string BuildParameters()
        {
            if (Parameters == null || Parameters.Count == 0)
                return String.Empty;

            StringBuilder result = new StringBuilder();
            foreach (TdsMetaParameter p in Parameters)
            {
                string parameterName = p.ParameterName;
                if (parameterName[0] == '@')
                {
                    parameterName = parameterName.Substring(1);
                }
                if (p.Direction != TdsParameterDirection.ReturnValue)
                {
                    if (result.Length > 0)
                        result.Append(", ");
                    if (p.Direction == TdsParameterDirection.InputOutput)
                        result.AppendFormat("@{0}={0} output", parameterName);
                    else
                        result.Append(FormatParameter(p));
                }
            }
            return result.ToString();
        }

        private string BuildPreparedParameters()
        {
            StringBuilder parms = new StringBuilder();
            foreach (TdsMetaParameter p in Parameters)
            {
                if (parms.Length > 0)
                    parms.Append(", ");

                // Set default precision according to the TdsVersion
                // Current default is 29 for Tds80 
                if (p.TypeName == "decimal")
                    p.Precision = (p.Precision != 0 ? p.Precision : (byte)Precision);

                parms.Append(p.Prepare());
                if (p.Direction == TdsParameterDirection.Output || p.Direction == TdsParameterDirection.InputOutput)
                    parms.Append(" output");
            }
            return parms.ToString();
        }

        private string BuildPreparedQuery(string id)
        {
            return BuildProcedureCall(String.Format("sp_execute {0},", id));
        }

        private string BuildProcedureCall(string procedure)
        {
            string exec = String.Empty;

            StringBuilder declare = new StringBuilder();
            StringBuilder select = new StringBuilder();
            StringBuilder set = new StringBuilder();

            int count = 0;
            if (Parameters != null)
            {
                foreach (TdsMetaParameter p in Parameters)
                {
                    string parameterName = p.ParameterName;
                    if (parameterName[0] == '@')
                    {
                        parameterName = parameterName.Substring(1);
                    }

                    if (p.Direction != TdsParameterDirection.Input)
                    {
                        if (count == 0)
                            select.Append("select ");
                        else
                            select.Append(", ");
                        select.Append("@" + parameterName);

                        if (p.TypeName == "decimal")
                            p.Precision = (p.Precision != 0 ? p.Precision : (byte)Precision);

                        declare.Append(String.Format("declare {0}\n", p.Prepare()));

                        if (p.Direction != TdsParameterDirection.ReturnValue)
                        {
                            if (p.Direction == TdsParameterDirection.InputOutput)
                                set.Append(String.Format("set {0}\n", FormatParameter(p)));
                            else
                                set.Append(String.Format("set @{0}=NULL\n", parameterName));
                        }

                        count++;
                    }
                    if (p.Direction == TdsParameterDirection.ReturnValue)
                        exec = "@" + parameterName + "=";
                }
            }
            exec = "exec " + exec;

            return String.Format("{0}{1}{2}{3} {4}\n{5}",
                declare.ToString(), set.ToString(), exec,
                procedure, BuildParameters(), select.ToString());
        }

        public override bool Connect(TdsConnectionParameters connectionParameters)
        {
            if (IsConnected)
                throw new InvalidOperationException("The connection is already open.");

            connectionParms = connectionParameters;

            SetLanguage(connectionParameters.Language);
            SetCharset("utf-8");

            byte[] empty = new byte[0];
            short authLen = 0;
            byte pad = (byte)0;

            byte[] domainMagic = { 6, 0x7d, 0x0f, 0xfd, 0xff, 0x0, 0x0, 0x0,
                                    0x0, 0xe0, 0x83, 0x0, 0x0,
                                    0x68, 0x01, 0x00, 0x00, 0x09, 0x04, 0x00, 0x00 };
            byte[] sqlserverMagic = { 6, 0x0, 0x0, 0x0,
                                        0x0, 0x0, 0x0, 0x0,
                                        0x0, 0xe0, 0x03, 0x0,
                                        0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
                                        0x0, 0x0, 0x0 };
            byte[] magic = null;

            if (connectionParameters.DomainLogin)
                magic = domainMagic;
            else
                magic = sqlserverMagic;

            string username = connectionParameters.User;
            string domain = null;

            int idx = username.IndexOf("\\");
            if (idx != -1)
            {
                domain = username.Substring(0, idx);
                username = username.Substring(idx + 1);

                connectionParameters.DefaultDomain = domain;
                connectionParameters.User = username;
            }
            else
            {
                domain = Environment.UserDomainName;
                connectionParameters.DefaultDomain = domain;
            }

            short partialPacketSize = (short)(86 + (
                connectionParameters.Hostname.Length +
                connectionParameters.ApplicationName.Length +
                DataSource.Length +
                connectionParameters.LibraryName.Length +
                Language.Length +
                connectionParameters.Database.Length +
                connectionParameters.AttachDBFileName.Length) * 2);

            if (connectionParameters.DomainLogin)
            {
                authLen = ((short)(32 + (connectionParameters.Hostname.Length +
                    domain.Length)));
                partialPacketSize += authLen;
            }
            else
                partialPacketSize += ((short)((username.Length + connectionParameters.Password.Length) * 2));

            int totalPacketSize = partialPacketSize;

            Comm.StartPacket(TdsPacketType.Logon70);

            Comm.Append(totalPacketSize);

            //Comm.Append (empty, 3, pad);
            //byte[] version = {0x00, 0x0, 0x0, 0x71};
            //Console.WriteLine ("Version: {0}", ClientVersion[3]);
            Comm.Append(ClientVersion); // TDS Version 7
            Comm.Append((int)this.PacketSize); // Set the Block Size
            Comm.Append(empty, 3, pad);
            Comm.Append(magic);

            short curPos = 86;

            // Hostname
            Comm.Append(curPos);
            Comm.Append((short)connectionParameters.Hostname.Length);
            curPos += (short)(connectionParameters.Hostname.Length * 2);

            if (connectionParameters.DomainLogin)
            {
                Comm.Append((short)0);
                Comm.Append((short)0);
                Comm.Append((short)0);
                Comm.Append((short)0);
            }
            else
            {
                // Username
                Comm.Append(curPos);
                Comm.Append((short)username.Length);
                curPos += ((short)(username.Length * 2));

                // Password
                Comm.Append(curPos);
                Comm.Append((short)connectionParameters.Password.Length);
                curPos += (short)(connectionParameters.Password.Length * 2);
            }

            // AppName
            Comm.Append(curPos);
            Comm.Append((short)connectionParameters.ApplicationName.Length);
            curPos += (short)(connectionParameters.ApplicationName.Length * 2);

            // Server Name
            Comm.Append(curPos);
            Comm.Append((short)DataSource.Length);
            curPos += (short)(DataSource.Length * 2);

            // Unknown
            Comm.Append((short)curPos);
            Comm.Append((short)0);

            // Library Name
            Comm.Append(curPos);
            Comm.Append((short)connectionParameters.LibraryName.Length);
            curPos += (short)(connectionParameters.LibraryName.Length * 2);

            // Language
            Comm.Append(curPos);
            Comm.Append((short)Language.Length);
            curPos += (short)(Language.Length * 2);

            // Database
            Comm.Append(curPos);
            Comm.Append((short)connectionParameters.Database.Length);
            curPos += (short)(connectionParameters.Database.Length * 2);

            // MAC Address
            Comm.Append((byte)0);
            Comm.Append((byte)0);
            Comm.Append((byte)0);
            Comm.Append((byte)0);
            Comm.Append((byte)0);
            Comm.Append((byte)0);

            // Authentication Stuff
            Comm.Append((short)curPos);
            if (connectionParameters.DomainLogin)
            {
                Comm.Append((short)authLen);
                curPos += (short)authLen;
            }
            else
                Comm.Append((short)0);

            // Unknown
            Comm.Append(curPos);
            Comm.Append((short)(connectionParameters.AttachDBFileName.Length));
            curPos += (short)(connectionParameters.AttachDBFileName.Length * 2);

            // Connection Parameters
            Comm.Append(connectionParameters.Hostname);
            if (!connectionParameters.DomainLogin)
            {
                // SQL Server Authentication
                Comm.Append(connectionParameters.User);
                string scrambledPwd = EncryptPassword(connectionParameters.Password);
                Comm.Append(scrambledPwd);
            }
            Comm.Append(connectionParameters.ApplicationName);
            Comm.Append(DataSource);
            Comm.Append(connectionParameters.LibraryName);
            Comm.Append(Language);
            Comm.Append(connectionParameters.Database);

            if (connectionParameters.DomainLogin)
            {
                // the rest of the packet is NTLMSSP authentication
                Type1Message msg = new Type1Message();
                msg.Domain = domain;
                msg.Host = connectionParameters.Hostname;
                msg.Flags = NtlmFlags.NegotiateUnicode |
                    NtlmFlags.NegotiateNtlm |
                    NtlmFlags.NegotiateDomainSupplied |
                    NtlmFlags.NegotiateWorkstationSupplied |
                    NtlmFlags.NegotiateAlwaysSign; // 0xb201
                Comm.Append(msg.GetBytes());
            }

            Comm.Append(connectionParameters.AttachDBFileName);
            Comm.SendPacket();
            MoreResults = true;
            SkipToEnd();

            return IsConnected;
        }

        private static string EncryptPassword(SecureString secPass)
        {
            int xormask = 0x5a5a;
            int len = secPass.Length;
            char[] chars = new char[len];
            string pass = GetPlainPassword(secPass);

            for (int i = 0; i < len; ++i)
            {
                int c = ((int)(pass[i])) ^ xormask;
                int m1 = (c >> 4) & 0x0f0f;
                int m2 = (c << 4) & 0xf0f0;
                chars[i] = (char)(m1 | m2);
            }

            return new String(chars);
        }

        public override bool Reset()
        {
            // Check validity of the connection - a false removes
            // the connection from the pool
            // NOTE: MS implementation will throw a connection-reset error as it will
            // try to use the same connection
            if (!Comm.IsConnected())
                return false;

            // Set "reset-connection" bit for the next message packet
            Comm.ResetConnection = true;
            base.Reset();
            return true;
        }

        public override void ExecPrepared(string commandText, TdsMetaParameterCollection parameters, int timeout, bool wantResults)
        {
            Parameters = parameters;
            ExecuteQuery(BuildPreparedQuery(commandText), timeout, wantResults);
        }

        public override void ExecProc(string commandText, TdsMetaParameterCollection parameters, int timeout, bool wantResults)
        {
            Parameters = parameters;
            ExecRPC(commandText, parameters, timeout, wantResults);
        }

        private void WriteRpcParameterInfo(TdsMetaParameterCollection parameters)
        {
            if (parameters != null)
            {
                foreach (TdsMetaParameter param in parameters)
                {
                    if (param.Direction == TdsParameterDirection.ReturnValue)
                        continue;
                    string pname = param.ParameterName;
                    if (pname != null && pname.Length > 0 && pname[0] == '@')
                    {
                        Comm.Append((byte)pname.Length);
                        Comm.Append(pname);
                    }
                    else
                    {
                        Comm.Append((byte)(pname.Length + 1));
                        Comm.Append("@" + pname);
                    }
                    short status = 0; // unused
                    if (param.Direction != TdsParameterDirection.Input)
                        status |= 0x01; // output
                    Comm.Append((byte)status);
                    WriteParameterInfo(param);
                }
            }
        }

        private void WritePreparedParameterInfo(TdsMetaParameterCollection parameters)
        {
            if (parameters == null)
                return;

            string param = BuildPreparedParameters();
            Comm.Append((byte)0x00); // no param meta data name
            Comm.Append((byte)0x00); // no status flags

            // Type_info - parameter info
            WriteParameterInfo(new TdsMetaParameter("prep_params",
                                                      param.Length > 4000 ? "ntext" : "nvarchar",
                                                      param));
        }

        protected void ExecRPC(TdsRpcProcId rpcId, string sql,
                                TdsMetaParameterCollection parameters,
                                int timeout, bool wantResults)
        {
            // clean up
            InitExec();
            Comm.StartPacket(TdsPacketType.RPC);

            Comm.Append((ushort)0xFFFF);
            Comm.Append((ushort)rpcId);
            Comm.Append((short)0x02); // no meta data

            Comm.Append((byte)0x00); // no param meta data name
            Comm.Append((byte)0x00); // no status flags

            // Convert BigNVarChar values larger than 4000 chars to nvarchar(max)
            // Need to do this here so WritePreparedParameterInfo emit the
            // correct data type
            foreach (TdsMetaParameter param2 in parameters)
            {
                var colType = param2.GetMetaType();

                if (colType == TdsColumnType.BigNVarChar)
                {
                    int size = param2.GetActualSize();
                    if ((size >> 1) > 4000)
                        param2.Size = -1;
                }
            }

            // Write sql as a parameter value - UCS2
            TdsMetaParameter param = new TdsMetaParameter("sql",
                                                           sql.Length > 4000 ? "ntext" : "nvarchar",
                                                           sql);
            WriteParameterInfo(param);

            // Write Parameter infos - name and type
            WritePreparedParameterInfo(parameters);

            // Write parameter/value info
            WriteRpcParameterInfo(parameters);
            Comm.SendPacket();
            CheckForData(timeout);
            if (!wantResults)
                SkipToEnd();
        }

        protected override void ExecRPC(string rpcName, TdsMetaParameterCollection parameters,
                         int timeout, bool wantResults)
        {
            // clean up
            InitExec();
            Comm.StartPacket(TdsPacketType.RPC);

            Comm.Append((short)rpcName.Length);
            Comm.Append(rpcName);
            Comm.Append((short)0); //no meta data
            WriteRpcParameterInfo(parameters);
            Comm.SendPacket();
            CheckForData(timeout);
            if (!wantResults)
                SkipToEnd();
        }

        private void WriteParameterInfo(TdsMetaParameter param)
        {
            /*
			Ms.net send non-nullable datatypes as nullable and allows setting null values
			to int/float etc.. So, using Nullable form of type for all data
			*/
            param.IsNullable = true;
            TdsColumnType colType = param.GetMetaType();
            param.IsNullable = false;

            bool partLenType = false;
            int size = param.Size;
            if (size < 1)
            {
                if (size < 0)
                    partLenType = true;
                size = param.GetActualSize();
            }

            /*
			 * If the value is null, not setting the size to 0 will cause varchar
			 * fields to get inserted as an empty string rather than an null.
			 */
            if (colType != TdsColumnType.IntN && colType != TdsColumnType.DateTimeN)
            {
                if (param.Value == null || param.Value == DBNull.Value)
                    size = 0;
            }

            // Change colType according to the following table
            /* 
			 * Original Type	Maxlen		New Type 
			 * 
			 * NVarChar		4000 UCS2	NText
			 * BigVarChar		8000 ASCII	Text
			 * BigVarBinary		8000 bytes	Image
			 * 
			 */
            TdsColumnType origColType = colType;
            if (colType == TdsColumnType.BigNVarChar)
            {
                // param.GetActualSize() returns len*2
                if (size == param.Size)
                    size <<= 1;
                if ((size >> 1) > 4000)
                    colType = TdsColumnType.NText;
            }
            else if (colType == TdsColumnType.BigVarChar)
            {
                if (size > 8000)
                    colType = TdsColumnType.Text;
            }
            else if (colType == TdsColumnType.BigVarBinary)
            {
                if (size > 8000)
                    colType = TdsColumnType.Image;
            }
            else if (colType == TdsColumnType.DateTime2 ||
                 colType == TdsColumnType.DateTimeOffset)
            {
                // HACK: Wire-level DateTime{2,Offset}
                // require TDS 7.3, which this driver
                // does not implement correctly--so we
                // serialize to ASCII instead.
                colType = TdsColumnType.Char;
            }
            // Calculation of TypeInfo field
            /* 
			 * orig size value		TypeInfo field
			 * 
			 * >= 0 <= Maxlen		origColType + content len
			 * > Maxlen		NewType as per above table + content len
			 * -1		origColType + USHORTMAXLEN (0xFFFF) + content len (TDS 9)
			 * 
			 */
            // Write updated colType, iff partLenType == false
            if (TdsVersion > TdsVersion.tds81 && partLenType)
            {
                Comm.Append((byte)origColType);
                Comm.Append((short)-1);
            }
            else if (ServerTdsVersion > TdsVersion.tds70
                     && origColType == TdsColumnType.Decimal)
            {
                Comm.Append((byte)TdsColumnType.Numeric);
            }
            else
            {
                Comm.Append((byte)colType);
            }

            if (IsLargeType(colType))
                Comm.Append((short)size); // Parameter size passed in SqlParameter
            else if (IsBlobType(colType))
                Comm.Append(size); // Parameter size passed in SqlParameter
            else
                Comm.Append((byte)size);

            // Precision and Scale are non-zero for only decimal/numeric
            if (param.TypeName == "decimal" || param.TypeName == "numeric")
            {
                Comm.Append((param.Precision != 0) ? param.Precision : Precision);
                Comm.Append(param.Scale);
                // Convert the decimal value according to Scale
                if (param.Value != null && param.Value != DBNull.Value &&
                    ((decimal)param.Value) != Decimal.MaxValue &&
                    ((decimal)param.Value) != Decimal.MinValue &&
                    ((decimal)param.Value) != long.MaxValue &&
                    ((decimal)param.Value) != long.MinValue &&
                    ((decimal)param.Value) != ulong.MaxValue &&
                    ((decimal)param.Value) != ulong.MinValue)
                {
                    long expo = (long)new Decimal(System.Math.Pow(10, (double)param.Scale));
                    long pVal = (long)(((decimal)param.Value) * expo);
                    param.Value = pVal;
                }
            }


            /* VARADHAN: TDS 8 Debugging */
            /*
			if (Collation != null) {
				Console.WriteLine ("Collation is not null");
				Console.WriteLine ("Column Type: {0}", colType);
				Console.WriteLine ("Collation bytes: {0} {1} {2} {3} {4}", Collation[0], Collation[1], Collation[2],
				                   Collation[3], Collation[4]);
			} else {
				Console.WriteLine ("Collation is null");
			}
			*/

            // Tds > 7.0 uses collation
            if (Collation != null &&
                (colType == TdsColumnType.BigChar || colType == TdsColumnType.BigNVarChar ||
                 colType == TdsColumnType.BigVarChar || colType == TdsColumnType.NChar ||
                 colType == TdsColumnType.NVarChar || colType == TdsColumnType.Text ||
                 colType == TdsColumnType.NText))
                Comm.Append(Collation);

            // LAMESPEC: size should be 0xFFFF for any bigvarchar, bignvarchar and bigvarbinary 
            // types if param value is NULL
            if ((colType == TdsColumnType.BigVarChar ||
                 colType == TdsColumnType.BigNVarChar ||
                 colType == TdsColumnType.BigVarBinary ||
                 colType == TdsColumnType.Image) &&
                (param.Value == null || param.Value == DBNull.Value))
                size = -1;
            else
                size = param.GetActualSize();

            if (IsLargeType(colType))
                Comm.Append((short)size);
            else if (IsBlobType(colType))
                Comm.Append(size);
            else
                Comm.Append((byte)size);

            if (size > 0)
            {
                switch (param.TypeName)
                {
                    case "money":
                        {
                            // 4 == SqlMoney::MoneyFormat.NumberDecimalDigits
                            Decimal val = Decimal.Round((decimal)param.Value, 4);
                            int[] arr = Decimal.GetBits(val);

                            if (val >= 0)
                            {
                                Comm.Append(arr[1]);
                                Comm.Append(arr[0]);
                            }
                            else
                            {
                                Comm.Append(~arr[1]);
                                Comm.Append(~arr[0] + 1);
                            }
                            break;
                        }
                    case "smallmoney":
                        {
                            // 4 == SqlMoney::MoneyFormat.NumberDecimalDigits
                            Decimal val = Decimal.Round((decimal)param.Value, 4);
                            if (val < SMALLMONEY_MIN || val > SMALLMONEY_MAX)
                                throw new OverflowException(string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Value '{0}' is not valid for SmallMoney."
                                    + "  Must be between {1:N4} and {2:N4}.",
                                    val,
                                    SMALLMONEY_MIN, SMALLMONEY_MAX));

                            int[] arr = Decimal.GetBits(val);
                            int sign = (val > 0 ? 1 : -1);
                            Comm.Append(sign * arr[0]);
                            break;
                        }
                    case "datetime":
                        Comm.Append((DateTime)param.Value, 8);
                        break;
                    case "smalldatetime":
                        Comm.Append((DateTime)param.Value, 4);
                        break;
                    case "varchar":
                    case "nvarchar":
                    case "char":
                    case "nchar":
                    case "text":
                    case "ntext":
                    case "datetime2":
                    case "datetimeoffset":
                        byte[] tmp = param.GetBytes();
                        Comm.Append(tmp);
                        break;
                    case "uniqueidentifier":
                        Comm.Append(((Guid)param.Value).ToByteArray());
                        break;
                    default:
                        Comm.Append(param.Value);
                        break;
                }
            }
            return;
        }

        public override void Execute(string commandText, TdsMetaParameterCollection parameters, int timeout, bool wantResults)
        {
            Parameters = parameters;
            string sql = commandText;
            if (wantResults || (Parameters != null && Parameters.Count > 0))
                sql = BuildExec(commandText);
            ExecuteQuery(sql, timeout, wantResults);
        }

        private string FormatParameter(TdsMetaParameter parameter)
        {
            string parameterName = parameter.ParameterName;
            if (parameterName[0] == '@')
            {
                parameterName = parameterName.Substring(1);
            }
            if (parameter.Direction == TdsParameterDirection.Output)
                return String.Format("@{0}=@{0} output", parameterName);
            if (parameter.Value == null || parameter.Value == DBNull.Value)
                return String.Format("@{0}=NULL", parameterName);

            string value = null;
            switch (parameter.TypeName)
            {
                case "smalldatetime":
                case "datetime":
                    DateTime d = Convert.ToDateTime(parameter.Value);
                    value = String.Format(base.Locale,
                        "'{0:MMM dd yyyy hh:mm:ss.fff tt}'", d);
                    break;
                case "bigint":
                case "decimal":
                case "float":
                case "int":
                case "money":
                case "real":
                case "smallint":
                case "smallmoney":
                case "tinyint":
                    object paramValue = parameter.Value;
                    Type paramType = paramValue.GetType();
                    if (paramType.IsEnum)
                        paramValue = Convert.ChangeType(paramValue,
                            Type.GetTypeCode(paramType));
                    value = paramValue.ToString();
                    break;
                case "nvarchar":
                case "nchar":
                    value = String.Format("N'{0}'", parameter.Value.ToString().Replace("'", "''"));
                    break;
                case "uniqueidentifier":
                    value = String.Format("'{0}'", ((Guid)parameter.Value).ToString(string.Empty));
                    break;
                case "bit":
                    if (parameter.Value.GetType() == typeof(bool))
                        value = (((bool)parameter.Value) ? "0x1" : "0x0");
                    else
                        value = parameter.Value.ToString();
                    break;
                case "image":
                case "binary":
                case "varbinary":
                    byte[] byteArray = (byte[])parameter.Value;
                    // In 1.0 profile, BitConverter.ToString() throws ArgumentOutOfRangeException when passed a 0-length
                    // array, so handle that as a special case.
                    if (byteArray.Length == 0)
                        value = "0x";
                    else
                        value = String.Format("0x{0}", BitConverter.ToString(byteArray).Replace("-", string.Empty).ToLower());
                    break;
                default:
                    value = String.Format("'{0}'", parameter.Value.ToString().Replace("'", "''"));
                    break;
            }

            return "@" + parameterName + "=" + value;
        }

        public override string Prepare(string commandText, TdsMetaParameterCollection parameters)
        {
            Parameters = parameters;

            TdsMetaParameterCollection parms = new TdsMetaParameterCollection();
            // Tested with MS SQL 2008 RC2 Express and MS SQL 2012 Express:
            // You may pass either -1 or 0, but not null as initial value of @Handle,
            // which is an output parameter.
            TdsMetaParameter parm = new TdsMetaParameter("@Handle", "int", -1);
            parm.Direction = TdsParameterDirection.Output;
            parms.Add(parm);

            parms.Add(new TdsMetaParameter("@VarDecl", "nvarchar", BuildPreparedParameters()));
            parms.Add(new TdsMetaParameter("@Query", "nvarchar", commandText));

            ExecProc("sp_prepare", parms, 0, true);
            SkipToEnd();
            return OutputParameters[0].ToString();
            //if (ColumnValues == null || ColumnValues [0] == null || ColumnValues [0] == DBNull.Value)
            //	throw new TdsInternalException ();
            //return string.Empty;
            //return ColumnValues [0].ToString ();
        }

        protected override void ProcessColumnInfo()
        {
            int numColumns = Comm.GetTdsShort();
            for (int i = 0; i < numColumns; i += 1)
            {
                byte[] flagData = new byte[4];
                for (int j = 0; j < 4; j += 1)
                    flagData[j] = Comm.GetByte();

                bool nullable = (flagData[2] & 0x01) > 0;
                //bool caseSensitive = (flagData[2] & 0x02) > 0;
                bool writable = (flagData[2] & 0x0c) > 0;
                bool autoIncrement = (flagData[2] & 0x10) > 0;
                bool isIdentity = (flagData[2] & 0x10) > 0;

                TdsColumnType columnType = (TdsColumnType)((Comm.GetByte() & 0xff));

                byte xColumnType = 0;
                if (IsLargeType(columnType))
                {
                    xColumnType = (byte)columnType;
                    if (columnType != TdsColumnType.NChar)
                        columnType -= 128;
                }

                int columnSize;
                string tableName = null;

                if (IsBlobType(columnType))
                {
                    columnSize = Comm.GetTdsInt();
                    tableName = Comm.GetString(Comm.GetTdsShort());
                }
                else if (IsFixedSizeColumn(columnType))
                {
                    columnSize = LookupBufferSize(columnType);
                }
                else if (IsLargeType((TdsColumnType)xColumnType))
                {
                    columnSize = Comm.GetTdsShort();
                }
                else
                {
                    columnSize = Comm.GetByte() & 0xff;
                }

                if (IsWideType((TdsColumnType)columnType))
                    columnSize /= 2;

                byte precision = 0;
                byte scale = 0;

                if (columnType == TdsColumnType.Decimal || columnType == TdsColumnType.Numeric)
                {
                    precision = Comm.GetByte();
                    scale = Comm.GetByte();
                }
                else
                {
                    precision = GetPrecision(columnType, columnSize);
                    scale = GetScale(columnType, columnSize);
                }

                string columnName = Comm.GetString(Comm.GetByte());

                TdsDataColumn col = new TdsDataColumn();
                Columns.Add(col);
                col.ColumnType = columnType;
                col.ColumnName = columnName;
                col.IsAutoIncrement = autoIncrement;
                col.IsIdentity = isIdentity;
                col.ColumnSize = columnSize;
                col.NumericPrecision = precision;
                col.NumericScale = scale;
                col.IsReadOnly = !writable;
                col.AllowDBNull = nullable;
                col.BaseTableName = tableName;
                col.DataTypeName = Enum.GetName(typeof(TdsColumnType), xColumnType);
            }
        }

        public override void Unprepare(string statementId)
        {
            TdsMetaParameterCollection parms = new TdsMetaParameterCollection();
            parms.Add(new TdsMetaParameter("@P1", "int", Int32.Parse(statementId)));
            ExecProc("sp_unprepare", parms, 0, false);
        }

        protected override bool IsValidRowCount(byte status, byte op)
        {
            if ((status & (byte)0x10) == 0 || op == (byte)0xc1)
                return false;
            return true;
        }

        protected override void ProcessReturnStatus()
        {
            int result = Comm.GetTdsInt();
            if (Parameters != null)
            {
                foreach (TdsMetaParameter param in Parameters)
                {
                    if (param.Direction == TdsParameterDirection.ReturnValue)
                    {
                        param.Value = result;
                        break;
                    }
                }
            }
        }

        byte GetScale(TdsColumnType type, int columnSize)
        {
            switch (type)
            {
                case TdsColumnType.DateTime:
                    return 0x03;
                case TdsColumnType.DateTime4:
                    return 0x00;
                case TdsColumnType.DateTimeN:
                    switch (columnSize)
                    {
                        case 4:
                            return 0x00;
                        case 8:
                            return 0x03;
                    }
                    break;
                default:
                    return 0xff;
            }

            throw new NotSupportedException(string.Format(
                CultureInfo.InvariantCulture,
                "Fixed scale not defined for column " +
                "type '{0}' with size {1}.", type, columnSize));
        }

        byte GetPrecision(TdsColumnType type, int columnSize)
        {
            switch (type)
            {
                case TdsColumnType.Binary:
                    return 0xff;
                case TdsColumnType.Bit:
                    return 0xff;
                case TdsColumnType.Char:
                    return 0xff;
                case TdsColumnType.DateTime:
                    return 0x17;
                case TdsColumnType.DateTime4:
                    return 0x10;
                case TdsColumnType.DateTimeN:
                    switch (columnSize)
                    {
                        case 4:
                            return 0x10;
                        case 8:
                            return 0x17;
                    }
                    break;
                case TdsColumnType.Real:
                    return 0x07;
                case TdsColumnType.Float8:
                    return 0x0f;
                case TdsColumnType.FloatN:
                    switch (columnSize)
                    {
                        case 4:
                            return 0x07;
                        case 8:
                            return 0x0f;
                    }
                    break;
                case TdsColumnType.Image:
                    return 0xff;
                case TdsColumnType.Int1:
                    return 0x03;
                case TdsColumnType.Int2:
                    return 0x05;
                case TdsColumnType.Int4:
                    return 0x0a;
                case TdsColumnType.IntN:
                    switch (columnSize)
                    {
                        case 1:
                            return 0x03;
                        case 2:
                            return 0x05;
                        case 4:
                            return 0x0a;
                    }
                    break;
                case TdsColumnType.Void:
                    return 0x01;
                case TdsColumnType.Text:
                    return 0xff;
                case TdsColumnType.UniqueIdentifier:
                    return 0xff;
                case TdsColumnType.VarBinary:
                    return 0xff;
                case TdsColumnType.VarChar:
                    return 0xff;
                case TdsColumnType.Money:
                    return 19;
                case TdsColumnType.NText:
                    return 0xff;
                case TdsColumnType.NVarChar:
                    return 0xff;
                case TdsColumnType.BitN:
                    return 0xff;
                case TdsColumnType.MoneyN:
                    switch (columnSize)
                    {
                        case 4:
                            return 0x0a;
                        case 8:
                            return 0x13;
                    }
                    break;
                case TdsColumnType.Money4:
                    return 0x0a;
                case TdsColumnType.NChar:
                    return 0xff;
                case TdsColumnType.BigBinary:
                    return 0xff;
                case TdsColumnType.BigVarBinary:
                    return 0xff;
                case TdsColumnType.BigVarChar:
                    return 0xff;
                case TdsColumnType.BigNVarChar:
                    return 0xff;
                case TdsColumnType.BigChar:
                    return 0xff;
                case TdsColumnType.SmallMoney:
                    return 0x0a;
                case TdsColumnType.Variant:
                    return 0xff;
                case TdsColumnType.BigInt:
                    return 0xff;
            }

            throw new NotSupportedException(string.Format(
                CultureInfo.InvariantCulture,
                "Fixed precision not defined for column " +
                "type '{0}' with size {1}.", type, columnSize));
        }

        #endregion // Methods

        #region Asynchronous Methods

        public override IAsyncResult BeginExecuteNonQuery(string cmdText,
                                                          TdsMetaParameterCollection parameters,
                                                          AsyncCallback callback,
                                                          object state)
        {
            Parameters = parameters;
            string sql = cmdText;
            if (Parameters != null && Parameters.Count > 0)
                sql = BuildExec(cmdText);

            IAsyncResult ar = BeginExecuteQueryInternal(sql, false, callback, state);
            return ar;
        }

        public override void EndExecuteNonQuery(IAsyncResult ar)
        {
            EndExecuteQueryInternal(ar);
        }

        public override IAsyncResult BeginExecuteQuery(string cmdText,
                                                                TdsMetaParameterCollection parameters,
                                                                AsyncCallback callback,
                                                                object state)
        {
            Parameters = parameters;
            string sql = cmdText;
            if (Parameters != null && Parameters.Count > 0)
                sql = BuildExec(cmdText);

            IAsyncResult ar = BeginExecuteQueryInternal(sql, true, callback, state);
            return ar;
        }

        public override void EndExecuteQuery(IAsyncResult ar)
        {
            EndExecuteQueryInternal(ar);
        }

        public override IAsyncResult BeginExecuteProcedure(string prolog,
                                                                    string epilog,
                                                                    string cmdText,
                                                                    bool IsNonQuery,
                                                                    TdsMetaParameterCollection parameters,
                                                                    AsyncCallback callback,
                                                                    object state)
        {
            Parameters = parameters;
            string pcall = BuildProcedureCall(cmdText);
            string sql = String.Format("{0};{1};{2};", prolog, pcall, epilog);

            IAsyncResult ar = BeginExecuteQueryInternal(sql, !IsNonQuery, callback, state);
            return ar;
        }

        public override void EndExecuteProcedure(IAsyncResult ar)
        {
            EndExecuteQueryInternal(ar);
        }

        #endregion // Asynchronous Methods
    }
}
