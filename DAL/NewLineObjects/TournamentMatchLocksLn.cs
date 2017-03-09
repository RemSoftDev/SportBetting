using SportRadar.DAL.CommonObjects;
using System.Diagnostics;
using System.Data;


namespace SportRadar.DAL.NewLineObjects
{
    public class TournamentMatchLocksLn : DatabaseBase, ILineObjectWithKey<TournamentMatchLocksLn>
    {
        public static readonly TableSpecification TableSpec = new TableSpecification("tournament_match_lock", false, "tmkey");
        public string TMKey;
        public string arrlocks { get; set; }

        public TournamentMatchLocksLn()
        {
        }

        public string KeyName
        {
            get { return this.TMKey; }
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.TournamentMatchLocks.ContainsKey(this.TMKey); }
        }

        public ObservablePropertyList ChangedProps { get; private set; }

        public override System.Data.DataRow CreateDataRow(System.Data.DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            dr["TMKey"] = this.TMKey;
            dr["arrlocks"] = this.arrlocks;

            return dr;
        }

        public override void FillFromDataRow(System.Data.DataRow dr)
        {
            this.TMKey = DbConvert.ToString(dr, "TMKey");
            this.arrlocks = DbConvert.ToString(dr, "arrlocks");
        }

        public void MergeFrom(TournamentMatchLocksLn objSource)
        {
            Debug.Assert(this.TMKey == objSource.TMKey);

            this.TMKey = objSource.TMKey;
            this.arrlocks = objSource.arrlocks;

            SetRelations();
        }

        public void MergeFrom(ISerializableObject so)
        {
            throw new System.NotImplementedException();
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

    public class TournamentMatchLocksDictionary : LineObjectDictionaryByKeyBase<TournamentMatchLocksLn>
    {
    }
}
