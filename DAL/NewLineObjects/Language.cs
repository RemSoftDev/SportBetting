using SportRadar.DAL.CommonObjects;
using System.Diagnostics;
using System.Data;

namespace SportRadar.DAL.NewLineObjects
{
    public class LanguageLn : DatabaseBase, ILineObjectWithId<LanguageLn>
    {
        public static readonly TableSpecification TableSpec = new TableSpecification("languages", false, "languageid");

        public long LanguageId { get; set; }
        public string ShortName { get; set; }
        public bool IsTerminal { get; set; }

        public LanguageLn(long lLanguageId, string sShortName, bool bIsTerminal)
        {
            this.LanguageId = lLanguageId;
            this.ShortName = sShortName;
            this.IsTerminal = bIsTerminal;
        }

        public LanguageLn()
        {
        }

        public long Id
        {
            get { return this.LanguageId; }
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.Languages.ContainsKey(this.LanguageId); }
        }

        public ObservablePropertyList ChangedProps { get; private set; }

        public override string ToString()
        {
            return string.Format("LanguageLn {{LanguageId={0}, ShortName='{1}', IsTerminal={2}}}", this.LanguageId, this.ShortName, this.IsTerminal);
        }

        public void MergeFrom(LanguageLn objSource)
        {
            Debug.Assert(this.LanguageId == objSource.LanguageId);

            this.ShortName = objSource.ShortName;
            this.IsTerminal = objSource.IsTerminal;

            SetRelations();
        }

        public void MergeFrom(ISerializableObject so)
        {
            throw new System.NotImplementedException();
        }

        public override void FillFromDataRow(System.Data.DataRow dr)
        {
            this.LanguageId = DbConvert.ToInt64(dr, "languageid");
            this.ShortName = DbConvert.ToString(dr, "shortname");
            this.IsTerminal = DbConvert.ToBool(dr, "isterminal");
        }

        public override System.Data.DataRow CreateDataRow(System.Data.DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            dr["languageid"] = this.LanguageId;
            dr["shortname"] = this.ShortName;
            dr["isterminal"] = this.IsTerminal;

            return dr;
        }

        public void NotifyPropertiesChanged()
        {
            throw new System.NotImplementedException();
        }

        public void UnsetPropertiesChanged()
        {
            throw new System.NotImplementedException();
        }

        public override void SetRelations()
        {
            base.SetRelations();
        }

        public ISerializableObject Serialize()
        {
            throw new System.NotImplementedException();
        }

        public void Deserialize(ISerializableObject so)
        {
            throw new System.NotImplementedException();
        }
    }

    public class LanguageDictionary : LineObjectDictionaryByIdBase<LanguageLn>
    {
    }
}
