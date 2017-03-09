using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.NewLineObjects
{
    public enum eObjectType
    {
        TaggedString = 0,
        Group = 1,
        Competitor = 2,
        OddTranslation = 5,
    }

    public sealed class TaggedStringLn : ObjectBase, ILineObjectWithId<TaggedStringLn>
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(TaggedStringLn));

        public const string CATEGORY_TAGGED_STRING = "TaggedString";
        public const string CATEGORY_GROUP = "Group";
        public const string ODDTRANSLATION = "OddTranslation";
        public const string CATEGORY_OUTCOMETYPE = "OutcomeType";
        public const string CATEGORY_COMPETITOR = "Competitor";

        public const string ENGLISH_LANGUAGE = "en";

        public static readonly TableSpecification TableSpec = new TableSpecification("strings", false, "string_id");

        public long Id { get { return this.StringId; } }
        public long StringId { get; set; }
        public string Category { get; set; }
        public string Tag { get; set; }
        public string Language { get; set; }
        public long? ObjectId { get; private set; }
        public string Text { get; set; }

        public static string GetKeyName(string sTag, string sLanguage)
        {
            try
            {
                return string.Concat(sTag.ToUpperInvariant(), KEY_SEPARATOR, sLanguage.ToLowerInvariant());
            }
            catch (Exception excp)
            {
                ExcpHelper.ThrowUp(excp, "GetKeyName(sTag='{0}', sLanguage='{1}') ERROR", sTag, sLanguage);
            }

            return null;
        }

        public TaggedStringLn()
            : base(true)
        {
            this.Category = string.Empty;
        }

        public bool IsRelated
        {
            get
            {
#if DEBUG
                if (this.ObjectId != null && this.ObjectId != 0)
                {
                    Debug.Assert(this.RelationType != eObjectType.TaggedString);
                }
#endif

                return this.ObjectId != null;// && this.ObjectId.Value > 0;
            }
        }

        public void SetRelation(eObjectType ot, long lObjectId)
        {
            Debug.Assert(lObjectId != 0);
            Debug.Assert(ot != eObjectType.TaggedString);

            this.Category = ot.ToString();
            this.ObjectId = lObjectId;
            relationType = null;
        }

        private eObjectType? relationType = null;

        public eObjectType RelationType
        {
            get
            {
                if (relationType == null)
                {
                    try
                    {
                        if (this.Category.Equals(ODDTRANSLATION, StringComparison.OrdinalIgnoreCase) ||
                            this.Category.Equals(CATEGORY_GROUP, StringComparison.OrdinalIgnoreCase) ||
                            this.Category.Equals(CATEGORY_COMPETITOR, StringComparison.OrdinalIgnoreCase))
                        {
                            relationType = (eObjectType)Enum.Parse(typeof(eObjectType), this.Category, true);
                        }
                        else
                            relationType = eObjectType.TaggedString;
                    }
                    catch
                    {
                        relationType = eObjectType.TaggedString;
                    }
                }
                return (eObjectType)relationType;
            }
            set { relationType = value; }
        }

        public string KeyName { get { return TaggedStringLn.GetKeyName(this.Tag, this.Language); } }

        public override void FillFromDataRow(DataRow dr)
        {
            this.StringId = DbConvert.ToInt64(dr, "string_id");
            this.Category = DbConvert.ToString(dr, "category");
            this.Tag = DbConvert.ToString(dr, "tag");
            this.Language = DbConvert.ToString(dr, "language");
            this.UpdateId = DbConvert.ToInt64(dr, "update_id");
            this.ObjectId = DbConvert.ToNullableInt64(dr, "object_id");
            this.Text = DbConvert.ToString(dr, "Text");
        }

        public override DataRow CreateDataRow(DataTable dtSample)
        {
            try
            {
                DataRow dr = dtSample.NewRow();

                dr["string_id"] = this.StringId;
                dr["category"] = this.Category;
                dr["Tag"] = this.Tag;
                dr["Language"] = this.Language;
                dr["update_id"] = this.UpdateId;
                DataCopy.SetNullableColumn(dr, "object_id", this.ObjectId);
                dr["Text"] = this.Text;

                return dr;
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "GroupLn.CreateDataRow() ERROR");
                throw;
            }
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.TaggedStrings.ContainsKey(this.StringId); }
        }

        public void MergeFrom(TaggedStringLn objSource)
        {
            lock (m_oLocker)
            {
                Debug.Assert(this.StringId == objSource.StringId);
                Debug.Assert(this.Tag == objSource.Tag);
                //Debug.Assert(this.Language == objSource.Language);

                this.Category = objSource.Category;
                this.ObjectId = objSource.ObjectId;
                this.Text = objSource.Text;
                relationType = null;
            }
        }

        public override ISerializableObject Serialize()
        {
            dynamic so = new SerializableObject(this.GetType());

            so.StringId = this.StringId;
            so.Category = this.Category;
            so.Tag = this.Tag;
            so.Language = this.Language;
            so.UpdateId = this.UpdateId;
            so.ObjectId = this.ObjectId;
            so.Text = this.Text;

            return so;
        }

        public override void Deserialize(ISerializableObject so)
        {
            dynamic dso = so;

            this.StringId = dso.StringId.Value;
            this.Category = dso.Category.Value;
            this.Tag = dso.Tag.Value;
            this.Language = dso.Language.Value;
            this.UpdateId = dso.UpdateId.Value;
            this.ObjectId = dso.ObjectId.Value;
            this.Text = dso.Text.Value;
            relationType = null;
        }

        public override string ToString()
        {
            return string.Format("TaggedStringLn {{StringId={0}, RelationType='{1}', Tag='{2}'; Language='{3}'; Category='{4}'; ObjectId={5}, Text='{6}', IsNew={7}, ChangedProps={8}}}",
                 this.StringId, this.RelationType, this.Tag, this.Language, this.Category, this.ObjectId, this.Text, this.IsNew, this.ChangedProps.Count);
        }
    }

    public class ObjectStringDictionary : SyncDictionary<string, TaggedStringLn>
    {
        public long MultiStringId { get; protected set; }
        public string MultiStringTag { get; protected set; }

        public ObjectStringDictionary(long lMultiStringId, string sMultiStringTag)
        {
            this.MultiStringId = lMultiStringId;
            this.MultiStringTag = sMultiStringTag;
        }

        public TaggedStringLn SafelyGetString(string sLanguage)
        {
            lock (m_oLocker)
            {
                sLanguage = sLanguage.ToLowerInvariant();

                if (!m_di.ContainsKey(sLanguage) && sLanguage != TaggedStringLn.ENGLISH_LANGUAGE)
                {
                    sLanguage = TaggedStringLn.ENGLISH_LANGUAGE;
                }

                return m_di.ContainsKey(sLanguage) ? m_di[sLanguage] : null;
            }
        }

        public TaggedStringLn Search(string sSearch, string sLanguage)
        {
            TaggedStringLn strFoundByLanguage = this.SafelyGetString(sLanguage);

            if (strFoundByLanguage != null && strFoundByLanguage.Text.IndexOf(sSearch, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return strFoundByLanguage;
            }

            SyncList<TaggedStringLn> lStrings = this.ToSyncList();

            foreach (TaggedStringLn str in lStrings)
            {
                if (!string.IsNullOrEmpty(str.Text))
                {
                    if (string.IsNullOrEmpty(sSearch) || str.Text.IndexOf(sSearch, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return str;
                    }
                }
            }

            return null;
        }

        public override string ToString()
        {
            return string.Format("ObjectStringDictionary {{MultiStringId={0}, MultiStringTag={1}, Count={2}}}", this.MultiStringId, this.MultiStringTag, this.Count);
        }
    }

    public class RelatedObject
    {
        public eObjectType ObjectType { get; private set; }
        public long ObjectId { get; private set; }

        public long MultistringId { get; set; }

        public RelatedObject(eObjectType eot, long lObjectId, long lMultiStringId)
        {
            ExcpHelper.ThrowIf<InvalidOperationException>(eot == eObjectType.TaggedString, "Related object cannot be with type TaggedString");

            this.ObjectType = eot;
            this.ObjectId = lObjectId;
            this.MultistringId = lMultiStringId;

        }

        public override string ToString()
        {
            return string.Format("RelatedObject {{Type={0}, Id={1}}}", this.ObjectType, this.ObjectId);
        }
    }

    public class TranslationDictionary : SyncDictionary<long, ObjectStringDictionary>
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(TranslationDictionary));

        [Obsolete]
        public override void Add(long key, ObjectStringDictionary value)
        {
            throw new NotImplementedException("Do not use obsolete method TranslationDictionary.Add(). Use instead method AddString");
        }

        public void AddRelatedString(long lKey, TaggedStringLn str)
        {
            if (str.IsRelated)
            {
                AddString(lKey, str);
            }
        }

        public void AddString(long lKey, TaggedStringLn str)
        {
            ObjectStringDictionary diStrings = null;

            lock (m_oLocker)
            {
                if (m_di.ContainsKey(lKey))
                {
                    diStrings = m_di[lKey];
                }
                else
                {
                    diStrings = m_di[lKey] = new ObjectStringDictionary(lKey, str.Tag);
                }
            }

            diStrings[str.Language] = str;
        }

        /*
        public void SetObjectStrings(long lMultiStringId, eObjectType ot, long lObjectId)
        {
#if DEBUG
            if (ot == eObjectType.Competitor && lObjectId == 527)
            {

            }
#endif

            if (m_di.ContainsKey(lMultiStringId))
            {
                ObjectStringDictionary diObjectStrings = m_di[lMultiStringId];

                diObjectStrings.SafelyForEach(delegate(TaggedStringLn str)
                {
                    if (!str.IsRelated)
                    {
                        str.SetRelation(ot, lObjectId);
                    }
                    else
                    {
                        m_logger.WarnFormat("TranslationDictionary.SetLineStringList(lMultiStringId={0}) is trying to set relation again for {1}", lMultiStringId, str);
                    }

                    return false;
                });
            }
        }
        */
    }

    public class TaggedStringDictionary : LineObjectDictionaryByIdBase<TaggedStringLn>
    {
        protected TranslationDictionary m_diGroupStrings = new TranslationDictionary();
        protected TranslationDictionary m_diOddStrings = new TranslationDictionary();
        protected TranslationDictionary m_diCompetitorStrings = new TranslationDictionary();
        //protected SyncDictionary<string, TaggedStringLn> m_diTaggedStrings = new SyncDictionary<string, TaggedStringLn>();
        protected SyncDictionary<string, IDictionary<string, TaggedStringLn>> m_diTaggedStrings2 = new SyncDictionary<string, IDictionary<string, TaggedStringLn>>();

        public string GetStringSafely(string sTag, string sLanguage)
        {
            lock (m_oLocker)
            {
                var str = m_diTaggedStrings2.SafelyGetValue(sTag.ToLowerInvariant());
                if (str != null && str.ContainsKey(sLanguage.ToLowerInvariant()) && !string.IsNullOrEmpty(str[sLanguage.ToLowerInvariant()].Text))
                    return str[sLanguage.ToLowerInvariant()].Text;
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["show_multistring_tags"]) && str != null && str.ContainsKey("en") && !string.IsNullOrEmpty(str["en"].Text))
                    return str["en"].Text;
                return string.Empty;

            }
        }

        public override TaggedStringLn MergeLineObject(TaggedStringLn objSource)
        {
            lock (m_oLocker)
            {
#if DEBUG
                if (m_di.ContainsKey(objSource.Id))
                {
                    TaggedStringLn strExisting = m_di[objSource.Id];
                }
#endif

                TaggedStringLn str = base.MergeLineObjectImp(objSource);

                if (str.IsRelated && str.RelationType != eObjectType.TaggedString)
                {
                    eObjectType ot = str.RelationType;

                    switch (ot)
                    {
                        case eObjectType.Group:

                            m_diGroupStrings.AddRelatedString((long)str.ObjectId, str);
                            break;

                        case eObjectType.Competitor:

                            m_diCompetitorStrings.AddRelatedString((long)str.ObjectId, str);
                            break;
                        case eObjectType.OddTranslation:

                            m_diOddStrings.AddRelatedString((long)str.ObjectId, str);
                            break;

                        default:

                            Debug.Assert(false);
                            break;
                    }
                }

                if (!m_diTaggedStrings2.ContainsKey(str.KeyName))
                {
                    IDictionary<string, TaggedStringLn> list = new Dictionary<string, TaggedStringLn>();
                    if (m_diTaggedStrings2.ContainsKey(str.Tag.ToLowerInvariant()))
                        list = m_diTaggedStrings2[str.Tag.ToLowerInvariant()];
                    list[str.Language.ToLowerInvariant()] = str;
                    m_diTaggedStrings2[str.Tag.ToLowerInvariant()] = list;
                }

                return str;
            }
        }

        public ObjectStringDictionary GetGroupStrings(long lGrouipId)
        {
            return m_diGroupStrings.ContainsKey(lGrouipId) ? m_diGroupStrings[lGrouipId] : null;
        }

        public ObjectStringDictionary GetCompetitorStrings(long lCompetitorId)
        {
            return m_diCompetitorStrings.ContainsKey(lCompetitorId) ? m_diCompetitorStrings[lCompetitorId] : null;
        }

        public ObjectStringDictionary GetOddStrings(long value)
        {
            return m_diOddStrings.ContainsKey(value) ? m_diOddStrings[value] : null;

        }

        public LineObjectList<TaggedStringLn> SearchRelatedStrings(string sSearch, string sLanguage, IdentityList ilGroups, IdentityList ilCompetitors)
        {
            ilGroups.Clear();
            ilCompetitors.Clear();

            LineObjectList<TaggedStringLn> lResult = new LineObjectList<TaggedStringLn>();

            SyncList<ObjectStringDictionary> lGroupStringDictionaries = m_diGroupStrings.ToSyncList();

            foreach (ObjectStringDictionary di in lGroupStringDictionaries)
            {
                TaggedStringLn strFound = di.Search(sSearch, sLanguage);

                if (strFound != null)
                {
                    Debug.Assert(strFound.RelationType == eObjectType.Group);
                    Debug.Assert(strFound.ObjectId.Value != null);

                    ilGroups.AddUnique(strFound.ObjectId.Value);
                }
            }

            SyncList<ObjectStringDictionary> lCompetitorStringDictionaries = m_diCompetitorStrings.ToSyncList();

            foreach (ObjectStringDictionary di in lCompetitorStringDictionaries)
            {
                TaggedStringLn strFound = di.Search(sSearch, sLanguage);

                if (strFound != null)
                {
                    Debug.Assert(strFound.RelationType == eObjectType.Competitor);
                    Debug.Assert(strFound.ObjectId.Value != null);

                    ilCompetitors.AddUnique(strFound.ObjectId.Value);
                }
            }

            return lResult;
        }

        public IDictionary<string, TaggedStringLn> GetStringByeKey(string value)
        {
            lock (m_oLocker)
            {
                if (m_diTaggedStrings2.ContainsKey(value))
                    return m_diTaggedStrings2[value];
                return null;
            }
        }
    }
}
