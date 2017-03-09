using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportRadar.Common.Collections
{
    public class SyncHashSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        protected HashSet<T> m_hs = null;
        protected object m_oLocker = new object();

        public SyncHashSet(IEnumerable<T> collection)
        {
            m_hs = new HashSet<T>(collection);
        }

        public SyncHashSet()
        {
            m_hs = new HashSet<T>();
        }

        public void SafelySynchronize(IEnumerable<T> collection)
        {
            lock (m_oLocker)
            {
                foreach (T obj in collection)
                {
                    if (!m_hs.Contains(obj))
                    {
                        m_hs.Add(obj);
                    }
                }

                SyncList<T> l = new SyncList<T>(m_hs);

                foreach (T obj in l)
                {
                    if (!collection.Contains(obj))
                    {
                        m_hs.Remove(obj);
                    }
                }
            }
        }

        public SyncList<T> ToSyncList()
        {
            lock (m_oLocker)
            {
                return new SyncList<T>(m_hs);
            }
        }

        public SyncHashSet<T> Clone()
        {
            lock (m_oLocker)
            {
                return new SyncHashSet<T>(m_hs);
            }
        }

        public int Count
        {
            get
            {
                lock (m_oLocker)
                {
                    return m_hs.Count;
                }
            }
        }

        public void Clear()
        {
            lock (m_oLocker)
            {
                m_hs.Clear();
            }
        }

        public virtual void Add(T obj)
        {
            lock (m_oLocker)
            {
                m_hs.Add(obj);
            }
        }

        public virtual void AddUnique(T obj)
        {
            lock (m_oLocker)
            {
                if (!m_hs.Contains(obj))
                {
                    m_hs.Add(obj);
                }
            }
        }

        public virtual bool Remove(T obj)
        {
            lock (m_oLocker)
            {
                return m_hs.Remove(obj);
            }
        }

        public virtual bool Contains(T obj)
        {
            lock (m_oLocker)
            {
                return m_hs.Contains(obj);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (m_oLocker)
            {
                m_hs.CopyTo(array, arrayIndex);
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public HashSet<T>.Enumerator GetEnumerator()
        {
            return m_hs.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return m_hs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_hs.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Format("{0}<{1}> {{Count={2}}}", this.GetType().Name, typeof(T).Name, this.Count);
        }
    }
}
