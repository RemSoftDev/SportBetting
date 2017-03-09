using System;
using System.Collections.Generic;
using IocContainer;
using Ninject;
using SharedInterfaces;
using System.Configuration;

namespace SportBetting.WPF.Prism.Shared
{
    /*public class CustomAppender : RollingFileAppender
    {
        public IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }

        string GrayOrRed()
        {

            string grayOrRed = "";
#if BETCENTER 
            grayOrRed = "RED";
#else
            grayOrRed = "GRAY";
#endif
            return grayOrRed;
        }


        protected override void CloseWriter()
        {
            if (Prism.Models.Repositories.StationRepository.RollingFileSize > 0)
            {
                MaximumFileSize = Prism.Models.Repositories.StationRepository.RollingFileSize + "MB";
                if (ConfigurationManager.AppSettings["logFileMaxSize"] != MaximumFileSize)
                {
                    ConfigurationManager.AppSettings.Set("logFileMaxSize", MaximumFileSize);
                }
            }

            else
                MaximumFileSize = "10MB";

            if (Prism.Models.Repositories.StationRepository.RollingFileCount > 0)
                MaxSizeRollBackups = Prism.Models.Repositories.StationRepository.RollingFileCount;
            else
                MaxSizeRollBackups = 4;


            base.CloseWriter();


            int count = 1;
            string logName = this.File + ".";

            if (System.IO.File.Exists(logName + count))
            {
                int zipCount = 1;
                while (System.IO.File.Exists(logName + zipCount + ".zip"))
                {
                    zipCount++;
                }

                if (zipCount == MaxSizeRollBackups + 1)
                {
                    System.IO.File.Delete(logName + MaxSizeRollBackups + ".zip");
                    zipCount--;
                }

                if (zipCount > 1)
                {
                    for (int i = zipCount; i > 1; i--)
                    {
                        if (System.IO.File.Exists(logName + (i - 1) + ".zip")
                            && !System.IO.File.Exists(logName + (i) + ".zip"))
                            System.IO.File.Move(logName + (i - 1) + ".zip", logName + (i) + ".zip");
                    }
                }


                string[] newName = { logName.Substring(logName.LastIndexOf('\\') + 1) };
                newName[0] = newName[0].Substring(0, newName[0].Length - 1);

                string[] fileToZIP = { logName + count };
                CreateZip.CreateZipFile(fileToZIP, newName, logName.Substring(0, logName.Length - 1) + ".1.zip",
                                        5);
                System.IO.File.Delete(logName + count);

            }
        }

        private void ArchiveLogsAndConfigs()
        {
            string toZip = @".\log\archivedlogs";


            string[] logFiles = 
            {
                @".\log\SportBetting.log",
                @".\log\SportBettingErrors.log",
            };

            //add files what we need to zip
            List<string> files = new List<string>();
            for (int i = 0; i < logFiles.Length; i++)
            {
                for (int k = 1; ; k++)
                {
                    if (System.IO.File.Exists(logFiles[i] + "." + k + ".zip"))
                    {
                        files.Add(logFiles[i] + "." + k + ".zip");
                    }
                    else break;
                }
            }

            files.Add(@".\Config\PrefFile.txt");
            files.Add(@".\SportBetting.WPF.Prism.exe.config");

            files.Add(logFiles[0]);
            files.Add(logFiles[1]);



            string zipPath = @".\log\" +
                            DateTime.Now.ToString("yyMMddhhmmss") + "_" + GrayOrRed() + "_" +
                            StationRepository.StationNumber
                            + ".zip";

            CreateZip.CreateZipFile(files.ToArray(), new string[files.Count], zipPath, 5);

            string file = FileToBase64(zipPath);
            string fileName = zipPath.Substring(zipPath.LastIndexOf('\\') + 1);

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

    }*/
}
