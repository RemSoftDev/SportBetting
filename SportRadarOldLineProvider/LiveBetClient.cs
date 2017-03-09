using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using IocContainer;
using Ionic.Zip;
using Ninject;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.Connection;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;
using WsdlRepository;

namespace SportRadar.DAL.SportRadarOldLineProvider
{
    public static class LiveBetClient
    {
        private const string COMPRESSED = "compressed";
        //private const string COMPRESSED_MASK = COMPRESSED + "{0:D8}";
        private const int COMPRESSED_MASK_SIZE = 18;

        private static readonly TimeSpan LIVE_TIME_OUT_TO_DISABLE = new TimeSpan(0, 0, 0, 0, DalStationSettings.Instance.LiveTimeOutToDisableMilliseconds);
        private static readonly TimeSpan LIVE_TIME_OUT_TO_REMOVE = new TimeSpan(0, 0, 0, 0, DalStationSettings.Instance.LiveTimeOutToRemoveMilliseconds);

        public const string READER_THREAD_NAME = "LiveBetClientReaderThread";
        public const string WORKER_THREAD_NAME = "LiveBetClientWorkerThread";
        public const string CLEANER_THREAD_NAME = "LiveBetClientCleanerThread";

        public const string LIVEBET_MSG_HELLO = "Hello";
        public const string LIVEBET_MSG_EMPTY = "Empty";
        //public const string LIVEBET_MSG_OK    = "OK";
        //public const string LIVEBET_MSG_ERROR = "ERROR";

        private static ILog m_logger = LogFactory.CreateLog(typeof(LiveBetClient));

        private static SyncQueue<LineContainer> m_sqMessages = new SyncQueue<LineContainer>();
        private static LiveClient m_liveClient = null;
        private static DateTime m_dtConnected = DateTime.Now;

        private static bool m_bRunning = false;

        private static string m_sHost = null;
        private static int m_iPort = 0;

        private static IStationRepository _stationRepository;
        private static IStationRepository StationRepository
        {
            get { return _stationRepository ?? (_stationRepository = IoCContainer.Kernel.Get<IStationRepository>()); }
        }

        public static void Initialize(string stationNumber)
        {
            m_sHost = DalStationSettings.Instance.LiveBetHost;
            m_iPort = DalStationSettings.Instance.LiveBetPort;

            StationNumber = stationNumber;
            ThreadHelper.ThreadStarted += ThreadHelper_ThreadStarted;
            ThreadHelper.ThreadCompleted += ThreadHelper_ThreadCompleted;
        }

        static void ThreadHelper_ThreadCompleted(ThreadContext tc)
        {
            if (m_bRunning)
            {
                if (tc.ThreadName == READER_THREAD_NAME)
                {
                    ThreadHelper.RunThread(READER_THREAD_NAME, ReaderThread);
                }
                else if (tc.ThreadName == WORKER_THREAD_NAME)
                {
                    ThreadHelper.RunThread(WORKER_THREAD_NAME, WorkerThread);
                }
            }
        }

        static void ThreadHelper_ThreadStarted(ThreadContext tc)
        {
        }

        public static void ChangeHost(string sHost, int iPort)
        {
            m_sHost = sHost;
            m_iPort = iPort;
        }

        public static void Run()
        {
            m_liveClient = new LiveClient(m_sHost, m_iPort, false, string.Empty, null);

            m_bRunning = true;

            ThreadHelper.RunThread(READER_THREAD_NAME, ReaderThread, ThreadPriority.BelowNormal);
            ThreadHelper.RunThread(WORKER_THREAD_NAME, WorkerThread, ThreadPriority.BelowNormal);
            if (ConfigurationManager.AppSettings["betradar_xml_files"] != null)
                ThreadHelper.RunThread(CLEANER_THREAD_NAME, CleanerThread, ThreadPriority.BelowNormal);

        }

        public static void Stop()
        {
            m_bRunning = false;

            ThreadHelper.RequestToStopByThreadName(READER_THREAD_NAME);
            ThreadHelper.RequestToStopByThreadName(WORKER_THREAD_NAME);
        }

        private static void RemoveLiveMatches(eServerSourceType linetype) // DK - PreMatches means live that comes from LiveBetServer like Vfl, Vhc etc
        {
            m_logger.Warn("RemoveLiveMatches() been called.");

            LineSr.SyncRoutines(eUpdateType.LiveBet, "Connection lost. SportRadar LiveMarket is cleared.", DalStationSettings.Instance.UseDatabaseForLiveMatches, null, delegate(object obj)
            {
                return LineSr.Instance.RemoveMatches(delegate(MatchLn match)
                    {
                        if (match.SourceType == linetype && match.IsLiveBet.Value)
                            return true;

                        //Debug.Assert(false);
                        return false;
                    });
            });
            LineSr.ProcessDataSqlUpdateSucceeded(eUpdateType.LiveBet, "Connection lost. SportRadar LiveMarket is cleared.");



        }

