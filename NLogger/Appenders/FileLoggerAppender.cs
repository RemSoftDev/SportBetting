using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace NLogger.Appenders
{
    /// <summary>
    /// File Logger Appender
    /// </summary>
    public class FileLoggerAppender : ILogAppender
    {
        #region Fields

        private SyncQueue<LogItem> _queue;

        private readonly Dictionary<string, Func<LogItem, string>> _formatting = new Dictionary
    <string, Func<LogItem, string>>()
            {
                {"%exception", x => x.Exception != null ? x.Exception.Message.Replace(Environment.NewLine, "") : ""},
                {
                    "%stacktrace",
                    x =>
                    x.Exception != null && x.Exception.StackTrace != null
                        ? x.Exception.StackTrace.Replace(Environment.NewLine, "")
                        : ""
                }
            };

        private readonly Dictionary<string, long> _conversion = new Dictionary<string, long>
            {
                {"KB", 1024},
                {"MB", 1024*1024},
                {"GB", 1024*1024*1024}
            };

        private bool _disposing;

        private Thread _loggerThread;

        private DateTime _lastWrite;

        #endregion


        #region Constants

        private const string DefaultLogPattern = "%level %date %message | %exception %stacktrace";
        const int MAX_QUEUE_RECORD_LIMIT = 100;

        #endregion

        volatile bool QueueIsOverloadedWarning = false;

        #region Properties

        public string Name { get; set; }

        public List<LoggingLevel> LoggingLevels { get; set; }

        public long Queued { get { return _queue.Count; } }

        public string LogPattern { get; set; }

        public string Parameters { get; set; }

        public TimeSpan TimeSinceLastWrite { get; set; }

        public int MaxQueueCache { get; set; }

        public int TimeBetweenChecks { get; set; }

        public long MaxFileSize { get; set; }

        public string Location { get; set; }

        public int MaxLogCount { get; set; }

        #endregion


        #region Events

        /// <summary>
        /// Fired when log is written
        /// </summary>
        public event Logger.LogWritten OnLogWritten;

        #endregion


        #region Constructors and destructors

        /// <summary>
        /// Initializes a new FileLoggerAppender
        /// </summary>
        public FileLoggerAppender()
        {
            LoggingLevels = new List<LoggingLevel>();
            MaxLogCount = -1;
            TimeSinceLastWrite = new TimeSpan(0, 0, 30);
            TimeBetweenChecks = 30;
            MaxQueueCache = 100;
            _queue = new SyncQueue<LogItem>();
            OnLogWritten += DefaultLogWriter;
            BeginLogWriter();
        }

        #endregion


        #region ILogAppender method implementations

        public void Log(string message, Exception exception, LoggingLevel level)
        {

            if (_queue.Count > MAX_QUEUE_RECORD_LIMIT)
            {
                if (!QueueIsOverloadedWarning)
                {
                    QueueIsOverloadedWarning = true;
                    _queue.Enqueue(new LogItem(String.Format("MAX QUEUE COUNT WAS EXCEEDED (CURRENT: {0})! ADDING NEW MSG INTO QUEUE IS STOPPED!\r\n", _queue.Count), null, LoggingLevel.Fatal));

                }
                return;
            }
            if (QueueIsOverloadedWarning)
            {
                QueueIsOverloadedWarning = false;
                _queue.Enqueue(new LogItem("MAX QUEUE COUNT IS NORMALIZED! CONTINUE TO ADD NEW MSG INTO QUEUE!\r\n", null, LoggingLevel.Fatal));

            }
            _queue.Enqueue(new LogItem(message, exception, level));
        }

        #endregion


        #region IDisposable implementation

        public void Dispose()
        {
            _disposing = true;
            FinalizeDispose();
        }

        #endregion


        #region Private methods

        #region Alternate implementation

        private void BeginLogWriter()
        {
            _loggerThread = new Thread(OnThreadStart);
            _loggerThread.Start();
            _lastWrite = DateTime.Now;
        }

        private void OnThreadStart()
        {
            do
            {
                Thread.Sleep(TimeBetweenChecks);
                if (_queue.Count == 0 || (_queue.Count < MaxQueueCache && (DateTime.Now - _lastWrite) < TimeSinceLastWrite))
                {
                    Thread.Sleep(50);
                    continue;
                }
                if (OnLogWritten == null) continue;
                var logItems = new List<LogItem>();
                for (var i = 0; i < _queue.Count; i++)
                {
                    LogItem item = _queue.Dequeue();
                    logItems.Add(item);
                }
                OnLogWritten(logItems);
                _lastWrite = DateTime.Now;
            } while (!_disposing);
            _loggerThread.Abort();
            _loggerThread = null;
            FinalizeDispose();
        }

        #endregion

        private void DefaultLogWriter(IList<LogItem> logItems)
        {
            try
            {
                //EventLogWriter.Log("Entering try block", EventLogEntryType.Information, 103);
                if (File.Exists(Location))
                {
                    var info = new FileInfo(Location);
                    //EventLogWriter.Log("File exists, file size is:" + info.Length, EventLogEntryType.Information, 104);
                    if (info.Length >= MaxFileSize)
                    {
                        //EventLogWriter.Log("File size is greater than max", EventLogEntryType.Information, 105);
                        //File.SetLastWriteTime(Location, DateTime.Now);
                        //var move = DateTime.Now;
                        ////EventLogWriter.Log("Attempting to move to: " + Location, EventLogEntryType.Information, 106);
                        //var loc = Location + "." + move.ToString("yyyy-dd-MM_HH-mm-ss_fffffff");
                        //info.MoveTo(loc);


                        var loc = Location + ".";
                        if (File.Exists(loc + "1"))
                            File.Delete(loc + "1");
                        info.MoveTo(loc + "1");

                        if (System.IO.File.Exists(loc + "1"))
                        {
                            int zipCount = 1;
                            while (System.IO.File.Exists(loc + zipCount + ".zip"))
                            {
                                zipCount++;
                            }

                            if (zipCount > MaxLogCount)
                            {
                                System.IO.File.Delete(loc + MaxLogCount + ".zip");
                                zipCount--;
                            }

                            if (zipCount > 1)
                            {
                                for (int i = zipCount; i > 1; i--)
                                {
                                    if (System.IO.File.Exists(loc + (i - 1) + ".zip") && !System.IO.File.Exists(loc + (i) + ".zip"))
                                        System.IO.File.Move(loc + (i - 1) + ".zip", loc + (i) + ".zip");
                                }
                            }

                                    string fileName = Location.Substring(Location.LastIndexOf('\\') + 1);
                                    CreateZip.CreateZipFile(Location + ".1", fileName);
                                    System.IO.File.Delete(Location + ".1");
                        }



                    }
                }
            }
            catch (Exception e)
            {
                EventLogWriter.Log(string.Format("Exception occurred in FileLoggerAppender -> DefaultLogWriter, {2}Message: {2}{0}{2}StackTrace: {2}{1}{2}Source: {2}{3}", e.Message, e.StackTrace, Environment.NewLine, e.Source), EventLogEntryType.Error, 4);
            }




            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(Location)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Location));
                }
                using (var fs = new FileStream(Location, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 1024 * 1024, FileOptions.WriteThrough))
                {
                    using (var fw = new StreamWriter(fs, new UTF8Encoding(), 1024 * 1024, true))
                        // ReSharper disable ForCanBeConvertedToForeach Reason: Optimization
                        for (var i = 0; i < logItems.Count; i++)
                        // ReSharper restore ForCanBeConvertedToForeach
                        {
                            var toWrite = string.Format("{0}", Logger.FormatLog(DefaultLogPattern, logItems[i], _formatting));
                            fw.WriteLine(toWrite);
                        }
                    fs.Flush(true);
                }
            }
            catch (IOException e)
            {
                if (!IsFileLocked(e))
                {
                    EventLogWriter.Log(string.Format("IOException occurred in FileLoggerAppender -> DefaultLogWriter, {2}Message: {2}{0}{2}StackTrace: {2}{1}{2}Source: {2}{3}", e.Message, e.StackTrace, Environment.NewLine, e.Source), EventLogEntryType.Error, 3);
                    return;
                }
                Thread.Sleep(2000);
                DefaultLogWriter(logItems);
            }
            catch (ArgumentNullException e)
            {
                EventLogWriter.Log(string.Format("ArgumentNullException occurred in FileLoggerAppender -> DefaultLogWriter, {2}Message: {2}{0}{2}StackTrace: {2}{1}{2}Source: {2}{3}", e.Message, e.StackTrace, Environment.NewLine, e.Source), EventLogEntryType.Error, 1);
                // Sometimes a 'file not found' exception is thrown, not sure why
            }
            catch (Exception e)
            {
                EventLogWriter.Log(string.Format("Exception occurred in FileLoggerAppender -> DefaultLogWriter, {2}Message: {2}{0}{2}StackTrace: {2}{1}{2}Source: {2}{3}", e.Message, e.StackTrace, Environment.NewLine, e.Source), EventLogEntryType.Error, 2);
            }
        }

        private void FinalizeDispose()
        {
            _queue = null;
        }

        private static bool IsFileLocked(Exception exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
            return errorCode == 32 || errorCode == 33;
        }

        #endregion

    }
}
