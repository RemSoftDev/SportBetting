using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportRadar.DAL.CommonObjects
{
    public class PlayerSession : ObjectBase
    {
        #region Fields

        private static readonly TableSpecification TableSpec = new TableSpecification("PlayerSessions", true,
            "PlayerSessionId");

        public override TableSpecification Table { get { return TableSpec; } }

        public long PlayerSessionId { get; set; }

        public long AccountId { get; set; }

        public decimal AvailableCash { get; set; }

        public string CardNumber { get; set; }

        public string Language { get; set; }

        public string Role { get; set; }

        public string RoleColor { get; set; }

        public string SessionId { get; set; }
        
        public string Username { get; set; }
        
        public bool Closed { get; set; }

        public override long ORMID { get { return PlayerSessionId; } }

        #endregion

        #region Override Methods

        public override void FillFromDataRow(DataRow dr)
        {
            PlayerSessionId = DbConvert.ToInt64(dr, "PlayerSessionId");
            AccountId = DbConvert.ToInt32(dr, "AccountId");
            AvailableCash = DbConvert.ToDecimal(dr, "AvailableCash");
            CardNumber = DbConvert.ToString(dr, "CardNumber");
            Language = DbConvert.ToString(dr, "Language");
            Role = DbConvert.ToString(dr, "Role");
            RoleColor = DbConvert.ToString(dr, "RoleColor");
            SessionId = DbConvert.ToString(dr, "SessionId");
            Username = DbConvert.ToString(dr, "Username");
            Closed = DbConvert.ToBool(dr, "Closed");
        }

        public override DataRow CreateDataRow(DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();
            dr["PlayerSessionId"] = PlayerSessionId;
            dr["AccountId"] = AccountId;
            dr["AvailableCash"] = AvailableCash;
            dr["CardNumber"] = CardNumber;
            dr["Language"] = Language;
            dr["Role"] = Role;
            dr["RoleColor"] = RoleColor;
            dr["SessionId"] = SessionId;
            dr["Username"] = Username;
            dr["Closed"] = Closed;
            return dr;
        }

        public static PlayerSession CreateFromDataRow(DataRow dr)
        {
            var ps = new PlayerSession();
            ps.FillFromDataRow(dr);
            return ps;
        }
        
        #endregion

        #region Public Methods

        public static List<PlayerSession> GetActivePlayerSessions()
        {
            var result = new List<PlayerSession>();
            using (
                DataTable dt =
                    DataCopy.GetDataTable(
                        "SELECT * FROM PlayerSessions WHERE closed = false ORDER BY PlayerSessionId DESC"))
            {
                result.AddRange(from DataRow dr in dt.Rows select CreateFromDataRow(dr));
            }
            return result;
        }

        public static PlayerSession GetSession(string sessionId)
        {
            using (
                var dt = DataCopy.GetDataTable("SELECT * FROM PlayerSessions WHERE sessionid = '{0}'", sessionId))
                return dt.Rows != null && dt.Rows.Count == 1 ? CreateFromDataRow(dt.Rows[0]) : null;
        }

        #endregion

    }
}
