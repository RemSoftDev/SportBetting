using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportRadar.DAL.CommonObjects
{
    public class PlayerTicket : ObjectBase
    {

        #region Fields

        private static readonly TableSpecification TableSpec = new TableSpecification("PlayerTickets", true,
            "PlayerTicketId");

        public override TableSpecification Table { get { return TableSpec; } }

        public bool Closed { get; set; }

        public string Number { get; set; }

        public string Checksum { get; set; }

        public long PlayerSessionId { get; set; }

        public long PlayerTicketId { get; set; }

        public override long ORMID { get { return PlayerTicketId; } }

        #endregion

        #region

        public override void FillFromDataRow(DataRow dr)
        {
            Closed = DbConvert.ToBool(dr, "Closed");
            Number = DbConvert.ToString(dr, "Number");
            Checksum = DbConvert.ToString(dr, "Checksum");
            PlayerSessionId = DbConvert.ToInt64(dr, "PlayerSessionId");
            PlayerTicketId = DbConvert.ToInt64(dr, "PlayerTicketId");
        }

        public override DataRow CreateDataRow(DataTable dtSample)
        {
            var dr = dtSample.NewRow();
            dr["Closed"] = Closed;
            dr["Number"] = Number;
            dr["Checksum"] = Checksum;
            dr["PlayerSessionId"] = PlayerSessionId;
            dr["PlayerTicketId"] = PlayerTicketId;

            return dr;
        }

        public static PlayerTicket CreateFromDataRow(DataRow dr)
        {
            var ticket = new PlayerTicket();
            ticket.FillFromDataRow(dr);
            return ticket;
        }

        public static IList<PlayerTicket> GetTicketsForSession(string sessionId)
        {
            var result = new List<PlayerTicket>();
            using (
                DataTable dt =
                    DataCopy.GetDataTable(
                        "SELECT pt.* FROM PlayerTickets AS pt LEFT JOIN PlayerSessions AS ps ON pt.playersessionid = ps.playersessionid WHERE ps.sessionid ='" +
                        sessionId + "' AND pt.closed = false ORDER BY PlayerTicketId DESC"))
            {
                result.AddRange(from DataRow dr in dt.Rows select CreateFromDataRow(dr));
            }
            return result;
        }

        public static PlayerTicket GetTicket(string number, string checksum)
        {
            using (
                DataTable dt =
                    DataCopy.GetDataTable("SELECT * FROM PlayerTickets WHERE number = '{0}' AND checksum='{1}'",
                        number, checksum))
            {
                return dt.Rows != null && dt.Rows.Count == 1 ? CreateFromDataRow(dt.Rows[0]) : null;
            }
        }

        #endregion

    }
}
