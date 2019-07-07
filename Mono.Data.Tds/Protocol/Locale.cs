using System;
using System.Reflection;
using System.Resources;

namespace Mono.Data.Tds.Protocol
{
    internal static class Locale
    {
        #region Local Variables
        private static ResourceManager rm;
        #endregion   // Local Variables

        #region Constructors
        static Locale()
        {
            rm = new ResourceManager("System.Windows.Forms", Assembly.GetExecutingAssembly());
        }
        #endregion

        #region Static Properties
        public static ResourceManager ResourceManager
        {
            get
            {
                return rm;
            }
        }
        #endregion // Static Properties

        #region Static Methods
        public static string GetText(string msg)
        {
            string ret = ResourceManager.GetString(msg);
            if (ret != null)
                return ret;
            return msg;

            //                        string ret;

            //// This code and behaviour may change without notice. It's a placeholder until I
            //// understand how Miguels wants localization of strings done.
            //                        ret = (string)rm.GetObject(msg);
            //                        if (ret != null) {
            //                                return ret;
            //                        }
            //                        return msg;
        }

        public static string GetText(string msg, params object[] args)
        {
            return String.Format(GetText(msg), args);
        }

        //public static object GetResource(string name) {
        //        return rm.GetObject(name);
        //}
        #endregion // Static Methods
    }
}
