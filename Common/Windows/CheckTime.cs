using System;
using System.Collections.Generic;
using System.Configuration;

namespace SportRadar.Common.Windows
{
    public enum eEnable
    {
        Undefined = 0,
        Disabled = 1,
        Enabled = 2
    }

    public class CheckTime
    {
        protected const string ENABLE_CHECK_TIME = "enable_check_time";

        protected DateTime m_dtStart = DateTime.UtcNow;
        protected string m_sMessage = "Started";
        protected List<CheckTimeEvent> m_lEvents = new List<CheckTimeEvent>();

        protected static eEnable m_ect = eEnable.Undefined;
        protected static object m_objLocker = new Object();

        protected bool m_bEnabled = false;

        public static bool Enabled
        {
            get
            {
                if (m_ect == eEnable.Undefined)
                {
                    lock (m_objLocker)
                    {
                        try
                        {
                            m_ect = (eEnable)Enum.Parse(typeof(eEnable), ConfigurationManager.AppSettings[ENABLE_CHECK_TIME], true);
                        }
                        catch
                        {
                        }
                    }
                }

                return m_ect == eEnable.Enabled;
            }
        }

        protected class CheckTimeEvent
        {
            public DateTime m_dtDone = DateTime.UtcNow;
            public string m_sMessage = string.Empty;
        }

        public CheckTime(bool bEnabled)
        {
            m_bEnabled = bEnabled;
        }

        public CheckTime() : this (true)
        {
            
        }

        public CheckTime(bool bEnabled, string sMessageFormat, params object[] args) : this (bEnabled)
        {
            m_sMessage = string.Format(sMessageFormat, args);
        }

        public CheckTime(string sMessageFormat, params object[] args) : this(true, sMessageFormat, args)
        {
        }

        public void AddEvent(string sMessageFormat, params object[] args)
        {
            if (CheckTime.Enabled && m_bEnabled)
            {
                string sMessage = string.Format(sMessageFormat, args);

                CheckTimeEvent cte = new CheckTimeEvent() { m_dtDone = DateTime.UtcNow, m_sMessage = sMessage };
                m_lEvents.Add(cte);
            }
        }

        protected void AppendEventLine(DateTime dtThis, DateTime dtPrevious, string sMessage, ref string sResult)
        {
            const int MESSAGE_WIDTH = 64;
            const int TIME_WIDTH = 32;

            TimeSpan tsSpan = dtThis - dtPrevious;
            TimeSpan tsTotal = dtThis - m_dtStart;

            sResult += sMessage.PadRight(MESSAGE_WIDTH, ' ') +
                       dtThis.ToString().PadLeft(TIME_WIDTH, ' ') +
                       tsSpan.ToString().PadLeft(TIME_WIDTH, ' ') +
                       tsTotal.ToString().PadLeft(TIME_WIDTH, ' ') + "\r\n";
        }

        public void Info(SportRadar.Common.Logs.ILog logger)
        {
            if (CheckTime.Enabled && m_bEnabled)
            {
                logger.Info(this.ToString());
            }
        }

        public void Error(SportRadar.Common.Logs.ILog logger)
        {
            if (CheckTime.Enabled && m_bEnabled)
            {
                Exception ex = new Exception(this.ToString());
                logger.Error(this.ToString(), ex);
            }
        }

        public override string ToString()
        {
            if (!(CheckTime.Enabled && m_bEnabled))
            {
                return "CheckTime {disabled}";
            }

            string sResult = @"
Event                                                                        Time                              Span                             Total
********************************************************************************************************************************************************************************
";

            DateTime dtPrevious = m_dtStart;
            this.AppendEventLine(m_dtStart, dtPrevious, m_sMessage, ref sResult);

            foreach (CheckTimeEvent cte in m_lEvents)
            {
                this.AppendEventLine(cte.m_dtDone, dtPrevious, cte.m_sMessage, ref sResult);
                dtPrevious = cte.m_dtDone;
            }

            return sResult;
        }
    }
}