using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.NewLineObjects
{
    public sealed class BetDomainTypeLn : ObjectBase, ILineObjectWithKey<BetDomainTypeLn>
    {
        private const int ONE_FORMAT_ARG = 1;
        private const int TWO_FORMAT_ARG = 2;

        private const int SUB_PART_SPECIAL_ODD_VALUE = -1;
        private const int SUB_PART_SPECIAL_ODD_VALUE_FULL = -2;

        public static SyncHashSet<string> BASKETBALL_TOTAL_BET_TAGS = new SyncHashSet<string>() { "TTLQ1", "TTLQ2", "TTLQ3", "TTLQ4", "TTLQ1_L", "TTLQ2_L", "TTLQ3_L", "TTLQ4_L" };
        public static SyncHashSet<string> BASKETBALL_DRAW_BET_TAGS = new SyncHashSet<string>() { "DRAWQ1", "DRAWQ2", "DRAWQ3", "DRAWQ4", "DRAWQ1_L", "DRAWQ2_L", "DRAWQ3_L", "DRAWQ4_L" };
        public static SyncHashSet<string> VOLLEYALL_TOTAL_BET_TAGS = new SyncHashSet<string>() { "TTLQ1", "TTLQ2", "TTLQ3", "TTLQ4", "TTLQ5", "TTLQ1_L", "TTLQ2_L", "TTLQ3_L", "TTLQ4_L", "TTLQ5_L" };
        public static SyncHashSet<string> TENNIS_SET_WINNERS = new SyncHashSet<string>() { "WINS1_L", "WINS2_L", "WINS3_L", "WINS4_L", "WINS5_L" };

        public const string BET_TAG_WINFT = BetTypeLn.BET_TYPE_WIN + TimeTypeLn.TIME_TYPE_FT;  // WINFT  - 'Who wins the match?'
        public const string BET_TAG_WINHFT = BetTypeLn.BET_TYPE_WIN + TimeTypeLn.TIME_TYPE_HFT;  // WINHT  - 'Who wins the period?'
        public const string BET_TAG_WINFTR = BetTypeLn.BET_TYPE_WIN + TimeTypeLn.TIME_TYPE_FTR; // WINFTR - 'Who wins the rest of the match?'

        public const string BET_TYPE_UNDER_OVER = "TTL";

        private static ILog m_logger = LogFactory.CreateLog(typeof(BetDomainTypeLn));
        public static readonly TableSpecification TableSpec = new TableSpecification("betdomain_type", false, "tag");

        public BetDomainTypeExternalState m_bdtes = new BetDomainTypeExternalState();

        protected static BetDomainTypeLn m_EmptyBetDomainType = null;

        public BetDomainTypeLn()
        {

        }

        public static BetDomainTypeLn EmptyBetDomainType
        {
            get
            {
                if (m_EmptyBetDomainType == null)
                {
                    m_EmptyBetDomainType = new BetDomainTypeLn();

                    m_EmptyBetDomainType.Tag = string.Empty;
                    m_EmptyBetDomainType.MappingCode = -1;
                    m_EmptyBetDomainType.Name = string.Empty;
                    m_EmptyBetDomainType.BetTypeTag = string.Empty;
                    m_EmptyBetDomainType.ScoreTypeTag = string.Empty;
                    m_EmptyBetDomainType.TimeTypeTag = string.Empty;
                    m_EmptyBetDomainType.Sort = 0;
                    m_EmptyBetDomainType.Active = true;
                    m_EmptyBetDomainType.ExternalState = string.Empty;

                    Debug.Assert(m_EmptyBetDomainType.TranslationArgsCount == 0);
                }

                return m_EmptyBetDomainType;
            }
        }

        public void EnsureExternalObjects()
        {
            m_bdtes = LineSerializeHelper.StringToObject<BetDomainTypeExternalState>(this.ExternalState);
        }

        private void EnsureExternalState()
        {
            this.ExternalState = LineSerializeHelper.ObjectToString<BetDomainTypeExternalState>(this.m_bdtes);
        }

        public ObservablePropertyList ChangedProps { get; private set; }

        public override void FillFromDataRow(DataRow dr)
        {
            this.Tag = DbConvert.ToString(dr, "tag");
            this.MappingCode = DbConvert.ToInt32(dr, "mapping_code");
            this.Name = DbConvert.ToString(dr, "name");
            this.BetTypeTag = DbConvert.ToString(dr, "bet_type");
            this.ScoreTypeTag = DbConvert.ToString(dr, "score_type");
            this.TimeTypeTag = DbConvert.ToString(dr, "time_type");
            this.Sort = DbConvert.ToInt32(dr, "sort");
            this.Active = DbConvert.ToBool(dr, "active");
            this.ExternalState = DbConvert.ToString(dr, "external_state");

            Debug.Assert(!string.IsNullOrEmpty(this.ExternalState));

            EnsureExternalObjects();
        }

        public override DataRow CreateDataRow(DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            EnsureExternalState();

            dr["tag"] = Tag;
            dr["mapping_code"] = MappingCode;
            dr["name"] = Name;
            dr["bet_type"] = BetTypeTag;
            dr["score_type"] = ScoreTypeTag;
            dr["time_type"] = TimeTypeTag;
            dr["sort"] = Sort;
            dr["active"] = Active;
            dr["external_state"] = this.ExternalState;

            return dr;
        }

        public string KeyName { get { return this.Tag; } }
        public string Tag { get; set; }
        public int MappingCode { get; set; }
        public string Name { get; set; }
        public string BetTypeTag { get; set; }
        public string ScoreTypeTag { get; set; }
        public string TimeTypeTag { get; set; }
        public BetTypeLn BetType { get; private set; }
        public ScoreTypeLn ScoreType { get; private set; }
        public TimeTypeLn TimeType { get; private set; }
        public int Sort { get; set; }
        public bool Active { get; set; }
        public string ExternalState { get; set; }

        public string BetTypeName { get { return this.BetType != null ? this.BetType.Name : string.Empty; } }
        public string ScoreTypeName { get { return this.ScoreType != null ? this.ScoreType.Name : string.Empty; } }
        public string TimeTypeName { get { return this.TimeType != null ? this.TimeType.Name : string.Empty; } }

        public int Part { get { return m_bdtes.Part; } set { m_bdtes.Part = value; } }
        public int SubPart { get { return m_bdtes.SubPart; } set { m_bdtes.SubPart = value; } }
        public int TranslationArgsCount { get { return m_bdtes.TranslationArgsCount; } set { m_bdtes.TranslationArgsCount = value; } }


        public int GetExternalSort(string sportDesc)
        {
            if (!m_bdtes.ExternalSortDict.ContainsKey(sportDesc))
                return this.Sort;
            return m_bdtes.ExternalSortDict[sportDesc];
        }

        // DK - XXX - We need sometimes format translation strings where we use string.Format (like 'bla bla {0}')
        public string FormatTranslation(string sTranslation, string sSpecialOddValue, string sSpecialOddValueFull)
        {
            try
            {
                if (m_bdtes.TranslationArgsCount == ONE_FORMAT_ARG)
                {
                    if (m_bdtes.SubPart == SUB_PART_SPECIAL_ODD_VALUE)
                    {
                        return string.Format(sTranslation, sSpecialOddValue, m_bdtes.Part);
                    }

                    if (m_bdtes.SubPart == SUB_PART_SPECIAL_ODD_VALUE_FULL)
                    {
                        return string.Format(sTranslation, sSpecialOddValueFull, m_bdtes.Part);
                    }

                    return string.Format(sTranslation, m_bdtes.Part, sSpecialOddValue);
                }

                if (m_bdtes.TranslationArgsCount == TWO_FORMAT_ARG)
                {
                    return string.Format(sTranslation, m_bdtes.SubPart, m_bdtes.Part);
                }
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "FormatTranslation(sTranslation='{0}', sSpecialOddValue='{1}') ERROR", sTranslation, sSpecialOddValue);
                throw;
            }

            Debug.Assert(false);

            return sTranslation;
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.BetDomainTypes.ContainsKey(this.Tag); }
        }

        public void MergeFrom(BetDomainTypeLn objSource)
        {
            Debug.Assert(this.Tag == objSource.Tag);
            Debug.Assert(this.BetTypeTag == objSource.BetTypeTag);
            Debug.Assert(this.ScoreTypeTag == objSource.ScoreTypeTag);
            Debug.Assert(this.TimeTypeTag == objSource.TimeTypeTag);

            objSource.EnsureExternalState();

            this.MappingCode = objSource.MappingCode;
            this.Name = objSource.Name;
            this.Sort = objSource.Sort;
            this.Active = objSource.Active;

            if (this.m_bdtes.Part == 0 && this.m_bdtes.Part != objSource.m_bdtes.Part)
                this.m_bdtes.Part = objSource.m_bdtes.Part;
            if (this.m_bdtes.SubPart == 0 && this.m_bdtes.SubPart != objSource.m_bdtes.SubPart)
                this.m_bdtes.SubPart = objSource.m_bdtes.SubPart;
            if (this.m_bdtes.TranslationArgsCount == 0 && this.m_bdtes.TranslationArgsCount != objSource.m_bdtes.TranslationArgsCount)
                this.m_bdtes.TranslationArgsCount = objSource.m_bdtes.TranslationArgsCount;

            foreach (var externalItem in objSource.m_bdtes.ExternalSort)
            {
                if (this.m_bdtes.ExternalSort.All(x => x.Name != externalItem.Name))
                    this.m_bdtes.ExternalSort.Add(externalItem);
                else
                {
                    this.m_bdtes.ExternalSort.First(x => x.Name == externalItem.Name).Value = externalItem.Value;
                }
            }
            this.EnsureExternalState();
        }


        public override void SetRelations()
        {
            try
            {
                ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(string.IsNullOrEmpty(this.BetTypeTag), "BetTypeTag is not specified.");
                ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(string.IsNullOrEmpty(this.ScoreTypeTag), "ScoreTypeTag is not specified.");
                ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(string.IsNullOrEmpty(this.TimeTypeTag), "TimeTypeTag is not specified.");

                this.BetType = LineSr.Instance.AllObjects.BetTypes.GetObject(this.BetTypeTag);
                ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(this.BetType == null, "BetType '{0}' is not found.", this.BetTypeTag);

                this.ScoreType = LineSr.Instance.AllObjects.ScoreTypes.GetObject(this.ScoreTypeTag);
                ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(this.ScoreType == null, "ScoreType '{0}' is not found.", this.ScoreTypeTag);

                this.TimeType = LineSr.Instance.AllObjects.TimeTypes.GetObject(this.TimeTypeTag);
                ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(this.TimeType == null, "TimeType '{0}' is not found.", this.TimeTypeTag);
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "BetDomainTypeLn.SetRelations(Tag='{0}', BetTypeTag='{1}', ScoreTypeTag='{2}', TimeTypeTag='{3}') ERROR", this.Tag, this.BetTypeTag, this.ScoreTypeTag, this.TimeTypeTag);
                throw;
            }
        }

        public ISerializableObject Serialize()
        {
            dynamic so = new SerializableObject(this.GetType());

            so.Tag = this.Tag;
            so.MappingCode = this.MappingCode;
            so.Name = this.Name;
            so.BetTypeTag = this.BetTypeTag;
            so.ScoreTypeTag = this.ScoreTypeTag;
            so.TimeTypeTag = this.TimeTypeTag;
            so.Sort = this.Sort;
            so.Active = this.Active;
            so.ExternalState = this.ExternalState;

            return so;
        }

        public  void Deserialize(ISerializableObject so)
        {
            dynamic dso = so;

            this.Tag = dso.Tag.Value;
            this.MappingCode = dso.MappingCode.Value;
            this.Name = dso.Name.Value;
            this.BetTypeTag = dso.BetTypeTag.Value;
            this.ScoreTypeTag = dso.ScoreTypeTag.Value;
            this.TimeTypeTag = dso.TimeTypeTag.Value;
            this.Sort = dso.Sort.Value;
            this.Active = dso.Active.Value;
            this.ExternalState = dso.ExternalState.Value;

            this.EnsureExternalObjects();
        }

        public override string ToString()
        {
            return string.Format("BetDomainType {{Tag='{0}', BetTypeTag='{1}', ScoreTypeTag='{2}', TimeTypeTag='{3}'}}", this.Tag, this.BetTypeTag, this.ScoreTypeTag, this.TimeTypeTag);
        }
    }

    [XmlRoot("state", Namespace = LineSerializeHelper.DEFAULT_NAMESPACE, IsNullable = false)]
    public class BetDomainTypeExternalState
    {
        private List<ExternalItem<int>> _externalSort = new List<ExternalItem<int>>();

        [XmlElement(ElementName = "part")]
        public int Part { get; set; }
        [XmlElement(ElementName = "subPart")]
        public int SubPart { get; set; }
        [XmlElement(ElementName = "argsCount")]
        public int TranslationArgsCount { get; set; }
        [XmlArray("ExternalSort")]
        [XmlArrayItem("f")]
        public List<ExternalItem<int>> ExternalSort
        {
            get { return _externalSort; }
            set { _externalSort = value; }
        }

        public IDictionary<string, int> ExternalSortDict
        {
            get
            {
                var dict = new Dictionary<string, int>();

                foreach (var externalSortItem in ExternalSort)
                {
                    dict.Add(externalSortItem.Name, externalSortItem.Value);
                }
                return dict;
            }
        }
    }

    public class ExternalItem<T>
    {
        [XmlAttribute(AttributeName = "n")]
        public string Name { get; set; }
        [XmlText()]
        public T Value { get; set; }
    }

    public class BetDomainTypeDictionary : LineObjectDictionaryByKeyBase<BetDomainTypeLn>
    {
    }
}
