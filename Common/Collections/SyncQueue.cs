using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportRadar.Common.Collections
{
    public class SyncQueue<T> : Queue<T>
    {
        private Queue<T> m_q = new Queue<T>();
        private object m_Locker = new Object();

        public virtual void Clear()
        {
            lock (m_Locker)
            {
                m_q.Clear();
            }
        }

        public virtual T Dequeue()
        {
            lock (m_Locker)
            {
                var item = m_q.Dequeue();
                return item;
            }
        }

        public virtual int Enqueue(T value)
        {
            lock (m_Locker)
            {
                m_q.Enqueue(value);
                return m_q.Count;
            }
        }

        public virtual IEnumerator GetEnumerator()
        {
            lock (m_Locker)
            {
                return m_q.GetEnumerator();
            }
        }

        public virtual object Peek()
        {
            lock (m_Locker)
            {
                return m_q.Peek();
            }
        }

        public virtual T[] ToArray()
        {
            lock (m_Locker)
            {
                return m_q.ToArray();
            }
        }

        public int Count
        {
            get
            {
                lock (m_Locker)
                {
                    return m_q.Count;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("SyncQueue<{0}> {{Count = {1}}}", typeof(T).Name, m_q.Count);
        }
    }
}