        private static string LinesToReceive
        {
            get { return (StationRepository.AllowLive ? "Live" : ""); }
        }


        private static void CleanerThread(ThreadContext tc)
        {
            while (!tc.IsToStop)
            {
                try
                {
                    ZipXml("Live");
                    ZipXml("Virtual");
                }
                catch (Exception e)
                {
                    m_logger.Excp(e, e.Message);
                }


                Thread.Sleep(60000);
            }
        }

        private static void ZipXml(string line)
        {
            if (!Directory.Exists(ConfigurationManager.AppSettings["betradar_xml_files"] + line + "\\"))
            {
                Directory.CreateDirectory(ConfigurationManager.AppSettings["betradar_xml_files"] + line + "\\");
            }
            var directories = Directory.GetDirectories(ConfigurationManager.AppSettings["betradar_xml_files"] + line);

            foreach (var path in directories)
            {
                var directoryName = path.Split('\\').Last();

                var time = new DateTime(Convert.ToInt32(directoryName.Split('_')[0]),
                    Convert.ToInt32(directoryName.Split('_')[1]), Convert.ToInt32(directoryName.Split('_')[2]),
                    Convert.ToInt32(directoryName.Split('_')[3]), 0, 0);
                var minTime = DateTime.Now.AddHours(-1);
                if (time < minTime)
                {
                    using (ZipFile zip = new ZipFile())
                    {
                        zip.UseUnicodeAsNecessary = true; // utf-8
                        var arhiveFolder = path.Split('\\').Last();
                        zip.AddDirectory(path, arhiveFolder);
                        zip.Comment = "This zip was created at " + System.DateTime.Now.ToString("G");
                        zip.Save(path + ".zip");
                    }

                    Directory.Delete(path, true);
                }
            }
            foreach (var file in Directory.GetFiles(ConfigurationManager.AppSettings["betradar_xml_files"] + line, "*.zip"))
            {
                var fileName = file.Split('\\').Last();

                var time = new DateTime(Convert.ToInt32(fileName.Split('_')[0]),
                    Convert.ToInt32(fileName.Split('_')[1]), Convert.ToInt32(fileName.Split('_')[2]),
                    Convert.ToInt32(fileName.Split('_')[3]), 0, 0);
                var minTime = DateTime.Now.AddDays(-1);
                if (time < minTime)
                {
                    File.Delete(file);
                }
            }

        }

