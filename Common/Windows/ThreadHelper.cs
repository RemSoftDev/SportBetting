using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;

namespace SportRadar.Common.Windows
{
    public sealed class ThreadContext
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(ThreadContext));

        protected string m_sThreadName = string.Empty;
        protected object m_oParam = null;
        protected DelegateThread m_dt = null;

        public ThreadContext(string sThreadName, DelegateThread dt, object oParam)
        {
            Debug.Assert(!string.IsNullOrEmpty(sThreadName));
            Debug.Assert(dt != null);

            m_sThreadName = sThreadName;
            m_oParam = oParam;
            m_dt = dt;
        }

        public string ThreadName { get { return m_sThreadName; } }
        public object Param { get { return m_oParam; } }
        public DelegateThread Method { get { return m_dt; } }

        public Exception Error { get; set; }
        public string ErrorMessage { get; set; }
        public object Result { get; set; }
        public object Tag { get; set; }
        public int ManagedThreadId { get; set; }

        private object m_objLocker = new Object();
        private bool m_bIsToStop = false;
        private bool m_bIsRunning = false;

        public bool IsToStop
        {
            get
            {
                lock (m_objLocker)
                {
                    return m_bIsToStop;
                }
            }
        }

        public bool IsRunning
        {
            get
            {
                lock (m_objLocker)
                {
                    return m_bIsRunning;
                }
            }
        }

        internal protected void SetRunning(bool bIsRunning)
        {
            lock (m_objLocker)
            {
                m_bIsRunning = bIsRunning;
            }
        }

        public void RequestToStop()
        {
            lock (m_objLocker)
            {
                m_bIsToStop = true;
            }

            m_logger.InfoFormat("{0} requested to stop by other thread {1}", this, Thread.CurrentThread.ManagedThreadId);
        }

        public override string ToString()
        {
            return string.Format("Thread {{Name='{0}', Id={1}, Method='{2}', Parameter='{3}', Result='{4}', IsToStop={5}, IsRunning={6}, Tag = {7}, Failed={8}}}",
                m_sThreadName,
                this.ManagedThreadId,
                m_dt.Method,
                this.Param,
                this.Result,
                this.IsToStop,
                this.IsRunning,
                this.Tag,
                this.Error != null || !string.IsNullOrEmpty(this.ErrorMessage) ? "Yes" : "No");
        }
    }

    public delegate void DelegateThread(ThreadContext tc);
    public delegate bool DelegateRequestToStop(ThreadContext tc);

    public static class ThreadHelper
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(ThreadHelper));
        private static object m_oLocker = new object();
        private static SyncDictionary<int, ThreadContext> m_diIdToContext = new SyncDictionary<int, ThreadContext>();
        private static SyncDictionary<string, ThreadContext> m_diNameToContext = new SyncDictionary<string, ThreadContext>();

        public static event DelegateThread ThreadStarted = null;
        public static event DelegateThread ThreadCompleted = null;
        public static event DelegateThread ThreadError = null;

        public static ThreadContext RunThread(string sThreadName, DelegateThread dt, ThreadPriority priority = ThreadPriority.Normal)
        {
            return ThreadHelper.RunThread(sThreadName, dt, null, priority);
        }

        private enum eAction
        {
            Started = 0,
            Completed = 1,
            Error = 2,
        }

        private static void ProcessEvent(eAction action, DelegateThread dt, ThreadContext tc)
        {
            if (dt != null)
            {
                lock (m_oLocker)
                {
                    switch (action)
                    {
                        case eAction.Started:

                            ExcpHelper.ThrowIf(m_diIdToContext.ContainsKey(tc.ManagedThreadId), "Thread is running already {0}", tc);
                            ExcpHelper.ThrowIf(m_diNameToContext.ContainsKey(tc.ThreadName), "Thread is running already {0}", tc);

                            m_diIdToContext.Add(tc.ManagedThreadId, tc);
                            m_diNameToContext.Add(tc.ThreadName, tc);

                            Debug.Assert(m_diIdToContext.Count == m_diNameToContext.Count);

                            break;

                        case eAction.Completed:

                            ExcpHelper.ThrowIf(!m_diIdToContext.ContainsKey(tc.ManagedThreadId), "Thread is not running {0}", tc);
                            ExcpHelper.ThrowIf(!m_diNameToContext.ContainsKey(tc.ThreadName), "Thread is not running {0}", tc);

                            m_diIdToContext.Remove(tc.ManagedThreadId);
                            m_diNameToContext.Remove(tc.ThreadName);

                            Debug.Assert(m_diIdToContext.Count == m_diNameToContext.Count);

                            break;

                        case eAction.Error:


                            Debug.Assert(m_diIdToContext.Count == m_diNameToContext.Count);
                            break;

                        default:

                            Debug.Assert(false);
                            break;
                    }

                    try
                    {
                        if (dt != null)
                        {
                            dt(tc);
                        }
                    }
                    catch (Exception excp)
                    {
                        m_logger.Error(ExcpHelper.FormatException(excp, "ProcessEvent {0} ERROR for {1}:\r\n{2}\r\n{3}", action, tc, excp.Message, excp.StackTrace),excp);
                    }
                }
            }
        }

        public static ThreadContext RequestToStopByThreadName(string sThreadName)
        {
            lock (m_oLocker)
            {
                ThreadContext tc = m_diNameToContext.ContainsKey(sThreadName) ? m_diNameToContext[sThreadName] : null;

                if (tc != null)
                {
                    tc.RequestToStop();

                    return tc;
                }

                return null;
            }
        }

        public static ThreadContext GetByName(string sThreadName)
        {
            lock (m_oLocker)
            {
                return m_diNameToContext.ContainsKey(sThreadName) ? m_diNameToContext[sThreadName] : null;
            }
        }

        public static ThreadContext GetById(int iManagedThreadId)
        {
            lock (m_oLocker)
            {
                return m_diIdToContext.ContainsKey(iManagedThreadId) ? m_diIdToContext[iManagedThreadId] : null;
            }
        }

        public static void StopAll()
        {
            m_diIdToContext.SafelyForEach(delegate(ThreadContext threadContext)
            {
                threadContext.RequestToStop();

                return false;
            });
        }

        public static ThreadContext RunThread(string sThreadName, DelegateThread dt, object objParam, ThreadPriority priority = ThreadPriority.Normal)
        {
            try
            {
                ThreadContext tc = null;

                lock (m_oLocker)
                {
                    tc = m_diNameToContext.ContainsKey(sThreadName) ? m_diNameToContext[sThreadName] : null;
                }

                if (tc == null)
                {
                    tc = new ThreadContext(sThreadName, dt, objParam);

                    Thread thread = new Thread(ThreadHelperThread);
                    thread.IsBackground = true;
                    thread.Priority = priority;
                    thread.Name = sThreadName;
                    thread.Start(tc);
                }

                return tc;
            }
            catch (Exception excp)
            {
                m_logger.Error(ExcpHelper.FormatException(excp, "RunThread('{0}', {1}, {2}) General ERROR", sThreadName, dt.Method, objParam),excp);
            }

            /* DK - ThreadPool is not suitable for long runnung items.
            ThreadPool.QueueUserWorkItem(delegate(object obj)
            {
            }, objParam);
            */
            return null;
        }

        private static void ThreadHelperThread(object objThreadContext)
        {
            ThreadContext tc = objThreadContext as ThreadContext;

            try
            {
                Debug.Assert(tc != null);

                tc.ManagedThreadId = Thread.CurrentThread.ManagedThreadId;
                m_logger.InfoFormat("{0} Started", tc);

                ProcessEvent(eAction.Started, ThreadStarted, tc);
                tc.SetRunning(true);

                try
                {
                    tc.Method(tc);
                }
                catch (Exception excp)
                {
                    tc.Error = excp;
                    m_logger.ErrorFormat("ERROR in {0}:\r\n{1}\r\n{2}",excp, tc, excp.Message, excp.StackTrace);

                    ProcessEvent(eAction.Error, ThreadError, tc);
                }

                m_logger.InfoFormat("{0} Completed", tc);

                tc.SetRunning(false);
                ProcessEvent(eAction.Completed, ThreadCompleted, tc);
            }
            catch (Exception excp)
            {
                tc.SetRunning(false);
                m_logger.Error(ExcpHelper.FormatException(excp, "ThreadHelperThread({0}) General ERROR", objThreadContext),excp);
            }
        }

        public static void LogThreads()
        {
            string sInfo = string.Empty;

            lock (m_oLocker)
            {
                sInfo += string.Format("\r\nThreadHelper(Count = {0}) thread(s):\r\n", m_diIdToContext.Count);

                foreach (ThreadContext tc in m_diIdToContext.Values)
                {
                    sInfo += tc.ToString() + "\r\n";
                }
            }

            m_logger.Debug(sInfo);
        }
    }
}