using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportRadar.Common.Collections
{
    public class SyncDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISafelyForeach<TValue>
    {
        protected Dictionary<TKey, TValue> m_di = new Dictionary<TKey, TValue>();
        protected object m_oLocker = new object();

        public object Locker { get { return m_oLocker; } }

        public virtual void Add(TKey key, TValue value)
        {
            lock (m_oLocker)
            {
                m_di.Add(key, value);
            }
        }

        public virtual SyncList<TKey> KeysToSyncList()
        {
            lock (m_oLocker)
            {
                return new SyncList<TKey>(m_di.Keys);
            }
        }

        public virtual SyncList<TValue> ToSyncList()
        {
            lock (m_oLocker)
            {
                return new SyncList<TValue>(m_di.Values);
            }
        }

        public virtual bool SafelyAdd(TKey key, TValue value)
        {
            lock (m_oLocker)
            {
                if (!m_di.ContainsKey(key))
                {
                    m_di.Add(key, value);
                    return true;
                }

                return false;
            }
        }

        public virtual bool ContainsKey(TKey key)
        {
            lock (m_oLocker)
            {
                return m_di.ContainsKey(key);
            }
        }

        public virtual ICollection<TKey> Keys
        {
            get
            {
                lock (m_oLocker)
                {
                    return m_di.Keys;
                }
            }
        }

        public virtual bool Remove(TKey key)
        {
            lock (m_oLocker)
            {
                return m_di.Remove(key);
            }
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            lock (m_oLocker)
            {
                return m_di.TryGetValue(key, out value);
            }
        }

        public virtual TValue SafelyGetValue(TKey key)
        {
            lock (m_oLocker)
            {
                return m_di.ContainsKey(key) ? m_di[key] : default(TValue);
            }
        }

        public virtual ICollection<TValue> Values
        {
            get
            {
                lock (m_oLocker)
                {
                    return m_di.Values;
                }
            }
        }

        public virtual TValue this[TKey key]
        {
            get
            {
                lock (m_oLocker)
                {
                    return m_di[key];
                }
            }
            set
            {
                lock (m_oLocker)
                {
                    m_di[key] = value;
                }
            }
        }

        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (m_oLocker)
            {
                m_di.Add(item.Key, item.Value);
            }
        }

        public virtual void Clear()
        {
            lock (m_oLocker)
            {
                m_di.Clear();
            }
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (m_oLocker)
            {
                return m_di.Contains(item);
            }
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int iIndex)
        {
            throw new NotImplementedException();
        }

        public virtual int Count
        {
            get
            {
                lock (m_oLocker)
                {
                    return m_di.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (m_oLocker)
            {
                return m_di.Remove(item.Key);
            }
        }

        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (m_oLocker)
            {
                return m_di.GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock (m_oLocker)
            {
                return m_di.GetEnumerator();
            }
        }

        /// <summary>
        /// ZB: Prevents “Collection was modified after the enumerator was instantiated” exception
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<TKey, TValue>> Snapshot()
        {
            TKey[] keys = null;

            lock (m_oLocker)
            {
                keys = m_di.Keys.ToArray();
            }

            foreach (var key in (keys ?? new TKey[0]))
            {
                TValue value;
                if (this.TryGetValue(key, out value))
                {
                    yield return new KeyValuePair<TKey, TValue>(key, value);
                }
            }

            yield break;
        }

        public SyncDictionary<TKey, TValue> ToSyncDictionary()
        {
            var dict = new SyncDictionary<TKey, TValue>();
            lock (m_oLocker)
            {

                foreach (var value in this.m_di)
                {
                    dict.Add(value);
                }
            }
            return dict;

        }


        public virtual TValue SafelyForEach(DelegateForEach<TValue> dfe)
        {
            lock (m_oLocker)
            {
                foreach (TValue item in m_di.Values)
                {
                    if (dfe(item))
                    {
                        return item;
                    }
                }
            }

            return default(TValue);
        }

        public override string ToString()
        {
            return string.Format("{0}<{1}, {2}> {{Count={3}}}", this.GetType().Name, typeof(TKey).Name, typeof(TValue).Name, this.Count);
        }
    }
}
