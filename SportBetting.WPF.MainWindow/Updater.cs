using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Configuration;
using System.Threading;
using DefaultViews.Views;
using Ionic.Zip;
using MainWpfWindow.Views;
using SportBetting.WPF.Prism.Views;
using SportRadar.Common.Logs;

namespace SportBetting.WPF.Prism
{
    public class Updater
    {

        private static ILog Log = LogFactory.CreateLog(typeof(Updater));

        static AutoResetEvent _autoEvent = new AutoResetEvent(false);
        static string _sUpdatePath = Directory.GetParent(Environment.CurrentDirectory) + "\\update";
        static string _sRemoteVersion;
        StartWindow _winStarter;
        private string _sHUBAddress = Convert.ToString(ConfigurationManager.AppSettings["UpdaterHUBAddress"]); // SportBetting.WPF.Prism.Properties.Settings.Default.HUBAddress;  
        private string _client = Convert.ToString(ConfigurationManager.AppSettings["client"]); // SportBetting.WPF.Prism.Properties.Settings.Default.HUBAddress;  

        //private static ILog _logger = LogManager.GetLogger("Updater");

        public bool IsError { get; set; }

        public int DoBinariesUpdate(out string sRemoteVersion, ref AutoResetEvent autoEvent, ref StartWindow winStarter)
        {
            // return values
            // 0 - no update needed
            // 1 - update done, do restart

            _autoEvent = autoEvent;
            _winStarter = winStarter;
            _sRemoteVersion = string.Empty;
            sRemoteVersion = string.Empty;
            string sRemoteURL = string.Empty;
            string[] arrVersions;

            _winStarter.SetMessage("Connecting to Version service");
            Log.Info("Connecting to Version service");

            X509Certificate2 Cert = new X509Certificate2();
            bool bIsSSL = _sHUBAddress.Contains("https");

            try
            {
                if (bIsSSL)
                {

                    BehaviorsSection clientSection = ConfigurationManager.GetSection("system.serviceModel/behaviors") as BehaviorsSection;
                    ServiceModelEnhancedConfigurationElementCollection<EndpointBehaviorElement> colX = clientSection.EndpointBehaviors;
                    EndpointBehaviorElement ebe = colX[0];
                    ClientCredentialsElement clientCred = (ClientCredentialsElement)ebe.First(x => x.GetType() == typeof(ClientCredentialsElement));

                    string sFindName = clientCred.ClientCertificate.FindValue;
                    string sStoreName = clientCred.ClientCertificate.StoreName.ToString();

                    X509Store store = new X509Store(sStoreName);
                    store.Open(OpenFlags.ReadOnly);
                    X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindBySubjectName, sFindName, false);
                    Cert = certs[0];
                }
            }
            catch (Exception ex)
            {
                Log.Error("Certificate file not found",ex);
                return 0;
            }


            try
            {
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(_sHUBAddress + "/BSMHub/release?version=1&app_type=terminal&client=" + _client);
                httpRequest.Method = "GET";
                httpRequest.KeepAlive = false;

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                if (bIsSSL) httpRequest.ClientCertificates.Add(Cert);
                HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                Stream responseStream = httpResponse.GetResponseStream();
                StreamReader sr = new StreamReader(responseStream);

                string sVersion = sr.ReadToEnd().ToString();

                responseStream.Close();
                httpResponse.Close();
                httpRequest = null;

                //string sVersion = "111";
                arrVersions = sVersion.Split((char)'\n');
                _winStarter.SetMessage("Downloading updated version");
                Log.Info("Got version information from " + _sHUBAddress + "/BSMHub/release?version=1&app_type=terminal&client=" + _client);
            }
            catch (Exception ex)
            {
                // log "no version file"
                Log.Error("Error getting remote version from " + _sHUBAddress + "/BSMHub/release?version=1&app_type=terminal&client=" + _client,ex);
                _winStarter.SetMessage("Error getting remote version");
                return 0;
            }

            if (arrVersions[0].Length > 1)
            {
                _sRemoteVersion = arrVersions[0].Trim();
                sRemoteVersion = _sRemoteVersion;
            }

            // compare versions
            string sLocalVersion = string.Empty;
            sRemoteURL = _sHUBAddress + "/BSMHub/release?id=SportBetting&app_type=terminal&client=" + _client;

            try
            {
                string sLocalVersionFile = Environment.CurrentDirectory + "/version.txt";
                if (File.Exists(sLocalVersionFile))
                {
                    string[] sVersionInfoContent = File.ReadAllLines(sLocalVersionFile);
                    if (sVersionInfoContent.Length > 0 && sVersionInfoContent[0] != null)
                    {
                        sLocalVersion = sVersionInfoContent[0].Trim();
                    }
                    Log.Info("Local version: " + sLocalVersion);
                }
                else
                {
                    sLocalVersion = "0";
                    Log.Info("Local version not found");
                }
            }
            catch (Exception e)
            {
                Log.Error("Error opening version.txt file",e);
            }


