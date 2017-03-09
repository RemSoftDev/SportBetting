using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class LogParams
    {
        public int Criticality { get; set; }
        public string ObjectId { get; set; }
        public TerminalMessageParams TerminalMessage { get; set; }
    }

    public enum TerminalMessageParams
    {
        [Description("MSG_ALL")]
        All,
        
        [Description("MSG_BETRADAR")]
        Betradar,

        [Description("MSG_SHOP")]
        Shop,

        [Description("MSG_PRIO_1")]
        Prio,

        [Description("MSG_TERMINAL")]
        Terminal,

        [Description("MSG_TICKET")]
        Ticket,

        [Description("MSG_MATCH")]
        Match,

        [Description("MSG_LIMITS")]
        Limits,

        [Description("MSG_WEBSERVICE")]
        WebService,

        [Description("MSG_DBSYNC")]
        DBSync,

        [Description("MSG_STATION")]
        Station,

        [Description("MSG_TICKETCALC")]
        Ticketcalc,

        [Description("MSG_LIVE_BET")]
        LiveBet,

        [Description("MSG_SMS")]
        SMS,

        [Description("MSG_SERVER")]
        Server,

        [Description("MSG_ADMIN")]
        Admin
    }
}
