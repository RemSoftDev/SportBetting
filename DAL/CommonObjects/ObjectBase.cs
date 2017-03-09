using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.DAL.Annotations;
using SportRadar.DAL.Connection;
using SportRadar.Common.Windows;
using SportRadar.DAL.ViewObjects;

namespace SportRadar.DAL.CommonObjects
{
    public abstract class ObservablePropertyBase : INotifyPropertyChanged
    {
        protected static ILog m_logger = LogFactory.CreateLog(typeof(ObservablePropertyBase));
        public ObjectBase Parent { get; protected set; }
        public string PropertyName { get; protected set; }
        public ObservablePropertyList NotifyCollection { get; protected set; }

        public override int GetHashCode()
        {
            return this.PropertyName.GetHashCode();
        }

        internal protected ObservablePropertyBase(ObjectBase obParent, ObservablePropertyList oplNotifyCollection, string sPropertyName)
        {
            Debug.Assert(obParent != null);
            Debug.Assert(oplNotifyCollection != null);
            Debug.Assert(!string.IsNullOrEmpty(sPropertyName));
            this.Parent = obParent;
            this.NotifyCollection = oplNotifyCollection;
            this.PropertyName = sPropertyName;
            obParent.ObservablePropertyList.Add(this);

        }

        public abstract void MergeFrom(SerializableProperty sp);
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ObservableProperty<T> : ObservablePropertyBase
    {
        protected T m_value = default(T);
        protected Type m_type = typeof (T);
        protected bool m_bIsNullableType = false;

        public ObservableProperty(ObjectBase obParent, ObservablePropertyList oplNotifyCollection, string sName) : base (obParent, oplNotifyCollection, sName)
        {
            m_bIsNullableType = !m_type.IsValueType || (m_type.IsGenericType && m_type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public T PreviousValue { get; protected set; }

        public T Value
        {
            get { return m_value; }
            set
            {
                try
                {
                    bool bSet = false;

                    if (m_bIsNullableType)
                    {
                        bSet = (m_value != null && !m_value.Equals(value)) || (m_value == null && value != null);
                    }
                    else
                    {
                        bSet = !m_value.Equals(value);
                    }

                    if (bSet)
                    {
                        this.PreviousValue = m_value;
                        m_value = value;

                        this.NotifyCollection.Add(this);
                    }
                }
                catch (Exception excp)
                {
                    m_logger.Excp(excp, "ObservableProperty<{0}>.Set_Value() ERROR", m_type);
                }
            }
        }

        public Type PropertyType { get { return m_type; } }

        public bool IsNullableType { get { return m_bIsNullableType; } }

        public override int GetHashCode()
        {
            return base.PropertyName.GetHashCode();
        }

        public override void MergeFrom(SerializableProperty sp)
        {
            if (sp.IsSpecified)
            {
                SerializableProperty<T> tsp = sp as SerializableProperty<T>;

                if (tsp == null)
                {
                    Debug.Assert(sp.PropertyType == typeof(object));
                    this.Value = default(T);
                }
                else
                {
                    this.Value = tsp.Value;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("ObservableProperty {{PropertyType='{0}'; Name='{1}', Value='{2}', PreviousValue='{3}'}}", m_type, this.PropertyName, this.Value, this.PreviousValue);
        }
    }
    /*
    public sealed class SerializableProperty
    {
        public PropertyInfo        Property      { get; private set; }
        public XmlElementAttribute XmlElement    { get; private set; }
        public bool                IsObservable  { get; private set; }

        public SerializableProperty(PropertyInfo property, XmlElementAttribute xmlElement, bool bIsObservable)
        {
            Debug.Assert(property != null);
            Debug.Assert(xmlElement != null);

            this.Property = property;
            this.XmlElement = xmlElement;

            this.IsObservable = bIsObservable;
        }
    }
    */

    public class SerializablePropertyDictionary : SyncDictionary<string, Type>
    {
        
    }

    public class PropertyInfoList : List<PropertyInfo>
    {
        internal static SerializablePropertyDictionary GetSerializablePropertyDictionary(Type type)
        {
            SerializablePropertyDictionary di = new SerializablePropertyDictionary();
            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo pi in props)
            {
                Type t = pi.PropertyType;

                if (t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof (ObservableProperty<>))
                {
                    Type tArg = t.GetGenericArguments()[0];
                    di.Add(pi.Name.ToLowerInvariant(), tArg);
                }
                else
                {
                    di.Add(pi.Name.ToLowerInvariant(), t);
                }
            }

            return di;
        }

        internal static PropertyInfoList GetObservablePropertyInfoList(Type type)
        {
            PropertyInfoList lProps = new PropertyInfoList();
            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo pi in props)
            {
                Type t = pi.PropertyType;

                if (t.IsConstructedGenericType)
                {
                    if (t.GetGenericTypeDefinition() == typeof(ObservableProperty<>))
                    {
                        lProps.Add(pi);
                    }
                }
            }

            return lProps;
        }
    }

    public sealed class PropertyInfoDictionary<T> : SyncDictionary<Type, T>
    {
        private static PropertyInfoDictionary<SerializablePropertyDictionary> m_diSerializableProperties = new PropertyInfoDictionary<SerializablePropertyDictionary>();
        private static PropertyInfoDictionary<PropertyInfoList>               m_diObservableProperties   = new PropertyInfoDictionary<PropertyInfoList>();

        public delegate T DelegateGetObject(Type type);

        private T GetCollection(Type type, DelegateGetObject dgo)
        {
            lock (m_oLocker)
            {
                if (!m_di.ContainsKey(type))
                {
                    T obj = dgo(type);
                    m_di.Add(type, obj);

                    return obj;
                }

                return this[type];
            }
        }

        public static SerializablePropertyDictionary GetSerializablePropertyDictionary(Type type)
        {
            return m_diSerializableProperties.GetCollection(type, PropertyInfoList.GetSerializablePropertyDictionary);
        }

        public static PropertyInfoList GetObservablePropertiesInfoList(Type type)
        {
            return m_diObservableProperties.GetCollection(type, PropertyInfoList.GetObservablePropertyInfoList);
        }
    }

    public class ObservablePropertyList : HashSet<ObservablePropertyBase>
    {
        public HashSet<string> GetPropertyNames()
        {
            HashSet<string> hs = new HashSet<string>();

            foreach (ObservablePropertyBase opb in this)
            {
                hs.Add(opb.PropertyName);
            }

            return hs;
        }
    }

    public abstract class ObjectBase : INotifyPropertyChanged
    {
        public const char KEY_SEPARATOR = '*';
        //protected static Dictionary<string, HashCodeColumns> m_diHashCodeColumns = null;

        //public const string EMPTY_EXTERNAL_STATE = "<state xmlns=\"sr\" />";
        public static readonly string EMPTY_EXTERNAL_STATE = LineSerializeHelper.ObjectToString<EmptyExternalState>(new EmptyExternalState());

        protected ObservablePropertyList m_lChangedProps = new ObservablePropertyList();
        public ObservablePropertyList ObservablePropertyList = new ObservablePropertyList();
        protected object m_objJustInsertedIdentity = null;
        protected ViewObjectBase m_objView = null;

        protected object m_oLocker = new Object();

        protected ObjectBase()
        {
        }

        protected ObjectBase(bool bIsObservable)
            : this()
        {
            if (bIsObservable)
            {
                this.InitializeObservableProperties();
            }
        }

        public object SourceObject { get; set; }

        [XmlIgnore]
        public virtual long ORMID { get { return 0; } }

        [XmlIgnore]
        public virtual long SvrID { get { return 0; } set {/* Setter should be overriden in derived classes */} }

        [XmlIgnore]
        public virtual bool IsModified { get; set; }

        public virtual bool Remove { get; set; }

        public virtual void SetRelations()
        {
            // Might be overriden in derived classes
        }

        public ObservablePropertyList ChangedProps
        {
            get { return m_lChangedProps; }
        }

        public string GetChangedPropNames()
        {
            List<string> lPropNames = new List<string>();

            foreach (ObservablePropertyBase op in this.ChangedProps)
            {
                lPropNames.Add(op.PropertyName);
            }

            return string.Join(", ", lPropNames.ToArray());
        }

        protected void InitializeObservableProperties()
        {
            PropertyInfoList lProps = PropertyInfoDictionary<PropertyInfoList>.GetObservablePropertiesInfoList(this.GetType());

            foreach (PropertyInfo pi in lProps)
            {
                Type[] arrTypes = pi.PropertyType.GetGenericArguments();
                Debug.Assert(arrTypes != null && arrTypes.Length > 0);

                Type t = typeof(ObservableProperty<>);
                Type[] typeArgs = new Type[] { arrTypes[0] };

                Type tObservableProperty = t.MakeGenericType(typeArgs);

                pi.SetValue(this, Activator.CreateInstance(tObservableProperty, this, m_lChangedProps, pi.Name));
            }
        }

        [XmlIgnore]
        public long UpdateId { get; set; }

        public abstract void FillFromDataRow(DataRow dr);
        //{
        //    throw new System.Exception(string.Format("FillFromDataRow() is not overriden for {0}", this.GetType()));
        //}

        /*
        public static T[] CreateFromDataTable<T>(DataTable dt) where T : ILineObject<T>
        {
            List<T> lResult = new List<T>();

            Type type = typeof(T);

            foreach (DataRow dr in dt.Rows)
            {
                try
                {
                    T obj = (T)Activator.CreateInstance(type);

                    obj.FillFromDataRow(dr);

                    ObjectBase objBase = obj as ObjectBase;
                    Debug.Assert(objBase != null);

                    objBase.OnCloneFromDataRow(dr);

                    lResult.Add(obj);
                }
                catch (Exception excp)
                {
                    m_logger.ErrorFormat("CreateFromDataTable<{0}>() ERROR:\r\n{1}\r\n{2}", typeof(T), excp.Message, excp.StackTrace);

                    throw;
                }
            }

            return lResult.ToArray();
        }

        private static void AddHashCodeColumns(string sTableName, params string[] args)
        {
            if (!m_diHashCodeColumns.ContainsKey(sTableName) && args.Length > 0)
            {
                HashCodeColumns hcc = new HashCodeColumns();

                foreach (string sColumnName in args)
                {
                    hcc.Add(sColumnName);
                }

                m_diHashCodeColumns.Add(sTableName, hcc);
            }
        }

        public static void InitializeHashCodeColumns()
        {
            if (m_diHashCodeColumns == null)
            {
                m_diHashCodeColumns = new Dictionary<string, HashCodeColumns>();

                ObjectBase.AddHashCodeColumns("MultiStringGroup", "MultiStringGroupTag", "Comment");
                ObjectBase.AddHashCodeColumns("MultiString", "MultiStringTag", "Comment", "MultiStringGroupID", "IsLiveBet");
                ObjectBase.AddHashCodeColumns("Language", "ShortName", "MultiStringID", "Priority", "IsTerminalLanguage");
                ObjectBase.AddHashCodeColumns("LanguageString", "Text", "MultiStringID", "LanguageID", "IsLiveBet");
                ObjectBase.AddHashCodeColumns("Competitor", "BtrCompetitorID", "DefaultName", "SportID", "MultiStringID", "CountryId", "BetTaxTypBetTaxTypID", "SvrCompetitorID", "LocalCountyTaxLocationID", "IsLiveBet", "BtrLiveBetCompetitorID");
                ObjectBase.AddHashCodeColumns("Country", "ISO2", "ISO3", "DefaultName", "MultiStringID");
                ObjectBase.AddHashCodeColumns("Sport", "BtrSportID", "DefaultName", "MultiStringID", "ShortName");
                ObjectBase.AddHashCodeColumns("Category", "MultiStringID", "DefaultName", "Sort");
                ObjectBase.AddHashCodeColumns("Tournament", "DefaultName", "BtrTournamentID", "MultiStringID", "SportID", "CategoryID", "Active", "MaxStakeLigaLimit", "MaxStakeTipLimit", "MinCombination", "OddGraduation", "Sort", "MultiStringID2", "ScoreFrequency", "Info", "CountryCountryId", "ExpireDate", "TennisSets", "ShowOnOddSheet", "OutrightTyp", "IsLiveBet", "IsLocked", "InfoMultiStringID", "AutoUpdateOdds", "LockWithAllOtherTournaments", "OddsGenerationModel", "DoNotUpdateSpecialBetDomains", "AutoEnablePreMatch");
            }
        }

        public static HashCodeColumns GetHashCodeColumns(string sTableName)
        {
            return m_diHashCodeColumns.ContainsKey(sTableName) ? m_diHashCodeColumns[sTableName] : null;
        }
        */

        public virtual void NotifyPropertiesChanged()
        {
            if (m_objView != null)
            {
                m_objView.RaisePropertiesChanged();
            }
        }

        public virtual void UnsetPropertiesChanged()
        {
            m_lChangedProps.Clear();

            if (m_objView != null)
            {
                m_objView.UnsetChanged();
            }
        }

        public bool DoesViewObjectExist
        {
            get { return m_objView != null; }
        }

        public virtual TableSpecification Table { get { return null; } }

        public virtual DataRow CreateDataRow(DataTable dtSample)
        {
            //throw new System.Exception(string.Format("CreateDataRow() is not overriden for {0}", this.GetType()));
            return DataCopy.CreateDataRow(dtSample, this, new ErrorList());
        }

        public virtual void Save()
        {
            this.Save(ConnectionManager.GetConnection(), null);
        }

        public virtual void Save(IDbConnection conn, IDbTransaction transaction)
        {
            if (this.ORMID == 0)
            {
                this.Insert(conn, transaction);
            }
            else
            {
                this.Update(conn, transaction);
            }
        }

        public virtual void Update()
        {
            this.Update(ConnectionManager.GetConnection(), null);
        }

        public virtual void Update(IDbConnection conn, IDbTransaction transaction)
        {
            ExcpHelper.ThrowIf(this.Table == null, "TableSpec is not specified for type {0}", this.GetType());

            using (DataTable dtUpdate = DataCopyTables.GetEmptyDataTableByName(conn, transaction, this.Table.TableName))
            {
                using (IDbCommand cmdUpdate = DataCopy.GenerateUpdateCommand(conn, transaction, dtUpdate, this.Table))
                {
                    using (IDisposable dsp = SqlObjectFactory.CreateDbAdapter(null) as IDisposable)
                    {
                        IDbDataAdapter daUpdate = dsp as IDbDataAdapter;
                        Debug.Assert(daUpdate != null);

                        daUpdate.UpdateCommand = cmdUpdate;

                        DataRow dr = this.CreateDataRow(dtUpdate);

                        dtUpdate.Rows.Add(dr);
                        dtUpdate.AcceptChanges();
                        dr.SetModified();

                        using (DataSet ds = new DataSet())
                        {
                            ds.Tables.Add(dtUpdate);
                            daUpdate.Update(ds);
                        }
                    }
                }
            }
        }


        public virtual void Insert()
        {
            this.Insert(ConnectionManager.GetConnection(), null);
        }

        public virtual void Insert(IDbConnection conn, IDbTransaction transaction)
        {
            ExcpHelper.ThrowIf(this.Table == null, "TableSpec is not specified for type {0}", this.GetType());

            using (DataTable dtInsert = DataCopyTables.GetEmptyDataTableByName(conn, transaction, this.Table.TableName))
            {
                using (IDbCommand cmdInsert = DataCopy.GenerateInsertCommand(conn, transaction, dtInsert, this.Table))
                {
                    using (IDisposable dsp = SqlObjectFactory.CreateDbAdapter(null) as IDisposable)
                    {
                        IDbDataAdapter daInsert = dsp as IDbDataAdapter;
                        Debug.Assert(daInsert != null);

                        daInsert.InsertCommand = cmdInsert;

                        dtInsert.AcceptChanges();

                        DataRow dr = this.CreateDataRow(dtInsert);

                        dtInsert.Rows.Add(dr);

                        using (DataSet ds = new DataSet())
                        {
                            ds.Tables.Add(dtInsert);
                            daInsert.Update(ds);
                        }

                        if (this.Table.IsAutoGeneratedIdentity)
                        {
                            Debug.Assert(this.Table.IdentityNames.Count > 0);
                            m_objJustInsertedIdentity = ConnectionManager.GetLastInsertId(conn, transaction, this.Table.IdentityNames[0]);
                        }
                    }
                }
            }
        }

        public virtual void Delete()
        {
            this.Delete(ConnectionManager.GetConnection(), null);
        }

        public virtual void Delete(IDbConnection conn, IDbTransaction transaction)
        {
            //DataCopy.ExecuteScalar(conn, transaction, "DELETE FROM {0} WHERE {1} = {2}", this.Table.TableName, this.Table.LocalIdentityName, this.ORMID);
        }

        public virtual ISerializableObject Serialize()
        {
            ExcpHelper.ThrowIf<NotImplementedException>(true, "{0}.Serialize() is not implemented", this.GetType().Name);

            return null;
        }

        public virtual void Deserialize(ISerializableObject so)
        {
            ExcpHelper.ThrowIf<NotImplementedException>(true, "{0}.Deserialize() is not implemented", this.GetType().Name);
        }

        public virtual void MergeFrom(ISerializableObject so)
        {

            foreach (var pi in this.ObservablePropertyList)
            {
                ObservablePropertyBase opb = pi;
                Debug.Assert(opb != null);

                SerializableProperty sp = so.GetSerializableProperty(opb.PropertyName.Trim());
                Debug.Assert(sp != null);

                opb.MergeFrom(sp);
            }

            this.SetRelations();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public abstract class DatabaseBase
    {
        public const char KEY_SEPARATOR = '*';
        //protected static Dictionary<string, HashCodeColumns> m_diHashCodeColumns = null;

        //public const string EMPTY_EXTERNAL_STATE = "<state xmlns=\"sr\" />";
        public static readonly string EMPTY_EXTERNAL_STATE = LineSerializeHelper.ObjectToString<EmptyExternalState>(new EmptyExternalState());

        protected object m_objJustInsertedIdentity = null;

        protected object m_oLocker = new Object();

        protected DatabaseBase()
        {
        }


        public object SourceObject { get; set; }

        [XmlIgnore]
        public virtual long ORMID { get { return 0; } }

        [XmlIgnore]
        public virtual long SvrID { get { return 0; } set {/* Setter should be overriden in derived classes */} }

        [XmlIgnore]
        public virtual bool IsModified { get; set; }

        public virtual bool Remove { get; set; }

        public virtual void SetRelations()
        {
            // Might be overriden in derived classes
        }


        [XmlIgnore]
        public long UpdateId { get; set; }

        public abstract void FillFromDataRow(DataRow dr);
        //{
        //    throw new System.Exception(string.Format("FillFromDataRow() is not overriden for {0}", this.GetType()));
        //}

       

        public virtual TableSpecification Table { get { return null; } }

        public virtual DataRow CreateDataRow(DataTable dtSample)
        {
            //throw new System.Exception(string.Format("CreateDataRow() is not overriden for {0}", this.GetType()));
            return DataCopy.CreateDataRow(dtSample, this, new ErrorList());
        }

        public virtual void Save()
        {
            this.Save(ConnectionManager.GetConnection(), null);
        }

        public virtual void Save(IDbConnection conn, IDbTransaction transaction)
        {
            if (this.ORMID == 0)
            {
                this.Insert(conn, transaction);
            }
            else
            {
                this.Update(conn, transaction);
            }
        }

        public virtual void Update()
        {
            this.Update(ConnectionManager.GetConnection(), null);
        }

        public virtual void Update(IDbConnection conn, IDbTransaction transaction)
        {
            ExcpHelper.ThrowIf(this.Table == null, "TableSpec is not specified for type {0}", this.GetType());

            using (DataTable dtUpdate = DataCopyTables.GetEmptyDataTableByName(conn, transaction, this.Table.TableName))
            {
                using (IDbCommand cmdUpdate = DataCopy.GenerateUpdateCommand(conn, transaction, dtUpdate, this.Table))
                {
                    using (IDisposable dsp = SqlObjectFactory.CreateDbAdapter(null) as IDisposable)
                    {
                        IDbDataAdapter daUpdate = dsp as IDbDataAdapter;
                        Debug.Assert(daUpdate != null);

                        daUpdate.UpdateCommand = cmdUpdate;

                        DataRow dr = this.CreateDataRow(dtUpdate);

                        dtUpdate.Rows.Add(dr);
                        dtUpdate.AcceptChanges();
                        dr.SetModified();

                        using (DataSet ds = new DataSet())
                        {
                            ds.Tables.Add(dtUpdate);
                            daUpdate.Update(ds);
                        }
                    }
                }
            }
        }


        public virtual void Insert()
        {
            this.Insert(ConnectionManager.GetConnection(), null);
        }

        public virtual void Insert(IDbConnection conn, IDbTransaction transaction)
        {
            ExcpHelper.ThrowIf(this.Table == null, "TableSpec is not specified for type {0}", this.GetType());

            using (DataTable dtInsert = DataCopyTables.GetEmptyDataTableByName(conn, transaction, this.Table.TableName))
            {
                using (IDbCommand cmdInsert = DataCopy.GenerateInsertCommand(conn, transaction, dtInsert, this.Table))
                {
                    using (IDisposable dsp = SqlObjectFactory.CreateDbAdapter(null) as IDisposable)
                    {
                        IDbDataAdapter daInsert = dsp as IDbDataAdapter;
                        Debug.Assert(daInsert != null);

                        daInsert.InsertCommand = cmdInsert;

                        dtInsert.AcceptChanges();

                        DataRow dr = this.CreateDataRow(dtInsert);

                        dtInsert.Rows.Add(dr);

                        using (DataSet ds = new DataSet())
                        {
                            ds.Tables.Add(dtInsert);
                            daInsert.Update(ds);
                        }

                        if (this.Table.IsAutoGeneratedIdentity)
                        {
                            Debug.Assert(this.Table.IdentityNames.Count > 0);
                            m_objJustInsertedIdentity = ConnectionManager.GetLastInsertId(conn, transaction, this.Table.IdentityNames[0]);
                        }
                    }
                }
            }
        }

        public virtual void Delete()
        {
            this.Delete(ConnectionManager.GetConnection(), null);
        }

        public virtual void Delete(IDbConnection conn, IDbTransaction transaction)
        {
            //DataCopy.ExecuteScalar(conn, transaction, "DELETE FROM {0} WHERE {1} = {2}", this.Table.TableName, this.Table.LocalIdentityName, this.ORMID);
        }


       

    }


    [XmlRoot("state", Namespace = LineSerializeHelper.DEFAULT_NAMESPACE, IsNullable = true)]
    public class EmptyExternalState
    {
    }
}