        private static void ReaderThread(ThreadContext tc)
        {
            RemoveLiveMatches(eServerSourceType.BtrPre);
            RemoveLiveMatches(eServerSourceType.BtrLive);

            DateTime dtLastReceived = DateTime.Now;

            bool bDisabled = false;
            bool bRemoved = false;
            var lineReseiveCopy = "";

            while (m_bRunning && !tc.IsToStop)
            {
                m_dtConnected = DateTime.Now;

                try
                {
                    string sServerMessage = "";

                    while (m_bRunning && !tc.IsToStop && m_liveClient.IsAlive)
                    {
                        lineReseiveCopy = LinesToReceive;

                        //if (m_liveClient.HaveData)
                        sServerMessage = m_liveClient.ReadLine();
                        ExcpHelper.ThrowIf(string.IsNullOrEmpty(sServerMessage), "Received Empty Message");

                        m_logger.Debug("received live message " + sServerMessage.Length);
                        m_liveClient.WriteLine(lineReseiveCopy);


                        if (sServerMessage == LIVEBET_MSG_HELLO || sServerMessage == LIVEBET_MSG_EMPTY)
                        {
                            m_logger.DebugFormat("Received '{0}'", sServerMessage);
                            dtLastReceived = DateTime.Now;
                            LineSr.LiveBetConnected = true;
                        }
                        else
                        {
                            try
                            {
                                var originalLength = sServerMessage.Length;
                                if (sServerMessage.StartsWith(COMPRESSED))
                                {
                                    string sPrefix = sServerMessage.Substring(0, COMPRESSED_MASK_SIZE);

                                    string sSize = sPrefix.Substring(COMPRESSED.Length);

                                    int iCompressedMessageSize = 0;
                                    bool bSizeParsed = int.TryParse(sSize, out iCompressedMessageSize);
                                    Debug.Assert(bSizeParsed && iCompressedMessageSize > 0);
                                    //Debug.Assert(iCompressedMessageSize + COMPRESSED_MASK_SIZE == sServerMessage.Length);

                                    sServerMessage = TextUtil.DecompressBase64String(sServerMessage.Substring(COMPRESSED_MASK_SIZE));
                                }

                                int startIndex = sServerMessage.IndexOf('<');

                                ExcpHelper.ThrowIf(startIndex < 0, "Cannot find start xml point '<'");
                                sServerMessage = sServerMessage.Substring(startIndex);

                                TimeSpan tsDuration = DateTime.Now - m_dtConnected;

                                LineContainer lc = LineContainer.FromXml(sServerMessage);

                                ExcpHelper.ThrowIf<NullReferenceException>(lc == null, "LineContainer is Null");
                                //lc.Duration = tsDuration;
                                m_sqMessages.Enqueue(lc);

                                int iQueueCount = m_sqMessages.Count;

                                ExcpHelper.ThrowIf(iQueueCount > DalStationSettings.Instance.LiveErrorQueueSize, "Live Client Queue size ({0}) is too big.", iQueueCount);

                                if (iQueueCount > DalStationSettings.Instance.LiveWarnQueueSize)
                                {
                                    m_logger.Warn("Live Client Queue size = " + iQueueCount);
                                }

                                dtLastReceived = DateTime.Now;
                                bDisabled = false;
                                bRemoved = false;
                                string sType = lc.Attributes.ContainsKey("line") ? lc.Attributes["line"] : "none";

                                m_logger.DebugFormat("Enqueueing message {0} (Total messages in queue {1}; Connected {2}; Duration {3},type {4},length {5},compressedLength {6})", lc.GetDocId(), m_sqMessages.Count, m_dtConnected, tsDuration, sType, sServerMessage.Length, originalLength);
                            }
                            catch (Exception excp)
                            {
                                m_logger.Warn(ExcpHelper.FormatException(excp, "Could not process incoming server message (Size={0})", sServerMessage.Length));
                                throw;
                            }
                        }

                        LineSr.LiveBetConnected = true;
                        Thread.Sleep(10);

                    }
                }
                catch (Exception excp)
                {
                    m_logger.Error(excp.Message, excp);

                    m_sqMessages.Clear();
                    m_logger.Excp(excp, "Connection to server lost after importing time ({0}) ms bacause of {1}", DateTime.Now - m_dtConnected, excp.GetType());
                    LineSr.LiveBetConnected = false;
                }

                if (dtLastReceived + LIVE_TIME_OUT_TO_DISABLE < DateTime.Now && !bDisabled)
                {
                    LineSr.SyncRoutines(eUpdateType.LiveBet, "Connection lost. SportRadar LiveMarket is disabled.", DalStationSettings.Instance.UseDatabaseForLiveMatches, null, delegate(object obj)
                    {
                        LineSr.Instance.DisableLiveMatches(eServerSourceType.BtrLive);
                        return false;
                    });


                    LineSr.ProcessDataSqlUpdateSucceeded(eUpdateType.LiveBet, "Connection lost. SportRadar LiveMarket is cleared.");

                    bDisabled = true;
                }
                else if (dtLastReceived + LIVE_TIME_OUT_TO_REMOVE < DateTime.Now && !bRemoved)
                {
                    m_logger.ErrorFormat("Removing all live matches because last message ('{0}') were too long time ago (TimeOut='{1}')", new Exception(), dtLastReceived, LIVE_TIME_OUT_TO_REMOVE);
                    RemoveLiveMatches(eServerSourceType.BtrPre);
                    RemoveLiveMatches(eServerSourceType.BtrLive);
                    bRemoved = true;
                }

                Thread.Sleep(10);

                EnsureConnection();
            }
        }

