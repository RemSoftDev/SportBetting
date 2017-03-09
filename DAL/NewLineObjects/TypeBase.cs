using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.Common.Logs;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.NewLineObjects
{
    // Base Class
    public class TypeBase : DatabaseBase
    {
        public TypeBase() 
        {
            
        }

        public override void FillFromDataRow(DataRow dr)
        {
            this.Tag = DbConvert.ToString(dr, "tag");
            this.Name = DbConvert.ToString(dr, "name");
            this.ExternalState = DbConvert.ToString(dr, "external_state");
        }

        public override DataRow CreateDataRow(DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            dr["tag"] = Tag;
            dr["name"] = Name;
            dr["external_state"] = ExternalState;

            return dr;
        }

        public string KeyName { get { return this.Tag; } }
        public string Tag { get; set; }
        public string Name { get; set; }
        public string ExternalState { get; set; }

        protected void MergeFrom(TypeBase objSource)
        {
            Debug.Assert(this.Tag == objSource.Tag);

            this.Name = objSource.Name;
            this.ExternalState = objSource.ExternalState;
        }

        public ISerializableObject Serialize()
        {
            dynamic so = new SerializableObject(this.GetType());

            so.Tag = this.Tag;
            so.Name = this.Name;
            so.ExternalState = this.ExternalState;

            return so;
        }

        public virtual bool IsNew { get { return false; } }

        public void Deserialize(ISerializableObject so)
        {
            dynamic dso = so;

            this.Tag = dso.Tag.Value;
            this.Name = dso.Name.Value;
            this.ExternalState = dso.ExternalState.Value;
        }

        public override string ToString()
        {
            return string.Format("{0} {{Tag='{1}', Name='{2}', IsNew={3}, ExtState='{4}'}}", this.GetType().Name, this.Tag, this.Name, this.IsNew, this.ExternalState);
        }
    }

    // Time Type
    public class TimeTypeLn : TypeBase, ILineObjectWithKey<TimeTypeLn>
    {
        public const string TIME_TYPE_FT  = "FT";  // Full Time
        public const string TIME_TYPE_HFT = "HFT"; // Half or full time
        public const string TIME_TYPE_FTR = "FTR"; // Full Time - Rest of the Match

        private static ILog m_logger = LogFactory.CreateLog(typeof(BetDomainTypeLn));
        public static readonly TableSpecification TableSpec = new TableSpecification("time_type", false, "tag");

        public override bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.TimeTypes.ContainsKey(this.Tag); }
        }

        public ObservablePropertyList ChangedProps { get; private set; }

        public void MergeFrom(TimeTypeLn objSource)
        {
            base.MergeFrom(objSource);
        }

        public void MergeFrom(ISerializableObject so)
        {
            throw new NotImplementedException();
        }

        public void NotifyPropertiesChanged()
        {
            throw new NotImplementedException();
        }

        public void UnsetPropertiesChanged()
        {
            throw new NotImplementedException();
        }
    }

    public class TimeTypeDictionary : LineObjectDictionaryByKeyBase<TimeTypeLn>
    {
    }

    // Score Type
    public class ScoreTypeLn : TypeBase, ILineObjectWithKey<ScoreTypeLn>
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(BetDomainTypeLn));
        public static readonly TableSpecification TableSpec = new TableSpecification("score_type", false, "tag");

        public override bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.ScoreTypes.ContainsKey(this.Tag); }
        }

        public ObservablePropertyList ChangedProps { get; private set; }

        public void MergeFrom(ScoreTypeLn objSource)
        {
            base.MergeFrom(objSource);
        }

        public void MergeFrom(ISerializableObject so)
        {
            throw new NotImplementedException();
        }

        public void NotifyPropertiesChanged()
        {
            throw new NotImplementedException();
        }

        public void UnsetPropertiesChanged()
        {
            throw new NotImplementedException();
        }
    }

    public class ScoreTypeDictionary : LineObjectDictionaryByKeyBase<ScoreTypeLn>
    {
    }

    // Bet Type
    public class BetTypeLn : TypeBase, ILineObjectWithKey<BetTypeLn>
    {
        public const string BET_TYPE_WIN = "WIN"; // Winner

        private static ILog m_logger = LogFactory.CreateLog(typeof(BetDomainTypeLn));
        public static readonly TableSpecification TableSpec = new TableSpecification("bet_type", false, "tag");

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.BetTypes.ContainsKey(this.Tag); }
        }

        public ObservablePropertyList ChangedProps { get; private set; }

        public void MergeFrom(BetTypeLn objSource)
        {
            base.MergeFrom(objSource);
        }

        public void MergeFrom(ISerializableObject so)
        {
            throw new NotImplementedException();
        }

        public void NotifyPropertiesChanged()
        {
            throw new NotImplementedException();
        }

        public void UnsetPropertiesChanged()
        {
            throw new NotImplementedException();
        }
    }

    public class BetTypeDictionary : LineObjectDictionaryByKeyBase<BetTypeLn>
    {
    }
}
