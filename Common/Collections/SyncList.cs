using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.Common.Windows;

namespace SportRadar.Common.Collections
{
    public class SyncList<T> : IList<T>, ISafelyForeach<T>
    {
        protected List<T> m_list = new List<T>();
        protected object m_oLocker = new object();

        public SyncList(IEnumerable<T> collection)
        {
            m_list = new List<T>(collection);
        }

        public SyncList(List<T> toList)
        {
            m_list = new List<T>(toList);
        }

        public SyncList()
        {
        }

        public SyncList<T> Clone()
        {
            lock (m_oLocker)
            {
                return new SyncList<T>(new List<T>(m_list));
            }
        }

        public virtual int IndexOf(T item)
        {
            lock (m_oLocker)
            {
                return m_list.IndexOf(item);
            }
        }

        public virtual void Insert(int iIndex, T item)
        {
            lock (m_oLocker)
            {
                m_list.Insert(iIndex, item);
            }
        }

        public virtual void RemoveAt(int iIndex)
        {
            lock (m_oLocker)
            {
                m_list.RemoveAt(iIndex);
            }
        }

        public virtual T this[int iIndex]
        {
            get
            {
                lock (m_oLocker)
                {
                    return m_list[iIndex];
                }
            }
            set
            {
                lock (m_oLocker)
                {
                    m_list[iIndex] = value;
                }
            }
        }

        public virtual void Add(T item)
        {
            lock (m_oLocker)
            {
                m_list.Add(item);
            }
        }

        public virtual bool SafelyAdd(T item)
        {
            lock (m_oLocker)
            {
                if (!m_list.Contains(item))
                {
                    m_list.Add(item);

                    return true;
                }
            }

            return false;
        }

        public virtual void Clear()
        {
            lock (m_oLocker)
            {
                m_list.Clear();
            }
        }

        public virtual bool Contains(T item)
        {
            lock (m_oLocker)
            {
                return m_list.Contains(item);
            }
        }

        public virtual void CopyTo(T[] array, int iIndex)
        {
            lock (m_oLocker)
            {
                m_list.CopyTo(array, iIndex);
            }
        }

        public virtual int Count
        {
            get
            {
                lock (m_oLocker)
                {
                    return m_list.Count;
                }
            }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        public virtual bool Remove(T item)
        {
            lock (m_oLocker)
            {
                return m_list.Remove(item);
            }
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            lock (m_oLocker)
            {
                return m_list.GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock (m_oLocker)
            {
                return m_list.GetEnumerator();
            }
        }

        public void Sort(IComparer<T> comparer)
        {
            lock (m_oLocker)
            {
                m_list.Sort(comparer);
            }
        }

        public void Sort(Comparison<T> comparison)
        {
            lock (m_oLocker)
            {
                m_list.Sort(comparison);
            }
        }

        public virtual T SafelyForEach(DelegateForEach<T> dfe)
        {
            lock (m_oLocker)
            {
                foreach (T item in m_list)
                {
                    if (dfe(item))
                    {
                        return item;
                    }
                }
            }

            return default(T);
        }

        protected void SynchronizeImp(IList<T> list)
        {
            ExcpHelper.ThrowIf<ArgumentNullException>(list == null, "SynchronizeImp() ERROR. List is null.");

            for (int i = 0; i < m_list.Count; )
            {
                T obj = m_list[i];

                if (!list.Contains(obj))
                {
                    m_list.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            for (int i = 0; i < list.Count; i ++)
            {
                T obj = list[i];
                int iCurrentIndex = m_list.IndexOf(obj);

                if (iCurrentIndex < 0)
                {
                    // Item is not in current collection => Insert it
                    m_list.Insert(i, obj);
                }
                else if (i != iCurrentIndex)
                {
                    Debug.Assert(i < iCurrentIndex);

                    // Move (Swap)
                    T objTemp = m_list[i];
                    m_list[i] = m_list[iCurrentIndex];
                    m_list[iCurrentIndex] = objTemp;
                }
            }
        }

        public void SafelySynchronize(IList<T> list)
        {
            lock (m_oLocker)
            {
                SynchronizeImp(list);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}<{1}> {{Count={2}}}", this.GetType().Name, typeof(T).Name, this.Count);
        }
    }
}
