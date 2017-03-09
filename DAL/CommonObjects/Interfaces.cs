using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SportRadar.Common.Collections;

namespace SportRadar.DAL.CommonObjects
{
    public enum eLineType
    {
        None = 0,
        PreMatches = 1,
        LiveMatches = 2,
        All = 3
    }

    // Line Provider
    public interface ILineProvider
    {
        string UniqueName { get; }
        void Initialize(object objParam);
        void Run(eLineType elt);
        void Stop(eLineType elt);
        void Clear(eLineType elt);
    }

    // Line Object Inteerfaces
    public interface ILineObject<T>
    {
        bool IsNew { get; }
        ObservablePropertyList ChangedProps { get; }
        void FillFromDataRow(DataRow dr);
        DataRow CreateDataRow(DataTable dtSample);
        void MergeFrom(T objSource);
        void MergeFrom(ISerializableObject so);
        long UpdateId { get; set; }
        void NotifyPropertiesChanged();
        void UnsetPropertiesChanged();
        void SetRelations();
        ISerializableObject Serialize();
        void Deserialize(ISerializableObject so);
    }

    public interface ILineObjectWithId<T> : ILineObject<T>
    {
        long Id { get; }
    }

    public interface ILineObjectWithKey<T> : ILineObject<T>
    {
        string KeyName { get; }
    }

    public interface IRemovableLineObject<T> : ILineObject<T>
    {
        long RemoveId { get; }
    }

    // Line Object Collection Interfaces
    public interface ILineObjectCollection
    {
        Type ObjectType { get; }
        void Clear();
        int Count { get; }
    }

    //public delegate T DelegateGetLineObject<T>(ISerializableObject so) where T : ILineObject<T>;

    public enum eOperationMask
    {
        AddedToCollection = 1,
        ObjectEdited = 2,
        RemovedFromCollection = 4,
        MatchPeriodInfoChanged = 8,
    }

    public interface ILineObjectCollection<T> : ILineObjectCollection where T : ILineObject<T>
    {
        T MergeLineObject(T objTarget, ISerializableObject so);
        T MergeLineObject(T objSource);
        void AddStrictly(T objSource);
        void AddSafely(T objSource);
        SyncList<T> ToSyncList();
    }

    public interface ILineObjectList<T> where T: ILineObject<T>
    {
        void SafelyAdd(T objSource);
    }

    public interface ILineObjectDictionaryById<T> : ILineObjectCollection<T> where T : ILineObjectWithId<T>
    {
        bool ContainsKey(long lKey);
        T GetObject(long lKey);
    }

    public interface ILineObjectDictionaryByKey<T> : ILineObjectCollection<T> where T : ILineObjectWithKey<T>
    {
        bool ContainsKey(string sKey);
        T GetObject(string sKey);
    }

    public interface ISerializableObject
    {
        SerializableProperty GetSerializableProperty(string sPropertyName);
        SyncList<SerializableProperty> GetProperties();
        void ToXml(XmlTextWriter tw);
        void FromXml(XmlTextReader tr);
        string XmlTagName { get; }
        bool IsToRemove();
        //object MethodTag { get; set; }
        //T GetObject<T>() where T : ILineObject<T>;
    }
}