            // compare versions. there might be upgrade and downgrade, so let's check for "not equal only"
            if (_sRemoteVersion != sLocalVersion)
            {
                try
                {
                    _winStarter.SetMessage("Downloading new file version: " + _sRemoteVersion);

                    // clean up update directory
                    if (Directory.Exists(_sUpdatePath))
                    {
                        Directory.Delete(_sUpdatePath, true);
                        Thread.Sleep(500);
                    }
                    Directory.CreateDirectory(_sUpdatePath);
                    Thread.Sleep(500);

                    string sDownloadLocalFile = _sUpdatePath + "\\download" + _sRemoteVersion + ".zip";

                    HttpWebRequest httpRequestZip = (HttpWebRequest)WebRequest.Create(sRemoteURL);

                    if (bIsSSL) httpRequestZip.ClientCertificates.Add(Cert);
                    HttpWebResponse httpResponseZip = (HttpWebResponse)httpRequestZip.GetResponse();

                    int uFileSize;
                    int.TryParse(httpResponseZip.Headers.Get("Content-Length"), out uFileSize);
                    Log.Info("download file size: " + uFileSize);

                    byte[] buffer = new byte[32768];
                    using (Stream input = httpResponseZip.GetResponseStream())
                    {
                        using (FileStream output = new FileStream(sDownloadLocalFile, FileMode.CreateNew))
                        {
                            long dlBytes = 0;
                            int bytesRead = 0;
                            int dlStep = 1;
                            int dlPercentNext = 0;
                            double dlPercent = 0;

                            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                output.Write(buffer, 0, bytesRead);
                                dlBytes += bytesRead;
                                dlPercent = dlBytes * 100 / uFileSize;
                                if (dlPercentNext == dlPercent)
                                {
                                    dlPercentNext += dlStep;
                                    _winStarter.SetMessageControlledSleep("Download progress: " + dlPercent + "% (" + dlBytes + "/" + uFileSize + ")", 25);
                                }
                            }
                        }
                    }

                    httpResponseZip.Close();

                    Log.Info("Got remote ZIP file with new version");
                    ProcessDownloadedFile();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ReleaseThread), _autoEvent);
                }
                catch (Exception ex)
                {
                    // log "no version file"
                    Log.Error("Error getting remote version from " + _sHUBAddress + "/BSMHub/release?versions",ex);
                    _winStarter.SetMessage("Error getting remote version");
                    return 0;
                }
            }
            else
            {
                // no update needed
                _winStarter.SetMessage("No update needed");
                return 0;
            }

            return 1;
        }

        private void ProcessDownloadedFile()
        {
            _winStarter.SetMessage("Update downloaded. Extracting files...");


            try
            {
                var currentDir = Directory.GetCurrentDirectory();
                // remove pre-version
                if (Directory.Exists(currentDir + "_old"))
                {
                    Directory.Delete(currentDir + "_old", true);
                }

                CopyDirectory(currentDir, currentDir + "_old");

                using (ZipFile zip = ZipFile.Read(_sUpdatePath + "\\download" + _sRemoteVersion + ".zip"))
                {
                    foreach (ZipEntry en in zip)
                    {
                        en.Extract(_sUpdatePath, ExtractExistingFileAction.OverwriteSilently);  // true => overwrite existing files
                    }
                }
                Log.Info("Unzipped new binaries");
            }
            catch (Exception e)
            {
                IsError = true;
                Log.Error("Error during backup or upzip",e);
                ThreadPool.QueueUserWorkItem(new WaitCallback(ReleaseThread), _autoEvent);
            }


            // check if we have .zip extracted
            if (Directory.GetDirectories(_sUpdatePath).Count() == 0)
            {
                IsError = true;
                Log.Error("Corrupted zip file",new Exception());
                ThreadPool.QueueUserWorkItem(new WaitCallback(ReleaseThread), _autoEvent);
            }

            _winStarter.SetMessage("Files extracted successfully. Pending Application restart");
            Log.Info("Files extracted successfully. Pending Application restart");

        }

        static void ReleaseThread(object stateInfo)
        {
            ((AutoResetEvent)stateInfo).Set();
        }

        /// <summary>
        /// copies content of current direcory to new directory
        /// </summary>
        /// <param name="oldDir">String containing name of old directory</param> 
        /// <param name="newDir">String containing name of new directory</param> 

        private static void CopyDirectory(string oldDir, string newDir)
        {

            if (!Directory.Exists(newDir))
            {
                Directory.CreateDirectory(newDir);
            }

            string[] files = Directory.GetFiles(oldDir);

            foreach (string file in files)
            {
                string destFile = Path.Combine(newDir, Path.GetFileName(file));
                //if (!file.Contains("vshost"))
                //{
                //    File.Copy(file, destFile, true);
                //}
                //else
                //{
                //    System.Diagnostics.Debug.WriteLine("A");
                //}
                File.Copy(file, destFile, true);
            }
            string[] dirs = Directory.GetDirectories(oldDir);
            //alle Unterverzeichnisse kopieren
            foreach (string dir in dirs)
            {
                string destDir = dir.Replace(oldDir.TrimEnd(Path.DirectorySeparatorChar), newDir.TrimEnd(Path.DirectorySeparatorChar));

                CopyDirectory(dir, destDir);
            }
        }
    }
}
