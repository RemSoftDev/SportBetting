using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using SportRadar.Common.Logs;

namespace SportBetting.WPF.Prism.Shared
{
    public class CreateZip
    {
        //private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(CreateZip).Name);
        private static ILog Log = LogFactory.CreateLog(typeof(CreateZip));

        public static bool CreateZipFile(string[] FileList, string[] newFileNameList, string ZipName, int ZipLevel)
        {
            try
            {
                // 'using' statements gaurantee the stream is closed properly which is a big source
                // of problems otherwise.  Its exception safe as well which is great.
                if (File.Exists(ZipName))
                {
                    File.Delete(ZipName);
                }
                string directoryName = ZipName.Substring(0, ZipName.LastIndexOf('\\'));
                if (!Directory.Exists(directoryName))
                {
                    Log.Debug("Directory " + directoryName + " not found");
                    //Directory.CreateDirectory(directoryName);
                }

                using (ZipOutputStream s = new ZipOutputStream(File.Create(ZipName)))
                {
                    s.SetLevel(ZipLevel); // 0 - store only to 9 - means best compression

                    for (int i = 0; i < FileList.Length; i++)
                    {
                        if (FileList[i] == null || !File.Exists(FileList[i]))
                        {
                            continue;
                        }
                        using (FileStream fs = new FileStream(FileList[i], FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            if (fs.Length <= 0)
                            {
                                fs.Close();
                                continue;
                            }

                            // im Normalfall allokiert man die Buffer im voraus
                            // hier aus Klarheitsgründen pro Datei einmal
                            byte[] buffer = new byte[fs.Length];
                            fs.Read(buffer, 0, buffer.Length);
                            fs.Close();
                            // und jetzt schreiben wir einen ZipEntry & die Daten      
                            // ZipEntry entry = new ZipEntry(FileList[i].Substring(FileList[i].LastIndexOf("\\") + 1));

                            if (String.IsNullOrEmpty(newFileNameList[i]))
                            {
                                newFileNameList[i] = FileList[i].Substring(FileList[i].LastIndexOf('\\') + 1);
                            }

                            ZipEntry entry = new ZipEntry(newFileNameList[i]);//ZipEntry entry = new ZipEntry(FileList[i]);

                            s.PutNextEntry(entry);
                            s.Write(buffer, 0, buffer.Length);
                        }
                    }
                    s.Finish();
                    s.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Exception during zipping files:\n", ex);
                return false;
            }
        }

        public static string[] UnzipFile(string ZipFileName, string targetDirectory, string SearchFilter)
        {
            string[] FileList = null;

            if (!File.Exists(ZipFileName))
            {
                Log.Warn("Cannot find file: " + ZipFileName);
                return null;
            }

            try
            {
                FastZip fz = new FastZip();

                if (targetDirectory.Length == 0)
                {
                    Log.Warn("Error creating targetDirectory from ZipFileName: " + ZipFileName);
                    targetDirectory = "C:\\Temp\\NBTUnzip";
                }
                fz.ExtractZip(ZipFileName, targetDirectory, "");
                FileList = Directory.GetFiles(targetDirectory, "*" + SearchFilter, SearchOption.AllDirectories);
            }
            catch (Exception Ex)
            {
                Log.Error("Error unzip file: " + ZipFileName, Ex);
                return null;
            }
            return FileList;
        }
    }
}
    


