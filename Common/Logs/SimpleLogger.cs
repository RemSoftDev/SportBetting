using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SportRadar.Common.Collections;
using SportRadar.Common.Windows;

namespace SportRadar.Common.Logs
{
    public enum eLoggerLevel
    {
        NONE = 0,
        DEBUG = 1,
        VERBOSE = 2,
        INFO = 4,
        WARN = 8,
        ERROR = 32,
        CRITICAL = 64,
    }

    public sealed class LoggerRecord
    {
        public Type ObjectType { get; private set; }
        public eLoggerLevel LoggerLevel { get; private set; }
        public DateTime Time { get; private set; }
        public string Message { get; private set; }
        public object[] Args { get; private set; }

        public LoggerRecord(Type type, eLoggerLevel level, string sMessage, object[] args)
        {
            this.ObjectType = type;
            this.LoggerLevel = level;
            this.Message = sMessage;
            this.Args = args;

            this.Time = DateTime.UtcNow;
        }
    }


    public class SimpleLogger
    {
        public const int DEFAULT_MAX_KILOBYTE_SIZE = 32768;
        protected const int KILOBYTE = 1024;

        protected string m_sThreadName = null;
        protected string m_sLogleFilePath = null;
        protected int m_lMaxSize = 0;
        protected int m_iAllowedLevels = 0;

        protected ThreadContext m_tc = null;

        protected SyncQueue<LoggerRecord> m_sqMessages = new SyncQueue<LoggerRecord>();

        public static SimpleLogger MainLogger { get; private set; }

        static SimpleLogger()
        {
            string sFilePath         = ConfigurationManager.AppSettings["simple_logger_file_path"];
            string sLoggerLevels     = ConfigurationManager.AppSettings["simple_logger_levels"];
            string sMaxKilobyteSize  = ConfigurationManager.AppSettings["simple_logger_max_kilobyte_size"];

            if (sLoggerLevels != null && !string.IsNullOrEmpty(sMaxKilobyteSize))
            {
                SimpleLogger.MainLogger = new SimpleLogger(sFilePath, sLoggerLevels, sMaxKilobyteSize);
            }
            else
            {
                SimpleLogger.MainLogger = new SimpleLogger(); // Mostly for design type
            }
        }

        private SimpleLogger()
        {
            
        }

        public void UpdateLoggerLevels(string sLoggerLevels)
        {
            string[] arrLevels = sLoggerLevels.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            int iAllowedLevels = 0;

            foreach (string sLevel in arrLevels)
            {
                try
                {
                    eLoggerLevel level = (eLoggerLevel)Enum.Parse(typeof(eLoggerLevel), sLevel, true);
                    iAllowedLevels |= (int)level;
                }
                catch (Exception excp)
                {
                    ExcpHelper.ThrowIf(true, "SimpleLogger init ERROR: Cannot get LoggerLevel from {0}", sLevel);
                }
            }

            m_iAllowedLevels = iAllowedLevels;
        }

        public void UpdateMaxKilobyteSize(string sMaxKilobyteSize)
        {
            int iMaxKilobyteSize = DEFAULT_MAX_KILOBYTE_SIZE;
            if (!string.IsNullOrEmpty(sMaxKilobyteSize))
            {
                ExcpHelper.ThrowIf(!int.TryParse(sMaxKilobyteSize, out iMaxKilobyteSize), "SimpleLogger init ERROR: Cannot get MaxKilobyteSize param.");
            }

            m_lMaxSize = iMaxKilobyteSize * KILOBYTE;
        }

        public SimpleLogger(string sLogleFilePath, string sLoggerLevels, string sMaxKilobyteSize)
        {
            m_sLogleFilePath = sLogleFilePath;
            m_sThreadName = sLogleFilePath.Replace(":", "__").Replace(@"\", "_");
            UpdateLoggerLevels(sLoggerLevels);
            UpdateMaxKilobyteSize(sMaxKilobyteSize);
        }

        public string LogleFilePath
        {
            get
            {
                return m_sLogleFilePath;
            }
        }

        public void Initialize()
        {
            ThreadHelper.ThreadStarted += ThreadHelperOnThreadStarted;
            ThreadHelper.ThreadCompleted += ThreadHelper_ThreadCompleted;

            m_tc = ThreadHelper.RunThread(m_sThreadName, LoggerThread);
        }

        void ThreadHelper_ThreadCompleted(ThreadContext tc)
        {
            if (m_tc.ManagedThreadId == tc.ManagedThreadId && !tc.IsToStop)
            {
                m_tc = ThreadHelper.RunThread(m_sThreadName, LoggerThread);
            }
        }

        private void ThreadHelperOnThreadStarted(ThreadContext tc)
        {
        }

        public void Stop()
        {
            m_tc.RequestToStop();
        }

        public void Write(Type type, eLoggerLevel level, string sMessage, object[] args)
        {
            int iLevel = (int)level;

            if ((iLevel & m_iAllowedLevels) > 0)
            {
                try
                {
                    LoggerRecord record = new LoggerRecord(type, level, sMessage, args);
                    m_sqMessages.Enqueue(record);
                }
                catch (Exception excp)
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine(ExcpHelper.FormatException(excp, "LiveLog.WriteConsole('{0}') ERROR", m_sLogleFilePath));
                }
            }
        }

        protected void LoggerThread(ThreadContext tc)
        {
            FileInfo fi = new FileInfo(m_sLogleFilePath);

            StringBuilder sb = new StringBuilder();

            try
            {
                while (!tc.IsToStop)
                {
                    if (fi.Exists && fi.Length > m_lMaxSize)
                    {
                        string sExtension = Path.GetExtension(m_sLogleFilePath);
                        string sNewFileName = string.Format("{0}.backup__{1:dd-MMM-yyyy__HH-mm-ss}{2}.", m_sLogleFilePath, DateTime.Now, sExtension);

                        File.Move(m_sLogleFilePath, sNewFileName);
                    }

                    using (StreamWriter sw = new StreamWriter(m_sLogleFilePath, true))
                    {
                        while (!tc.IsToStop)
                        {
                            if (m_sqMessages.Count > 0)
                            {
                                LoggerRecord record = m_sqMessages.Dequeue();

                                sb.AppendFormat("{0} {1:dd-MMM-yyyy HH:mm:ss.ffffff} ", record.LoggerLevel, record.Time);

                                if (record.Args != null && record.Args.Length > 0)
                                {
                                    sb.AppendFormat(record.Message, record.Args);
                                }
                                else
                                {
                                    sb.Append(record.Message);
                                }

                                sb.Append("\r\n");

                                sw.Write(sb.ToString());
                                sw.Flush();
                            }

                            sb.Clear();
                            fi.Refresh();

                            if (fi.Length > m_lMaxSize)
                            {
                                break;
                            }

                            Thread.Sleep(1);
                        }
                    }
                }
            }
            catch (Exception excp)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine(ExcpHelper.FormatException(excp, "LiveLog.WriteConsole('{0}') ERROR", m_sLogleFilePath));
            }
        }
    }
}
