using System;
using SportRadar.Common.Logs;

namespace Nbt.Station.Design
{
    public class NbtLogSr
    {

        private static ILog Log = LogFactory.CreateLog(typeof(NbtLogSr));
        
        //protected static log4net.ILog m_logger = log4net.LogManager.GetLogger(typeof(NbtLogSr));
        private static string _MSG_ALL = "MSG_ALL";
        private static string _MSG_BETRADAR = "MSG_BETRADAR";
        private static string _MSG_SHOP = "MSG_SHOP";
        private static string _MSG_PRIO_1 = "MSG_PRIO_1"; // critical message
        private static string _MSG_TERMINAL = "MSG_TERMINAL";
        private static string _MSG_TICKET = "MSG_TICKET";
        private static string _MSG_MATCH = "MSG_MATCH";
        private static string _MSG_LIMITS = "MSG_LIMITS";
        private static string _MSG_WEBSERVICE = "MSG_WEBSERVICE";
        private static string _MSG_DBSYNC = "MSG_DBSYNC";
        private static string _MSG_STATION = "MSG_STATION";
        private static string _MSG_TICKETCALC = "MSG_TICKETCALC";
        private static string _MSG_LIVE_BET = "MSG_LIVE_BET";
        private static string _MSG_SMS = "MSG_SMS";
        private static string _MSG_SERVER = "MSG_SERVER";
        private static string _MSG_ADMIN = "MSG_ADMIN";

        private static object _LOGSYNC = new Object();
        private static object _READLOGSYNC = new Object();
        private static int _PRIORITY_HIGH = 1;
        private static int _PRIORITY_MEDIUM = 2;
        private static int _PRIORITY_LOW = 3;


        public static string MSG_ADMIN
        {
            get { return _MSG_ADMIN; }
        }

        public static string MSG_TICKETCALC
        {
            get { return _MSG_TICKETCALC; }
        }
        public static string MSG_DBSYNC
        {
            get { return _MSG_DBSYNC; }
        }

        public static string MSG_WEBSERVICE
        {
            get { return _MSG_WEBSERVICE; }
        }

        public static string MSG_LIMITS
        {
            get { return _MSG_LIMITS; }
        }

        public static string MSG_SHOP
        {
            get { return _MSG_SHOP; }
        }

        public static string MSG_MATCH
        {
            get { return _MSG_MATCH; }
        }

        public static string MSG_PRIO_1
        {
            get { return _MSG_PRIO_1; }
        }

        public static string MSG_TERMINAL
        {
            get { return _MSG_TERMINAL; }
        }

        public static string MSG_TICKET
        {
            get { return _MSG_TICKET; }
        }

        public static string MSG_ALL
        {
            get { return _MSG_ALL; }
        }

        public static string MSG_BETRADAR
        {
            get { return _MSG_BETRADAR; }
        }

        public static string MSG_STATION
        {
            get { return _MSG_STATION; }
        }

        public static string MSG_LIVE_BET
        {
            get { return _MSG_LIVE_BET; }
        }

        public static string MSG_SMS
        {
            get { return _MSG_SMS; }
        }

        public static string MSG_SERVER
        {
            get { return _MSG_SERVER; }
        }

        public static int PRIORITY_HIGH
        {
            get { return _PRIORITY_HIGH; }
        }

        public static int PRIORITY_MEDIUM
        {
            get { return _PRIORITY_MEDIUM; }
        }

        public static int PRIORITY_LOW
        {
            get { return _PRIORITY_LOW; }
        }


        private static object m_objLocker = new Object();




        public long NBTLogID { get; set; }
        public string Text { get; set; }
        public string ObjectID { get; set; }
        public int Criticality { get; set; }
        public bool Confirmed { get; set; }
        public DateTime InsertDate { get; set; }
        public string GroupCriteria { get; set; }
        public bool IsTransmited { get; set; }

        public static bool WriteNbtLogEntry(string Text, int Criticality, string ObjectID, string GroupCriteria)
        {
            return NbtLogSr.WriteNbtLogEntry(Text, Criticality, ObjectID, GroupCriteria, false);
        }

        public static bool WriteNbtLogEntry(string Text, int Criticality, string ObjectID, string GroupCriteria, bool IsTransmitted)
        {
            try
            {
                //GMA 21.04.2010 lesen und schreiben synchronisieren.
                lock (m_objLocker)
                {
                    // session.Flush();
                    NbtLogSr nBTLog = new NbtLogSr();

                    nBTLog.Text = Text;
                    nBTLog.Criticality = Criticality;
                    nBTLog.ObjectID = ObjectID;
                    nBTLog.Confirmed = false;
                    nBTLog.GroupCriteria = GroupCriteria;
                    nBTLog.InsertDate = DateTime.Now;
                    nBTLog.IsTransmited = IsTransmitted;
                    Log.Error(Text,new Exception(Text));
                    return true;
                }
            }
            catch (Exception excp)
            {
                Log.ErrorFormat("Error write to NBTLog-Table:{0}\r\n{1}\r\n",excp, excp.Message, excp.StackTrace);
            }

            return false;
        }
    }
}
