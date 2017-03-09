using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.NewLineObjects
{
    public sealed class DictionaryOfLineObjectCollectionLight : SyncDictionary<Type, ILineObjectCollection>
    {
        public void SafelyAddObject<T>(T obj) where T : ILineObject<T>
        {
            Type type = typeof(T);

            LineObjectCollection<T> collection = null;

            lock (m_oLocker)
            {
                if (m_di.ContainsKey(type))
                {
                    collection = m_di[type] as LineObjectCollection<T>;
                }
                else
                {
                    collection = new LineObjectCollection<T>();
                    m_di.Add(type, collection);
                }
            }

            Debug.Assert(collection != null);

            collection.AddSafely(obj);
        }

        public bool ContainsObjectById<T>(long lId) where T : ILineObject<T>
        {
            return this.ContainsObjectByKey<T>(lId.ToString("G"));
        }

        public bool ContainsObjectByKey<T>(string sKey) where T : ILineObject<T>
        {
            Type type = typeof(T);
            LineObjectCollection<T> collection = null;

            lock (m_oLocker)
            {
                collection = m_di.ContainsKey(type) ? m_di[type] as LineObjectCollection<T> : null;
            }

            if (collection != null)
            {
                return collection.ContainsKey(sKey);
            }

            return false;
        }

        public LineObjectCollection<T> GetLineObjectCollection<T>() where T : ILineObject<T>
        {
            Type type = typeof(T);

            return this.ContainsKey(type) ? this[type] as LineObjectCollection<T> : null;
        }

        private void NotifyPropertiesChanged<T>() where T : ILineObject<T>
        {
            Type type = typeof(T);

            if (this.ContainsKey(type))
            {
                LineObjectCollection<T> loc = this[type] as LineObjectCollection<T>;

                foreach (T obj in loc.Values)
                {
                    obj.NotifyPropertiesChanged();
                }
            }
        }

        private void UnsetPropertiesChanged<T>() where T : ILineObject<T>
        {
            Type type = typeof(T);

            if (this.ContainsKey(type))
            {
                LineObjectCollection<T> loc = this[type] as LineObjectCollection<T>;

                foreach (T obj in loc.Values)
                {
                    obj.UnsetPropertiesChanged();
                }
            }
        }

        public void NotifyPropertiesChanged()
        {
            NotifyPropertiesChanged<TaggedStringLn>();
            NotifyPropertiesChanged<GroupLn>();
            NotifyPropertiesChanged<MatchLn>();
            NotifyPropertiesChanged<LiveMatchInfoLn>();
            NotifyPropertiesChanged<BetDomainLn>();
            NotifyPropertiesChanged<OddLn>();
            NotifyPropertiesChanged<MatchResultLn>();
        }

        public void UnsetPropertiesChanged()
        {
            UnsetPropertiesChanged<TaggedStringLn>();
            UnsetPropertiesChanged<GroupLn>();
            UnsetPropertiesChanged<MatchLn>();
            UnsetPropertiesChanged<LiveMatchInfoLn>();
            UnsetPropertiesChanged<BetDomainLn>();
            UnsetPropertiesChanged<OddLn>();
            UnsetPropertiesChanged<MatchResultLn>();
        }

        public override string ToString()
        {
            List<string> lResult = new List<string>();

            foreach (Type type in this.Keys)
            {
                ILineObjectCollection loc = this[type];

                lResult.Add(string.Format("'{0}({1})'", type.Name, loc.Count));
            }

            return string.Format("DataCopyQueueItemDictionary {{{0}}}", string.Join(",", lResult.ToArray()));
        }
    }

    public sealed class DictionaryOfLineObjectCollection : SyncDictionary<Type, ILineObjectCollection>
    {
        private TaggedStringDictionary m_diTaggedStrings = null;
        private TimeTypeDictionary m_diTimeTypes = null;
        private ScoreTypeDictionary m_diScoreTypes = null;
        private BetTypeDictionary m_diBetTypes = null;
        private BetDomainTypeDictionary m_diBetDomainTypes = null;
        private GroupDictionary m_diGroups = null;
        private CompetitorDictionary m_diCompetitors = null;
        private CompetitorToOutrightDictionary m_diCompetitorsToOutright = null;
        private MatchDictionary m_diMatches = null;
        private MatchResultDictionary m_diMatchResults = null;
        private LiveMatchInfoDictionary m_diLiveMatchInfos = null;
        private MatchToGroupDictionary m_diMatchesToGroups = null;
        private BetDomainDictionary m_diBetDomains = null;
        private OddDictionary m_diOdds = null;
        private ResourceRepositoryDictionary m_diResources = null;
        private ResourceAssignmentDictionary m_diResourceAssignments = null;
        private CompetitorInfosDictionary m_diCompetitorInfos = null;
        private MatchInfosDictionary m_diMatchesInfos = null;
        private TournamentInfosDictionary m_diTournamentInfos = null;
        private LiabilitiesDictionary m_diLiabilities = null;
        private LanguageDictionary m_diLanguages = null;
        private MultistringGroupDictionary m_MultistringGroups = null;
        private TournamentMatchLocksDictionary m_diTournamentMatchLocks = null;
        private ActiveTournamentsDictionary m_diActiveTournaments = null;

        [Obsolete]
        public new void Clear()
        {

        }

        public void ClearAll()
        {
            foreach (ILineObjectCollection loc in this.Values)
            {
                loc.Clear();
            }

            base.Clear();
        }

        [Obsolete]
        public new void Add(Type type, ILineObjectCollection collection)
        {
            throw new Exception("Method DictionaryOfLineObjectCollection.Add() is obsolete. Use AddCollection() instead.");
        }

        public void AddCollection(ILineObjectCollection collection)
        {
            base.Add(collection.ObjectType, collection);
        }

        public void Initialize()
        {
            m_diTaggedStrings = new TaggedStringDictionary();
            m_diTimeTypes = new TimeTypeDictionary();
            m_diScoreTypes = new ScoreTypeDictionary();
            m_diBetTypes = new BetTypeDictionary();
            m_diBetDomainTypes = new BetDomainTypeDictionary();
            m_diGroups = new GroupDictionary();
            m_diCompetitors = new CompetitorDictionary();
            m_diCompetitorsToOutright = new CompetitorToOutrightDictionary();
            m_diMatches = new MatchDictionary();
            m_diMatchResults = new MatchResultDictionary();
            m_diLiveMatchInfos = new LiveMatchInfoDictionary();
            m_diMatchesToGroups = new MatchToGroupDictionary();
            m_diBetDomains = new BetDomainDictionary();
            m_diResources = new ResourceRepositoryDictionary();
            m_diResourceAssignments = new ResourceAssignmentDictionary();
            m_diCompetitorInfos = new CompetitorInfosDictionary();
            m_diMatchesInfos = new MatchInfosDictionary();
            m_diTournamentInfos = new TournamentInfosDictionary();
            m_diLiabilities = new LiabilitiesDictionary();
            m_diOdds = new OddDictionary();
            m_diLanguages = new LanguageDictionary();
            m_MultistringGroups = new MultistringGroupDictionary();
            m_diTournamentMatchLocks = new TournamentMatchLocksDictionary();
            m_diActiveTournaments = new ActiveTournamentsDictionary();

            AddCollection(m_diTaggedStrings);
            AddCollection(m_diTimeTypes);
            AddCollection(m_diScoreTypes);
            AddCollection(m_diBetTypes);
            AddCollection(m_diBetDomainTypes);
            AddCollection(m_diGroups);
            AddCollection(m_diCompetitors);
            AddCollection(m_diCompetitorsToOutright);
            AddCollection(m_diMatches);
            AddCollection(m_diMatchResults);
            AddCollection(m_diMatchesToGroups);
            AddCollection(m_diLiveMatchInfos);
            AddCollection(m_diBetDomains);
            AddCollection(m_diResources);
            AddCollection(m_diResourceAssignments);
            AddCollection(m_diCompetitorInfos);
            AddCollection(m_diMatchesInfos);
            AddCollection(m_diTournamentInfos);
            AddCollection(m_diLiabilities);
            AddCollection(m_diOdds);
            AddCollection(m_diLanguages);
            AddCollection(m_MultistringGroups);
            AddCollection(m_diTournamentMatchLocks);
            AddCollection(m_diActiveTournaments);
        }

        public TaggedStringDictionary TaggedStrings { get { return m_diTaggedStrings; } }
        public TimeTypeDictionary TimeTypes { get { return m_diTimeTypes; } }
        public ScoreTypeDictionary ScoreTypes { get { return m_diScoreTypes; } }
        public BetTypeDictionary BetTypes { get { return m_diBetTypes; } }
        public BetDomainTypeDictionary BetDomainTypes { get { return m_diBetDomainTypes; } }
        public GroupDictionary Groups { get { return m_diGroups; } }
        public CompetitorDictionary Competitors { get { return m_diCompetitors; } }
        public CompetitorToOutrightDictionary CompetitorsToOutright { get { return m_diCompetitorsToOutright; } }
        public MatchDictionary Matches { get { return m_diMatches; } }
        public MatchResultDictionary MatchResults { get { return m_diMatchResults; } }
        public LiveMatchInfoDictionary LiveMatchInfos { get { return m_diLiveMatchInfos; } }
        public MatchToGroupDictionary MatchesToGroups { get { return m_diMatchesToGroups; } }
        public BetDomainDictionary BetDomains { get { return m_diBetDomains; } }
        public OddDictionary Odds { get { return m_diOdds; } }
        public ResourceRepositoryDictionary Resources { get { return m_diResources; } }
        public ResourceAssignmentDictionary ResourceAssignments { get { return m_diResourceAssignments; } }
        public CompetitorInfosDictionary CompetitorInfos { get { return m_diCompetitorInfos; } }
        public MatchInfosDictionary MatchInfos { get { return m_diMatchesInfos; } }
        public TournamentInfosDictionary TournamentInfos { get { return m_diTournamentInfos; } }
        public LiabilitiesDictionary Liabilities { get { return m_diLiabilities; } }
        public LanguageDictionary Languages { get { return m_diLanguages; } }
        public MultistringGroupDictionary MultistringGroups { get { return m_MultistringGroups; } }
        public TournamentMatchLocksDictionary TournamentMatchLocks { get { return m_diTournamentMatchLocks; } }
        public ActiveTournamentsDictionary ActiveTournaments { get { return m_diActiveTournaments; } }

        public ILineObjectCollection<T> GetLineObjectCollection<T>() where T : ILineObject<T>
        {
            Type type = typeof(T);

            ExcpHelper.ThrowIf(!base.ContainsKey(type), "DictionaryOfLineObjectCollection does not contain collection for type '{0}'", type);

            return this[type] as ILineObjectCollection<T>;
        }

        public ILineObjectDictionaryById<T> GetLineObjectDictionaryById<T>() where T : ILineObjectWithId<T>
        {
            ILineObjectDictionaryById<T> di = GetLineObjectCollection<T>() as ILineObjectDictionaryById<T>;

            ExcpHelper.ThrowIf(di == null, "DictionaryOfLineObjectCollection does not contain Dictionary(long Id) for type '{0}'", typeof(T));

            return di;
        }

        public ILineObjectDictionaryByKey<T> GetLineObjectDictionaryByKey<T>() where T : ILineObjectWithKey<T>
        {
            ILineObjectDictionaryByKey<T> di = GetLineObjectCollection<T>() as ILineObjectDictionaryByKey<T>;

            ExcpHelper.ThrowIf(di == null, "DictionaryOfLineObjectCollection does not contain Dictionary(string Key) for type '{0}'", typeof(T));

            return di;
        }

        public bool ContainsKey<T>(long lId) where T : ILineObjectWithId<T>
        {
            return GetLineObjectDictionaryById<T>().ContainsKey(lId);
        }

        public T GetObject<T>(long lId) where T : ILineObjectWithId<T>
        {
            ILineObjectDictionaryById<T> di = GetLineObjectDictionaryById<T>();

            return di.ContainsKey(lId) ? di.GetObject(lId) : default(T);
        }

        public bool ContainsKey<T>(string sKey) where T : ILineObjectWithKey<T>
        {
            return GetLineObjectDictionaryByKey<T>().ContainsKey(sKey);
        }

        public T GetObject<T>(string sKey) where T : ILineObjectWithKey<T>
        {
            ILineObjectDictionaryByKey<T> di = GetLineObjectDictionaryByKey<T>();

            return di.ContainsKey(sKey) ? di.GetObject(sKey) : default(T);
        }

        public int GetCount<T>() where T : ILineObject<T>
        {
            return GetLineObjectCollection<T>().Count;
        }
    }

    public class LineObjectList<T> : HashSet<T>, ILineObjectList<T> where T : ILineObject<T>
    {
        protected ILog m_logger = LogFactory.CreateLog(typeof(LineObjectList<T>));
        protected Type m_type = typeof(T);
        protected object m_objLocker = new object();

        public new void Add(T objSource)
        {
            ExcpHelper.ThrowIf(true, "Method LineObjectList<{0}>.Add() is obsolete. Use method AddObject instead of.", m_type);
        }

        public new int Count
        {
            get
            {
                lock (m_objLocker)
                {
                    return base.Count;
                }
            }
        }

        public delegate void DelegateForEach<T>(T objSource);

        public void SafelyForEach(DelegateForEach<T> dfe)
        {
            Debug.Assert(dfe != null);

            lock (m_objLocker)
            {
                foreach (T objSource in this)
                {
                    dfe(objSource);
                }
            }
        }

        public void SafelyAdd(T objSource)
        {
            Debug.Assert(objSource != null);

            lock (m_objLocker)
            {
                if (!this.Contains(objSource))
                {
                    base.Add(objSource);
                }
            }
        }
    }

    public class LineObjectCollection<T> : SyncDictionary<string, T>, ILineObjectCollection<T> where T : ILineObject<T>
    {
        protected ILog m_logger = LogFactory.CreateLog(typeof(LineObjectCollection<T>));

        protected Type m_type = typeof(T);

        public Type ObjectType { get { return m_type; } }

        public static string GetKeyName(T objSource)
        {
            ILineObjectWithId<T> objById = objSource as ILineObjectWithId<T>;

            if (objById != null)
            {
                return objById.Id.ToString("G");
            }

            ILineObjectWithKey<T> objByKey = objSource as ILineObjectWithKey<T>;

            if (objByKey != null)
            {
                return objByKey.KeyName;
            }

            Debug.Assert(false);

            return string.Empty;
        }

        public void AddStrictly(T objSource)
        {
            string sKey = LineObjectCollection<T>.GetKeyName(objSource);

            ExcpHelper.ThrowIf(base.ContainsKey(sKey), "LineObjectCollection<{0}>.AddStrictly({1}) ERROR. Such object already exists in database.", m_type.Name, objSource);

            lock (m_oLocker)
            {
                base.Add(sKey, objSource);
            }
        }

        public void AddSafely(T objSource)
        {
            ILineObjectWithId<T> objById = objSource as ILineObjectWithId<T>;
            ILineObjectWithKey<T> objByKey = objSource as ILineObjectWithKey<T>;

            string sKey = null;

            if (objById != null)
            {
                sKey = objById.Id.ToString("G");
            }
            else if (objByKey != null)
            {
                sKey = objByKey.KeyName;
            }
            else
            {
                Debug.Assert(false);
            }

            lock (m_oLocker)
            {
                if (!base.ContainsKey(sKey))
                {
                    base.Add(sKey, objSource);
                }
                //else
                //{
                //    m_logger.WarnFormat("{0} is skipped because it already exists in collection.", objSource);
                //}
            }
        }

        public virtual T MergeLineObject(T objSource)
        {
            this.AddSafely(objSource);

            return default(T);
        }

        [Obsolete]
        public virtual T MergeLineObject(T objTarget, ISerializableObject so)
        {
            throw new InvalidOperationException("MergeLineObject() is not allowed for this class.");
        }

        public override string ToString()
        {
            return string.Format("LineObjectCollection {{ObjectType='{0}', Count={1}}}", m_type.Name, this.Count);
        }
    }

    public abstract class LineObjectDictionaryByIdBase<T> : SyncDictionary<long, T>, ILineObjectDictionaryById<T> where T : ILineObjectWithId<T>
    {
        protected Type m_type = typeof(T);

        public Type ObjectType { get { return m_type; } }

        [Obsolete]
        public override void Add(long lId, T obj)
        {
            ExcpHelper.ThrowIf(true, "Method LineObjectDictionaryByIdBase<long, {0}>.Add() is obsolete. Use method AddObject instead of.", typeof(T));
        }

        [Obsolete]
        public override T this[long lId]
        {
            get
            {
                ExcpHelper.ThrowIf(true, "Method LineObjectDictionaryByIdBase<long, {0}>.this[] is obsolete. Use method GetObject instead of.", typeof(T));

                return default(T);
            }
        }

        public new int Count
        {
            get
            {
                lock (m_oLocker)
                {
                    return base.Count;
                }
            }
        }

        public T GetObject(long lKey)
        {
            lock (m_oLocker)
            {
                return base.ContainsKey(lKey) ? base[lKey] : default(T);
            }
        }

        protected void AddStrictlyImp(T objSource)
        {
            lock (m_oLocker)
            {
                ExcpHelper.ThrowIf(base.ContainsKey(objSource.Id), "LineObjectCollection<{0}>.AddStrictly({1}) ERROR. Such object already exists in database.", m_type.Name, objSource);

                base.Add(objSource.Id, objSource);
            }
        }

        public virtual void AddStrictly(T objSource)
        {
            this.AddStrictlyImp(objSource);
        }

        protected void AddSafelyImp(T objSource)
        {
            lock (m_oLocker)
            {
                if (!this.ContainsKey(objSource.Id))
                {
                    base.Add(objSource.Id, objSource);
                }
            }
        }

        public virtual void AddSafely(T objSource)
        {
            this.AddSafelyImp(objSource);
        }

        protected T MergeLineObjectImp(T objSource)
        {
            if (m_di.ContainsKey(objSource.Id))
            {
                T objTarget = base[objSource.Id];
                objTarget.MergeFrom(objSource);

                LineSr.Instance.NewOrChangedObjects.SafelyAddObject(objTarget);

                return objTarget;
            }

            objSource.SetRelations();

            m_di.Add(objSource.Id, objSource);
            LineSr.Instance.NewOrChangedObjects.SafelyAddObject(objSource);

            return objSource;
        }

        public virtual T MergeLineObject(T objSource)
        {
            lock (m_oLocker)
            {
                return MergeLineObjectImp(objSource);
            }
        }

        public virtual T MergeLineObject(T objTarget, ISerializableObject so)
        {
            lock (m_oLocker)
            {
                //T objTarget = so.GetObject<T>();
                //ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(objTarget == null, "MergeLineObject<{0}>() ERROR for {1}", typeof(T).Name, so);

                objTarget.MergeFrom(so);

                {
                    LineSr.Instance.NewOrChangedObjects.SafelyAddObject(objTarget);
                }

                return objTarget;
            }
        }
    }

    public abstract class LineObjectDictionaryByKeyBase<T> : SyncDictionary<string, T>, ILineObjectDictionaryByKey<T> where T : ILineObjectWithKey<T>
    {
        protected Type m_type = typeof(T);

        public Type ObjectType { get { return m_type; } }

        [Obsolete]
        public override void Add(string sKey, T obj)
        {
            ExcpHelper.ThrowIf(true, "Method LineObjectDictionaryByIdBase<string, {0}>.Add() is obsolete. Use method AddObject() instead of.", typeof(T));
        }

        [Obsolete]
        public override T this[string sKey]
        {
            get
            {
                ExcpHelper.ThrowIf(true, "Method LineObjectDictionaryByIdBase<long, {0}>.this[] is obsolete. Use method GetObject() instead of.", typeof(T));

                return default(T);
            }
        }

        public new int Count
        {
            get
            {
                lock (m_oLocker)
                {
                    return base.Count;
                }
            }
        }

        protected void AddObject(T obj)
        {
            base.Add(obj.KeyName, obj);
        }

        public T GetObject(string sKey)
        {
            lock (m_oLocker)
            {
                return base.ContainsKey(sKey) ? base[sKey] : default(T);
            }
        }

        public void AddStrictly(T objSource)
        {
            ExcpHelper.ThrowIf(base.ContainsKey(objSource.KeyName), "LineObjectCollection<{0}>.AddStrictly({1}) ERROR. Such object already exists in database.", m_type.Name, objSource);

            base.Add(objSource.KeyName, objSource);
        }

        public void AddSafely(T objSource)
        {
            lock (m_oLocker)
            {
                if (!this.ContainsKey(objSource.KeyName))
                {
                    base.Add(objSource.KeyName, objSource);
                }
            }
        }

        protected T MergeLineObjectImp(T objSource)
        {
            if (base.ContainsKey(objSource.KeyName))
            {
                T objTarget = base[objSource.KeyName];
                objTarget.MergeFrom(objSource);

                {
                    LineSr.Instance.NewOrChangedObjects.SafelyAddObject(objTarget);
                }

                return objTarget;
            }

            objSource.SetRelations();

            base.Add(objSource.KeyName, objSource);
            LineSr.Instance.NewOrChangedObjects.SafelyAddObject(objSource);

            return objSource;
        }

        public virtual T MergeLineObject(T objSource)
        {
            lock (m_oLocker)
            {
                return MergeLineObjectImp(objSource);
            }
        }

        public virtual T MergeLineObject(T objTarget, ISerializableObject so)
        {
            lock (m_oLocker)
            {
                //T objTarget = so.GetObject<T>();
                //ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(objTarget == null, "MergeLineObject<{0}>() ERROR for {1}", typeof(T).Name, so);

                objTarget.MergeFrom(so);

                {
                    LineSr.Instance.NewOrChangedObjects.SafelyAddObject(objTarget);
                }

                return objTarget;
            }
        }
    }
}