        private static void WorkerThread(ThreadContext tc)
        {
            while (m_bRunning) //(!m_bImportError)
            {
                try
                {
                    int counter = 0;
                    int iLiveQueueCount = m_sqMessages.Count;

                    if (!m_liveClient.IsAlive)
                    {
                        m_sqMessages.Clear();
                    }
                    else if (iLiveQueueCount > 0)
                    {
                        while (m_sqMessages.Count > 0)
                        {
                            counter++;

                            LineContainer lc = m_sqMessages.Dequeue();

                            eFileSyncResult fsr = eFileSyncResult.Failed;
                            Debug.Assert(lc != null);

                            try
                            {
                                string sSrcTime = lc.Attributes["srctime"];
                                DateTimeSr dtSrcTime = DateTimeSr.FromString(sSrcTime);
                                string sSrcDelay = (DateTime.UtcNow - dtSrcTime.UtcDateTime).ToString();

                                string sTime = lc.Attributes["time"];
                                DateTimeSr dt = DateTimeSr.FromString(sTime);
                                TimeSpan ts = DateTime.UtcNow - dt.UtcDateTime;

                                string sType = lc.Attributes.ContainsKey("line") ? lc.Attributes["line"] : "none";

                                m_logger.InfoFormat("Queue={0} Delay={1} Size={2}", iLiveQueueCount, ts, lc.OriginalXml.Length);

                                if (ConfigurationManager.AppSettings["betradar_xml_files"] != null)
                                    SaveXml(lc, "Live");


                                fsr = LineSr.SyncRoutines(eUpdateType.LiveBet, string.Format("Host='{0}' Connected={1}; SrcDelay={2}; Delay={3}; Line={4}", m_sHost, DateTime.Now - m_dtConnected, sSrcDelay, ts, sType), DalStationSettings.Instance.UseDatabaseForLiveMatches, null, delegate(object objParam)
                                    { return ProviderHelperNew.MergeFromLineContainer(lc); });



                            }
                            catch (Exception excp)
                            {
                                fsr = eFileSyncResult.Failed;
                                m_logger.Excp(excp, "WorkerThread() ERROR for {0}", lc);
                            }


                            if (fsr == eFileSyncResult.Failed)
                            {
                                m_liveClient.Disconnect();
                                RemoveLiveMatches(eServerSourceType.BtrPre);
                                RemoveLiveMatches(eServerSourceType.BtrLive);
                            }
                        }
                        LineSr.ProcessDataSqlUpdateSucceeded(eUpdateType.LiveBet, string.Format("liveUpdate {0} messages", counter));

                    }
                }
                catch (Exception excp)
                {
                    m_logger.Excp(excp, "WorkerThread() general ERROR");
                }

                Thread.Sleep(10);
            }
        }

        private static Thread savingThread = null;
        private static SyncQueue<LineMessage> savingQueue = new SyncQueue<LineMessage>();

        public static void SaveXml(LineContainer slc, string sline)
        {
            savingQueue.Enqueue(new LineMessage(slc, sline));
            if (savingThread == null)
            {
                savingThread = new Thread(() =>
                {
                    while (true)
                    {
                        if (savingQueue.Count == 0)
                        {
                            Thread.Sleep(10);
                            continue;
                        }


                        var linemsg = savingQueue.Dequeue();
                        var time = DateTime.Now;
                        var folderPath = ConfigurationManager.AppSettings["betradar_xml_files"] + linemsg.Line + "\\" +
                                         time.Year +
                                         "_" +
                                         time.Month + "_" + time.Day + "_" + time.Hour + "_00" + "\\";
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        var timestamp = time.ToFileTime().ToString();

                        var filename = folderPath + linemsg.LineContainer.Attributes["type"] + timestamp;
                        int index = 1;
                        var tempfilename = filename;
                        while (!SaveFile(linemsg.LineContainer, tempfilename))
                        {
                            tempfilename = filename + " " + index++;
                        }
                    }

                });
                savingThread.Start();

            }

        }

        private static bool SaveFile(LineContainer lc, string filename)
        {
            int counter = 0;
            var newFileName = filename;
            while (true)
            {
                try
                {
                    if (File.Exists(newFileName))
                        return true;
                    StreamWriter writer = new StreamWriter(newFileName + ".xml");
                    writer.Write(lc.OriginalXml);
                    writer.Flush();
                    writer.Close();
                    return true;
                }
                catch (Exception)
                {
                    newFileName = filename + counter++;
                }
            }

        }


        private static void EnsureConnection()
        {
            m_liveClient.Disconnect();
            m_logger.WarnFormat("(Re)Connecting to {0}", m_liveClient.HostName);

            m_liveClient.Connect();

            if (m_liveClient.IsAlive)
            {
                try
                {
                    m_liveClient.WriteLine(StationNumber);
                    m_logger.InfoFormat("Connected to {0}", m_liveClient.HostName);
                }
                catch
                {
                }
            }

            if (m_liveClient.IsAlive)
            {
                m_dtConnected = DateTime.Now;
            }
        }

        public static string StationNumber { get; set; }
    }

    internal class LineMessage
    {
        public LineContainer LineContainer { get; set; }
        public string Line { get; set; }
        public LineMessage(LineContainer slc, string sline)
        {
            LineContainer = slc;
            Line = sline;
        }
    }
}
