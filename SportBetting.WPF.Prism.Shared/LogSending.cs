using System;
using System.Collections.Generic;
using System.IO;
using IocContainer;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Shared.Models.Repositories;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common.Logs;
using System.Management;
using System.Threading;
using WsdlRepository.WsdlServiceReference;

namespace SportBetting.WPF.Prism.Shared
{
    public static class LogSending
    {
        private static ILog Log = LogFactory.CreateLog(typeof(LogSending));

        //public static List<StationAppConfigSr> stationConfig = StationAppConfigSr.ListStationAppConfigByQuery();
        private static string _zipPath;
        private static Thread sendingThread = new Thread(onSendLogsWoPleaseWait);

        static string GrayOrRed()
        {

            string grayOrRed = "";
#if BETCENTER
            grayOrRed = "RED";
#else
            grayOrRed = "GRAY";
#endif
            return grayOrRed;
        }

        public static string stationNumber;

        public static Byte[] FileArray { get; private set; }


        public static string FileName { get; private set; }

        public static long maxFileSize { get; set; }

        public static bool sendingThreadIsAlive
        {
            get
            {
                return sendingThread.IsAlive;

            }
        }

        //[AsyncMethod]
        //public static void SendLogs()
        //{
        //    onSendLogs();
        //}
        //[PleaseWaitAspect]
        public static void SendLogs()
        {
            if (!LogSending.sendingThreadIsAlive)
            {
                sendingThread = new Thread(onSendLogsWoPleaseWait);
                sendingThread.Priority = ThreadPriority.BelowNormal;
                sendingThread.IsBackground = true;
                sendingThread.Start();
                //onSendLogsWoPleaseWait();
            }
        }

        public static void onSendLogsWoPleaseWait()
        {
            string[] msg = new string[2];
            try
            {

                ZippedLogsToByteArray();
                msg[0] = FileName;
                BSMHubServiceClient x = new BSMHubServiceClient();
                x.InnerChannel.OperationTimeout = new TimeSpan(1, 0, 0);
                Log.Debug("export file size = " + FileArray.Length);
                x.ExportLogData(FileName, FileArray, stationNumber);

                /*
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://tll1-betsw-dev4:8080/");
                request.Method = "POST";

                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = FileArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(FileArray, 0, FileArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = HttpUtility.UrlDecode(reader.ReadToEnd());
                //You may need HttpUtility.HtmlDecode depending on the response

                reader.Close();
                dataStream.Close();
                response.Close();

                */



            }
            catch (Exception ex)
            {
                Log.Debug(ex.ToString());
                msg[1] = "0";
            }
            finally
            {
                IMediator myMediator = IoCContainer.Kernel.Get<IMediator>();
                myMediator.SendMessage(msg, MsgTag.ZippedLogsUploaded);
            }
        }

        //[AsyncMethod]
        //private static void ArchiveLogsAndConfigsAndGetZipName()
        //{
        //    onArchiveLogsAndConfigsAndGetZipName();
        //}

        //[PleaseWaitAspect]
        private static void ArchiveLogsAndConfigsAndGetZipName()
        {
            try
            {
                FileInfo fi = new FileInfo(@".\log\SportBetting.log");
                long maxFileSizeInBytes = (maxFileSize + 1) * 1024 * 1024;


                if (fi.Length > maxFileSizeInBytes)
                {
                    // Read the file and display it line by line.
                    using (StreamReader file = new StreamReader(@".\log\SportBetting.log"))
                    {
                        //File.Create(@"c:\log\SportBetting-CUT.log");
                        using (StreamWriter writer = new StreamWriter(@".\log\SportBetting-CUT.log"))
                        {
                            string line;
                            while ((line = file.ReadLine()) != null)
                            {
                                writer.WriteLine(line);
                                if (new FileInfo(@".\log\SportBetting-CUT.log").Length >= maxFileSizeInBytes)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }


            string[] logFiles = 
            {
                @".\log\SportBetting.log",
                @".\log\SportBettingErrors.log"
            };

            //add files what we need to zip
            List<string> files = new List<string>();
            for (int i = 0; i < logFiles.Length; i++)
            {
                for (int k = 1; k <= StationRepository.RollingFileCount; k++)
                {
                    if (File.Exists(logFiles[i] + "." + k + ".zip"))
                    {
                        files.Add(logFiles[i] + "." + k + ".zip");
                    }
                    else break;
                }
            }

            if (File.Exists(@".\log\SportBetting-CUT.log"))
            {
                files.Add(@".\log\SportBetting-CUT.log");
            }
            else
            {
                files.Add(logFiles[0]);
            }

            files.Add(@".\Config\PrefFile.txt");
            files.Add(@".\SportBetting.WPF.Prism.exe.config");
            files.Add(@".\log\SystemInfo.log");

            files.Add(logFiles[1]);


            if (string.IsNullOrEmpty(stationNumber))
            {
                stationNumber = GetStationIP();
            }

            //_zipPath = toZip +
            _zipPath = @".\log\" +
                   DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + GrayOrRed() + "_" +
                   stationNumber
                        + ".zip";

            CreateZip.CreateZipFile(files.ToArray(), new string[files.Count], _zipPath, 5);
        }

        private static string GetStationIP()
        {
            string IpAddress = "";
            string query = "SELECT * FROM Win32_NetworkAdapterConfiguration"
                     + " WHERE IPEnabled = 'TRUE'";
            ManagementObjectSearcher moSearch = new ManagementObjectSearcher(query);
            ManagementObjectCollection moCollection = moSearch.Get();

            // Every record in this collection is a network interface
            foreach (ManagementObject mo in moCollection)
            {

                IpAddress = ((string[])mo["IPAddress"])[0];
                break;
            }
            return IpAddress;
        }

        public static void ZippedLogsToByteArray()
        {
            ArchiveLogsAndConfigsAndGetZipName();
            if (System.IO.File.Exists(_zipPath))
            {
                FileArray = FileToByteArray(_zipPath);
                FileName = _zipPath.Substring(_zipPath.LastIndexOf('\\') + 1);
            }
        }

        public static byte[] FileToByteArray(string filename)
        {
            byte[] returnByteArray = null;
            // Instantiate FileStream to read file
            System.IO.FileStream readFileStream = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            // Instantiate BinaryReader and attach FileStream with the object
            System.IO.BinaryReader readBinaryReader = new System.IO.BinaryReader(readFileStream);

            // Get file's byte length
            long fileByteSize = new System.IO.FileInfo(filename).Length;

            // Read bytes from the file
            returnByteArray = readBinaryReader.ReadBytes((Int32)fileByteSize);

            // Clean up / disposal
            readFileStream.Close();
            readFileStream.Dispose();
            readBinaryReader.Close();
            return returnByteArray;
        }
        public static string FileToBase64(string filename)
        {
            return Convert.ToBase64String(FileToByteArray(filename));
        }

        public static void Base64ToFile(string base64String, string filename)
        {
            byte[] fileByteArray = Convert.FromBase64String(base64String);
            // Instantiate FileStream to create a new file
            System.IO.FileStream writeFileStream = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            // Write converted base64String to newly created file
            writeFileStream.Write(fileByteArray, 0, fileByteArray.Length);
            // Clean up / disposal
            writeFileStream.Close();
        }

    }
}
