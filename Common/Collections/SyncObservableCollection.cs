using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;

namespace SportRadar.Common.Collections
{
    public class SyncObservableCollection<T> : SyncList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(SyncObservableCollection<T>));

        protected const string COUNT_PROPERTY_NAME = "Count";
        protected const string ITEM_PROPERTY_NAME = "Item[]";

        public event NotifyCollectionChangedEventHandler CollectionChanged = null;
        public event PropertyChangedEventHandler PropertyChanged = null;

        public delegate bool DelegateFilter(T obj);

        public DelegateFilter Filter { get; set; }

        //protected SimpleMonitor m_monitor = new SimpleMonitor();

        public SyncObservableCollection() : base()
        {
        }

        public SyncObservableCollection(ICollection<T> collection) : base (collection)
        {
        }

        public object Locker { get { return m_oLocker; } }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, e);
            }
        }

        protected static string GetCollectionChangedInfo(NotifyCollectionChangedEventArgs e)
        {
            return string.Format("NotifyCollectionChangedEventArgs {{Action={0}, OldItem='{1}', NewItem='{2}', OldIndex={3}, NewIndex={4}}}",
                e.Action, e.OldItems != null && e.OldItems.Count > 0 ? e.OldItems[0] : null, e.NewItems != null && e.NewItems.Count > 0 ? e.NewItems[0] : null, e.OldStartingIndex, e.NewStartingIndex);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
            {
                //using (BlockReentrancy())
                //{
                try
                {
                        this.CollectionChanged(this, e);
                                }
                catch (Exception excp)
                {
                    m_logger.Excp(excp, "OnCollectionChanged() ERROR for {0}", GetCollectionChangedInfo(e));
                    throw;
                }
                //}
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, object obj)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, obj));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, object obj, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, obj, index));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, object obj, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, obj, index, oldIndex));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, object oldObj, object newObj, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newObj, oldObj, index));
        }

        protected virtual void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public override T this[int iIndex]
        {
            get
            {
                //CheckReentrancy();

                lock (m_oLocker)
                {
                    return m_list[iIndex];
                }
            }
            set
            {
                //CheckReentrancy();

                lock (m_oLocker)
                {
                    T obj = m_list[iIndex];

                    if (!obj.Equals(value))
                    {
                        m_list[iIndex] = value;

                        OnPropertyChanged(ITEM_PROPERTY_NAME);
                        OnCollectionChanged(NotifyCollectionChangedAction.Replace, obj, value, iIndex);
                    }
                }
            }
        }

        public void AddImp(T obj)
        {
            m_list.Add(obj);

            OnPropertyChanged(COUNT_PROPERTY_NAME);
            OnPropertyChanged(ITEM_PROPERTY_NAME);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, obj, m_list.Count - 1);
        }

        public override void Add(T obj)
        {
            //CheckReentrancy();

            lock (m_oLocker)
            {
                AddImp(obj);
            }
        }

        public override void Clear()
        {
            //CheckReentrancy();

            lock (m_oLocker)
            {
                m_list.Clear();
                OnPropertyChanged(COUNT_PROPERTY_NAME);
                OnPropertyChanged(ITEM_PROPERTY_NAME);
                OnCollectionReset();
            }
        }

        protected bool RemoveImp(T obj)
        {
            int iIndex = m_list.IndexOf(obj);
            bool bResult = m_list.Remove(obj);

            if (bResult)
            {
                OnPropertyChanged(COUNT_PROPERTY_NAME);
                OnPropertyChanged(ITEM_PROPERTY_NAME);
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, obj, iIndex);
            }

            return bResult;
        }

        public override bool Remove(T obj)
        {
            //CheckReentrancy();

            lock (m_oLocker)
            {
                return RemoveImp(obj);
            }
        }

        protected void RemoveAtImp(int iIndex)
        {
            T obj = m_list[iIndex];

            m_list.RemoveAt(iIndex);

            OnPropertyChanged(COUNT_PROPERTY_NAME);
            OnPropertyChanged(ITEM_PROPERTY_NAME);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, obj, iIndex);
        }

        public override void RemoveAt(int iIndex)
        {
            //CheckReentrancy();

            lock (m_oLocker)
            {
                RemoveAtImp(iIndex);
            }
        }

        protected void InsertImp(int iIndex, T obj)
        {
            m_list.Insert(iIndex, obj);

            OnPropertyChanged(COUNT_PROPERTY_NAME);
            OnPropertyChanged(ITEM_PROPERTY_NAME);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, obj, iIndex);
        }

        public override void Insert(int iIndex, T obj)
        {
            //CheckReentrancy();

            lock (m_oLocker)
            {
                InsertImp(iIndex, obj);
            }
        }

        protected void MoveImp(int iOldIndex, int iNewIndex)
        {
            T obj = m_list[iOldIndex];

            m_list.RemoveAt(iOldIndex);
            m_list.Insert(iNewIndex, obj);

            OnPropertyChanged(ITEM_PROPERTY_NAME);
            OnCollectionChanged(NotifyCollectionChangedAction.Move, obj, iNewIndex, iOldIndex);
        }

        protected virtual void Move(int iOldIndex, int iNewIndex)
        {
            //CheckReentrancy();

            lock (m_oLocker)
            {
                MoveImp(iOldIndex, iNewIndex);
            }
        }

        public SyncList<T> ToSyncList()
        {
            lock (m_oLocker)
            {
                return new SyncList<T>(m_list.ToList<T>());
            }
        }

        public void ApplyChanges(IList<T> lItems)
        {
            //CheckReentrancy();
            //CheckTime ct = null;

            lock (m_oLocker)
            {
                try
                {
                    //ct = new CheckTime("ApplyChanges<{0}>(this.Count={1}, sync.Count={2}) entered", typeof(T).Name, m_list.Count, lItems.Count);

                    // Remove
                    for (int i = 0; i < m_list.Count; )
                    {
                        T obj = m_list[i];

                        if (!lItems.Contains(obj))
                        {
                            RemoveAtImp(i);
                        }
                        else
                        {
                            i++;
                        }
                    }

                    //ct.AddEvent("Removed");

                    // Add or Move
                    for (int i = 0; i < lItems.Count; i++)
                    {
                        T obj = lItems[i];
                        int iCurrentIndex = m_list.IndexOf(obj);

                        if (iCurrentIndex < 0)
                        {
                            InsertImp(i, obj);
                        }
                        else if (i != iCurrentIndex)
                        {
                            MoveImp(iCurrentIndex, i);
                        }
                    }

                    //ct.AddEvent("Added and Moved");
                }
                catch (Exception excp)
                {
                    m_logger.Excp(excp, "ApplyChanges<{0}>(lItems.Count={1}, this.Count={2}) ERROR", typeof(T).Name, lItems.Count, m_list.Count);
                    throw;
                }
            }

            //ct.AddEvent("ApplyChanges() Completed");
            //ct.Info(m_logger);
        }

        /*
        protected void CheckReentrancy()
        {
            ExcpHelper.ThrowIf<InvalidOperationException>(m_monitor.Busy && this.CollectionChanged != null && this.CollectionChanged.GetInvocationList().Length > 1, "SyncObservableCollectionReentrancyNotAllowed");
        }

        protected IDisposable BlockReentrancy()
        {
            m_monitor.Enter();
            return m_monitor;
        }

        protected sealed class SimpleMonitor : IDisposable
        {
            private int m_iCount = 0;

            public void Enter()
            {
                ++m_iCount;
            }

            public void Dispose()
            {
                --m_iCount;
            }

            public bool Busy { get { return m_iCount > 0; } }
        }
        */
    }
}
