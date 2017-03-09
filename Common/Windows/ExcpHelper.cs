using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportRadar.Common.Windows
{
    public static class ExcpHelper
    {
        private static void ThrowException<T>(Exception excpInner, string sFormat, params object[] args) where T : System.Exception
        {
            string sMessage = sFormat;

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    object arg = args[i];
                    sMessage = sMessage.Replace("{" + i + "}", arg.ToString());
                }
            }

            T tObj = (T)System.Activator.CreateInstance(typeof(T), sMessage, excpInner);

            throw tObj;
        }

        public static void ThrowIf<T>(bool bCondition, string sFormat, params object[] args) where T : System.Exception
        {
            if (bCondition)
            {
                ThrowException<T>(null, sFormat, args);
            }
        }

        public static void ThrowIf(bool bCondition, string sFormat, params object[] args)
        {
            if (bCondition)
            {
                ThrowException<System.Exception>(null, sFormat, args);
            }
        }

        public static void ThrowUp<T>(Exception excpInner, string sFormat, params object[] args) where T : System.Exception
        {
            ThrowException<T>(excpInner, sFormat, args);
        }

        public static void ThrowUp(Exception excpInner, string sFormat, params object[] args)
        {
            ThrowException<System.Exception>(excpInner, sFormat, args);
        }

        public static void RecursivelyFormatInnerException(System.Exception excp, ref string sFormatInnerException)
        {
            if (excp.InnerException != null)
            {
                sFormatInnerException += string.Format("<!--SportRadar Inner Exception Delimeter-->//\r\nInner Exception:{0}\r\n{1}\r\n", excp.InnerException.Message, excp.InnerException.StackTrace);

                RecursivelyFormatInnerException(excp.InnerException, ref sFormatInnerException);
            }
        }

        public static string FormatException(Exception excp, string sMessageFormat, params object[] args)
        {
            string sMessage = string.Format(sMessageFormat, args);

            string sResultFormat = @"
{0}:{1}
{2}

{3}
";

            string sInnerException = string.Empty;
            RecursivelyFormatInnerException(excp, ref sInnerException);

            return string.Format(sResultFormat, sMessage, excp.Message, excp.StackTrace, sInnerException);
        }
    }
}
