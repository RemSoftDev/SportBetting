using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.NewLineObjects
{
    public sealed class CompetitorLn : ObjectBase, ILineObjectWithId<CompetitorLn>
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(CompetitorLn));

        public static readonly TableSpecification TableSpec = new TableSpecification("Competitor", false, "CompetitorId");

        private ObjectStringDictionary m_diStrings = null;

        private CompetitorExternalState m_ces = null;

        public long Id { get { return this.CompetitorId; } }
        public long CompetitorId { get; set; }
        public long BtrCompetitorId { get; set; }
        public string DefaultName { get; set; }
        public string ExternalState { get; set; }
        public string Base64Image { get; set; }

        public CompetitorLn()
            : base(true)
        {

        }

        public ObjectStringDictionary Strings
        {
            get
            {
                if (m_diStrings == null)
                {
                    Debug.Assert(this.CompetitorId != 0);

                    m_diStrings = LineSr.Instance.AllObjects.TaggedStrings.GetCompetitorStrings(this.CompetitorId);
                    if (m_diStrings == null)
                        m_diStrings = LineSr.Instance.AllObjects.TaggedStrings.GetCompetitorStrings(-this.CompetitorId);
                }

                return m_diStrings;
            }
        }

        public string GetDisplayName(string sLanguage)
        {
            TaggedStringLn tstr = this.Strings != null ? m_diStrings.SafelyGetString(sLanguage) : null;

            return tstr != null ? tstr.Text : string.Format("COMPETITOR_{0}", this.CompetitorId.ToString("G"));
        }

        public string DisplayName
        {
            get { return this.GetDisplayName(DalStationSettings.Instance.Language); }
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.Competitors.ContainsKey(this.CompetitorId); }
        }

        public CompetitorExternalState CompetitorExternalState
        {
            get
            {
                if (m_ces == null)
                {
                    m_ces = new CompetitorExternalState();
                }

                return m_ces;
            }
        }

        private void EnsureExternalObjects()
        {
            m_ces = LineSerializeHelper.StringToObject<CompetitorExternalState>(this.ExternalState);
        }

        private void EnsureExternalState()
        {
            this.ExternalState = LineSerializeHelper.ObjectToString<CompetitorExternalState>(this.CompetitorExternalState);
        }

        public override void FillFromDataRow(DataRow dr)
        {
            this.CompetitorId = DbConvert.ToInt64(dr, "CompetitorId");
            this.BtrCompetitorId = DbConvert.ToInt64(dr, "BtrCompetitorId");
            this.UpdateId = DbConvert.ToInt64(dr, "UpdateId");
            this.ExternalState = DbConvert.ToString(dr, "ExternalState");
            this.Base64Image = DbConvert.ToString(dr, "Base64Image");

            EnsureExternalObjects();
        }

        public override DataRow CreateDataRow(DataTable dtSample)
        {
            try
            {
                DataRow dr = dtSample.NewRow();

                EnsureExternalState();

                dr["CompetitorId"] = this.CompetitorId;
                dr["BtrCompetitorId"] = this.BtrCompetitorId;
                dr["UpdateId"] = this.UpdateId;
                dr["ExternalState"] = this.ExternalState;
                dr["Base64Image"] = this.Base64Image;

                return dr;
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "GroupLn.CreateDataRow() ERROR");
                throw;
            }
        }

        public void MergeFrom(CompetitorLn objSource)
        {
            Debug.Assert(this.CompetitorId == objSource.CompetitorId);
            Debug.Assert(this.BtrCompetitorId == objSource.BtrCompetitorId);

            objSource.EnsureExternalState();

            if (this.ExternalState != null)
            {
                this.EnsureExternalObjects();
            }

            this.SetRelations();
        }

        public override void SetRelations()
        {
            //ExcpHelper.ThrowIf(!LineSr.Instance.AllObjects.RelatedStrings.ContainsCompetitorId(this.CompetitorId), "Cannot get strings for {0}", this);
        }

        public override ISerializableObject Serialize()
        {
            EnsureExternalState();

            dynamic so = new SerializableObject(this.GetType());

            so.CompetitorId = this.CompetitorId;
            so.BtrCompetitorId = this.BtrCompetitorId;
            so.ExternalState = this.ExternalState;

            return so;
        }

        public override void Deserialize(ISerializableObject so)
        {
            dynamic dso = so;

            this.CompetitorId = dso.CompetitorId.Value;
            this.BtrCompetitorId = dso.BtrCompetitorId.Value;
            this.ExternalState = dso.ExternalState.Value;
            this.Base64Image = dso.Base64Image.Value;

            EnsureExternalObjects();
        }

        public override string ToString()
        {
            string sLang = DalStationSettings.Instance.Language;

            return string.Format("CompetitorLn {{CompetitorId={0}, BtrCompetitorId={1}, DisplayName({2})='{3}'}}", this.CompetitorId, this.BtrCompetitorId, sLang, this.GetDisplayName(sLang));
        }
    }

    [XmlRoot("state", Namespace = LineSerializeHelper.DEFAULT_NAMESPACE, IsNullable = true)]
    public class CompetitorExternalState
    {
        [XmlElement(ElementName = "desc")]
        public string SportDescriptor { get; set; }

        public override int GetHashCode()
        {
            return this.SportDescriptor != null ? this.SportDescriptor.GetHashCode() : base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            GroupSportExternalState gses = obj as GroupSportExternalState;

            return gses != null && gses.GetHashCode() == this.GetHashCode();
        }
    }

    public class CompetitorDictionary : LineObjectDictionaryByIdBase<CompetitorLn>
    {
    }
}
