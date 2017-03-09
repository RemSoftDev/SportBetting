using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportRadar.Common.Logs
{
    public interface ILog
    {
        void Debug(string sInfo, params object[] args);
        void DebugFormat(string sInfo, params object[] args);
        void InfoFormat(string sInfo, params object[] args);
        void Info(string sInfo, params object[] args);
        void Warn(string sInfo, params object[] args);
        void WarnFormat(string sInfo, params object[] args);
        void Error(string sInfo,Exception ex, params object[] args);
        void Error(Exception ex,string sInfo, params object[] args);
        void ErrorFormat(string sInfo,Exception ex, params object[] args);
        void Error(string sInfo,params object[] args);
        void Excp(System.Exception excp, string sInfo, params object[] args);
        void SetMaxSizeAndFileCount(int size, int logRotationCount);
        void Warning(Exception exception, string msg);
        void Warning(string msg);
        void Warning(string msg,params object[] args);
        void Debug(Exception sInfo, string msg, params object[] args);
        void Warning(Exception exception, string msg, params object[] args);
        void Debug(Exception exception);
    }
}
