using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.NewLineObjects
{
    public class CompetitorToOutrightLn : ObjectBase, ILineObjectWithId<CompetitorToOutrightLn>, IRemovableLineObject<CompetitorToOutrightLn>
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(CompetitorToOutrightLn));
        public static readonly TableSpecification TableSpec = new TableSpecification("competitor_to_outright", false, "competitor_to_outright_id");

        public CompetitorToOutrightLn() : base(false)
        {
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.CompetitorsToOutright.ContainsKey(this.match2competitorid); }
        }

        public override void FillFromDataRow(DataRow dr)
        {
            this.match2competitorid = DbConvert.ToInt64(dr, "competitor_to_outright_id");
            this.CompetitorId = DbConvert.ToInt64(dr, "competitor_id");
            this.MatchId = DbConvert.ToInt64(dr, "match_id");
            this.hometeam = DbConvert.ToInt64(dr, "position");
            this.ExtendedId = DbConvert.ToInt32(dr, "extended_id");
            this.ExtendedState = DbConvert.ToString(dr, "extended_state");
            this.UpdateId = DbConvert.ToInt64(dr, "update_id");
        }

        public override DataRow CreateDataRow(DataTable dtSample)
        {
            try
            {
                DataRow dr = dtSample.NewRow();

                dr["competitor_to_outright_id"] = this.match2competitorid;
                dr["competitor_id"] = this.CompetitorId;
                dr["match_id"] = this.MatchId;
                dr["position"] = this.hometeam;
                dr["extended_id"] = this.ExtendedId;
                dr["extended_state"] = this.ExtendedState;
                dr["update_id"] = this.UpdateId;

                return dr;
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "GroupLn.CreateDataRow() ERROR");
                throw;
            }
        }

        public override void Deserialize(ISerializableObject so)
        {
            dynamic dso = so;

            this.MatchId = dso.MatchId.Value;
            this.CompetitorId = dso.CompetitorId.Value;
            this.match2competitorid = dso.match2competitorid.Value;
            this.hometeam = dso.hometeam.Value;
            this.Islivebet = dso.Islivebet.Value;
           

        }

        public void MergeFrom(CompetitorToOutrightLn objSource)
        {
            Debug.Assert(this.match2competitorid == objSource.match2competitorid);
            Debug.Assert(this.CompetitorId == objSource.CompetitorId);
            Debug.Assert(this.MatchId == objSource.MatchId);

            this.hometeam = objSource.hometeam;
            this.ExtendedId = objSource.ExtendedId;
            this.ExtendedState = objSource.ExtendedState;
        }

        public long Id { get { return this.match2competitorid; } }
        public long match2competitorid { get; set; }
        public long CompetitorId { get; set; }
        public long MatchId { get; set; }
        public bool Islivebet { get; set; }
        public long hometeam { get; set; }
        public long ExtendedId { get; set; }
        public string ExtendedState { get; set; }

        public CompetitorLn GetCompetitor()
        {
            return LineSr.Instance.AllObjects.Competitors.GetObject(this.CompetitorId);
        }

        public override string ToString()
        {
            return string.Format("CompetitorToOutrightLn {{match2competitorid={0}, CompetitorId={1}, MatchId={2}, hometeam={3}, ExtendedId={4}, IsNew={5}}}",
                this.match2competitorid, this.CompetitorId, this.MatchId, this.hometeam, this.ExtendedId, this.IsNew);
        }

        public long RemoveId
        {
            get { return this.match2competitorid; }
        }
    }

    public sealed class PositionToOutrightDictionary : SyncDictionary<long, CompetitorToOutrightLn>
    {
    }

    public sealed class CompetitorToOutrightDictionary : LineObjectDictionaryByIdBase<CompetitorToOutrightLn>
    {
        private SyncDictionary<long, PositionToOutrightDictionary> m_diMatchIdToMPositionToOutrightDictionary = new SyncDictionary<long, PositionToOutrightDictionary>();

        public PositionToOutrightDictionary GetPositionToOutrightDictionaryByMatchId(long lMatchId)
        {
            return m_diMatchIdToMPositionToOutrightDictionary.SafelyGetValue(lMatchId);
        }

        public override CompetitorToOutrightLn MergeLineObject(CompetitorToOutrightLn objSource)
        {
            lock (m_oLocker)
            {
                CompetitorToOutrightLn cto = MergeLineObjectImp(objSource);

                PositionToOutrightDictionary di = m_diMatchIdToMPositionToOutrightDictionary.SafelyGetValue(cto.MatchId);

                if (di == null)
                {
                    di = new PositionToOutrightDictionary();
                    m_diMatchIdToMPositionToOutrightDictionary.Add(cto.MatchId, di);
                }

                di[cto.hometeam] = cto;

                return cto;
            }
        }

        public void RemoveByOutrightMatchId(long lMatchId)
        {
            PositionToOutrightDictionary di = m_diMatchIdToMPositionToOutrightDictionary.SafelyGetValue(lMatchId);

            if (di != null)
            {
                lock (m_oLocker)
                {
                    foreach (CompetitorToOutrightLn cto in di.ToSyncList())
                    {
                        m_di.Remove(cto.match2competitorid);
                        LineSr.Instance.ObjectsToRemove.SafelyAddObject(cto);
                    }
                }

                m_diMatchIdToMPositionToOutrightDictionary.Remove(lMatchId);
            }
        }
    }
}
