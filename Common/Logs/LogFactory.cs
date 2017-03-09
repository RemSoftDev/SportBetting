using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLogger;
using SportRadar.Common.Windows;

namespace SportRadar.Common.Logs
{
    public enum eLogType
    {
        None = 0,
        Log4Net = 1,
        NLogger = 2
    }

    static class StaticHelper
    {

        public static Type GetCallingType()
        {

            var frame = new StackFrame(2, false);
            var type = frame.GetMethod().DeclaringType;


            return type;
        }
    }

    public class LogManager
    {
        public static ILog MyLogger()
        {
            var callingType = StaticHelper.GetCallingType();
            var logger = LogFactory.CreateLog(callingType);
            return logger;
        }

    }

    public static class LogFactory
    {
        public const eLogType CURRENT_LOG_TYPE = eLogType.NLogger;

        public static void Initialize()
        {
            switch (CURRENT_LOG_TYPE)
            {
                case eLogType.Log4Net:

                    //log4net.Config.XmlConfigurator.Configure();
                    break;

                case eLogType.NLogger:
                    break;
                case eLogType.None: ExcpHelper.ThrowIf(true, "LogFactory.Initialize() ERROR: Please specify Log Type.");

                default: ExcpHelper.ThrowIf(true, "LogFactory.Initialize() ERROR: Missed Log Type {0}", CURRENT_LOG_TYPE);
            }
        }

        public static SportRadar.Common.Logs.ILog CreateLog(Type classType)
        {

            switch (CURRENT_LOG_TYPE)
            {
                //case eLogType.Log4Net: return new Log4NetEnvelope(classType);
                case eLogType.NLogger:
                    return new NLoggerEnvelope(classType);

                case eLogType.None: ExcpHelper.ThrowIf(true, "LogFactory.CreateLog(classType='{0}') ERROR: Please specify Log Type.", classType);

                default: ExcpHelper.ThrowIf(true, "LogFactory.CreateLog(classType='{0}') ERROR: Missed Log Type {1}", classType, CURRENT_LOG_TYPE);
            }
        }

        public static string GetLogFilePath()
        {
            switch (CURRENT_LOG_TYPE)
            {
                case eLogType.NLogger:
                    var appender =
                        Log.Instance.Appenders.FirstOrDefault(x => x.GetType() == typeof(NLogger.Appenders.FileLoggerAppender));
                    if (appender != null)
                        return Path.GetDirectoryName(appender.Location);
                    break;
                case eLogType.Log4Net:

                    /*foreach (log4net.Appender.IAppender appender in log4net.LogManager.GetRepository().GetAppenders())
                    {
                        log4net.Appender.RollingFileAppender rfa = appender as log4net.Appender.RollingFileAppender;

                        if (rfa != null)
                        {
                            return Path.GetDirectoryName(rfa.File);
                        }*
                    }*/

                    break;

                case eLogType.None:

                    return string.Empty;

                default:

                    Debug.Assert(false);
                    break;
            }

            return string.Empty;
        }
    }

    internal class NLoggerEnvelope : ILog
    {
        internal protected NLoggerEnvelope(Type classType)
        {

        }



        public void Debug(string sInfo, params object[] args)
        {
            Log.DebugFormat(sInfo, args);
        }

        public void DebugFormat(string sInfo, params object[] args)
        {
            Debug(sInfo, args);
        }

        public void InfoFormat(string sInfo, params object[] args)
        {
            Info(sInfo, args);
        }


        public void Info(string sInfo, params object[] args)
        {
            Log.InfoFormat(sInfo, args);
        }



        public void Warn(string sInfo, params object[] args)
        {
            Log.WarningFormat(sInfo, args);
        }

        public void WarnFormat(string sInfo, params object[] args)
        {
            Warn(sInfo, args);
        }

        public void Error(string sInfo)
        {
            Log.Error(sInfo);
        }

        public void Error(string sInfo, Exception ex)
        {
            if (ex.InnerException != null)
            {
                Error(sInfo, ex.InnerException);
            }
            Log.Error(sInfo, ex);
        }

        public void Error(string sInfo, Exception ex, params object[] args)
        {

            Error(sInfo, ex);
            if (args.Length > 0)
                Log.ErrorFormat(sInfo, args);
        }

        public void Error(Exception ex, string sInfo, params object[] args)
        {
            Error(sInfo, ex, args);
        }

        public void ErrorFormat(string sInfo, Exception ex, params object[] args)
        {
            Error(sInfo, ex, args);
        }

        public void Error(string sInfo, params object[] args)
        {
            Error(sInfo);
            if (args.Length > 0)
                Log.ErrorFormat(sInfo, args);
        }

        public void Excp(Exception excp, string sInfo, params object[] args)
        {
            Log.Error(string.Format(sInfo, args), excp);
        }

        public void SetMaxSizeAndFileCount(int size, int logRotationCount)
        {
            Log.SetMaxSizeAndFileCount(size, logRotationCount);
        }

        public void Warning(Exception exception, string msg)
        {
            Error(msg, exception);
        }

        public void Warning(string msg)
        {
            Warn(msg);
        }

        public void Warning(string msg, params object[] args)
        {
            Warn(msg, args);
        }

        public void Debug(Exception sInfo, string msg, params object[] args)
        {
            Error(sInfo,msg,args);
        }

        public void Warning(Exception exception, string msg, params object[] args)
        {
            Error(exception,msg,args);
        }

        public void Debug(Exception exception)
        {
            Error(exception.Message,exception);
        }
    }
}
