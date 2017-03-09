using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using SportRadar.Common.Logs;

//using NLogger;


namespace SportRadar.Common.Windows
{
    public sealed class LiveClient
    {
        private const int DEFAULT_TIMEOUT = 10000; // 10 Seconds
        private const int WAITFOR_TIMEOUT = 1000;  //  1 Second
        private const string NOT_READY_ERROR = "(LiveClient is not ready at the moment)";

        private static readonly ILog _logger = LogFactory.CreateLog(typeof(LiveClient));

        private object m_objLocker = new object();

        private string m_sHost = null;
        private int m_iPort = 0;
        private bool m_bUseSsl = false;
        private string m_sRemoteSecureHostName = null;
        private X509Certificate2 m_certificate = null;

        private TcpClient m_tcpClient = null;

        private MyStreamClass m_sr = new MyStreamClass();

#if DEBUG
        public MessageHistory SendMessageHistory { get; set; }
        public MessageHistory ReceiveMessageHistory { get; set; }
#endif

        private LiveClient(bool bUseSsl, string sRemoteSecureHostName, X509Certificate2 certificate)
        {
            m_bUseSsl = bUseSsl;
            m_sRemoteSecureHostName = sRemoteSecureHostName;
            m_certificate = certificate;

#if DEBUG
            this.SendMessageHistory = new MessageHistory();
            this.ReceiveMessageHistory = new MessageHistory();
#endif
        }

        private LiveClient(bool bUseSsl, X509Certificate2 certificate)
            : this(bUseSsl, string.Empty, certificate)
        {

        }

        public LiveClient(string sHost, int iPort, bool bUseSsl, string sRemoteSecureHostName, X509Certificate2 certificate)
            : this(bUseSsl, sRemoteSecureHostName, certificate)
        {
            m_sHost = sHost;
            m_iPort = iPort;
        }

        public LiveClient(string sHost, int iPort, bool bUseSsl, X509Certificate2 certificate)
            : this(sHost, iPort, bUseSsl, string.Empty, certificate)
        {
        }

        public static LiveClient FromTcpClient(TcpClient tcpClient, bool bUseSsl, X509Certificate2 certificate)
        {
            ExcpHelper.ThrowIf(tcpClient == null, "Parameter tcpClient is Null");

            LiveClient lcNew = new LiveClient(bUseSsl, certificate);

            return lcNew.Connect(tcpClient) ? lcNew : null;
        }

        public string HostName
        {
            get
            {
                return m_sHost;
            }
        }

        public int Port
        {
            get
            {
                return m_iPort;
            }
        }

        public bool Connect()
        {
            return this.Connect(null);
        }

        // DK - if param tcpClient is specified then it is Server Side and tcpClient came from method listener.AcceptTcpClient()
        // Else it is new ClientSide connection
        private bool Connect(TcpClient tcpClient)
        {
            try
            {
                lock (m_objLocker)
                {
                    if (tcpClient != null)
                    {
                        // Server
                        m_tcpClient = tcpClient;

                        IPEndPoint ep = tcpClient.Client.RemoteEndPoint as IPEndPoint;

                        m_sHost = ep == null ? "Undefined" : ep.Address.ToString();
                    }
                    else
                    {
                        // Client
                        _logger.InfoFormat("Connectiong to {0}:{1}{2}", m_sHost, m_iPort, m_bUseSsl ? " using SSL" : string.Empty);
                        m_tcpClient = new TcpClient(m_sHost, m_iPort);
                    }

                    m_tcpClient.ReceiveTimeout = DEFAULT_TIMEOUT;
                    m_tcpClient.SendTimeout = DEFAULT_TIMEOUT;

                    _logger.DebugFormat("LiveClient connected to Server {0}:{1}", m_sHost, m_iPort);

                    if (m_bUseSsl)
                    {
                        // DK - use certmgr.msc to check local certificates
                        _logger.Info("Authenticating...");
                        SslStream m_sslStream = null;
                        if (tcpClient != null)
                        {
                            // Server
                            ExcpHelper.ThrowIf(m_certificate == null, "LiveClient.Connect() ERROR. UseSSL is specified but certificate is null.");

                            m_sslStream = new SslStream(m_tcpClient.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                            m_sslStream.AuthenticateAsServer(m_certificate, true, SslProtocols.Default, false);
                        }
                        else
                        {
                            // Client
                            m_sslStream = new SslStream(m_tcpClient.GetStream());

                            if (m_certificate == null)
                            {
                                m_sslStream.AuthenticateAsClient(m_sRemoteSecureHostName);
                            }
                            else
                            {
                                m_sslStream.AuthenticateAsClient(m_sRemoteSecureHostName, new X509Certificate2Collection() { m_certificate }, SslProtocols.Default, false);
                            }

                            _logger.DebugFormat("LiveClient authenticated (SSL) at Server {0}:{1}", m_sHost, m_iPort);
                        }
                        m_sr.SslStream = m_sslStream;
                        //m_sw = new StreamWriter(m_sslStream);

                        return m_sslStream.IsAuthenticated;
                    }

                    _logger.Info("No SSL");

                    m_sr.NetworkStream = m_tcpClient.GetStream();
                    //m_sw = new StreamWriter(m_tcpClient.GetStream());

                    return m_tcpClient.Connected;
                }
            }
            catch (Exception excp)
            {
                m_tcpClient = null;
                _logger.WarnFormat("LiveClient COULD NOT establish connection to Server {0}:{1}\r\n{2}\r\n{3}", m_sHost, m_iPort, excp.Message, excp.StackTrace);
            }

            return false;
        }

        public bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyError)
        {
            if (sslPolicyError == SslPolicyErrors.None)
            {
                return true;
            }
            var ex =new Exception("LiveClient -> Server Certificate error:");
            _logger.Error("LiveClient -> Server Certificate error: " + sslPolicyError,ex);

            return false;
        }

        public bool ValidateClientCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyError)
        {
            if (sslPolicyError == SslPolicyErrors.None)
            {
                return true;
            }

            var ex = new Exception("LiveClient -> Server Certificate error:");
            _logger.Error("LiveClient -> Client Certificate error: " + sslPolicyError,ex);

            return false;
        }

        private bool IsConnectedAndReady
        {
            get
            {
                return
                    m_tcpClient != null &&
                    m_tcpClient.Connected &&
                    m_sr != null;
            }
        }

        public bool IsAlive
        {
            get
            {
                lock (m_objLocker)
                {
                    return m_bUseSsl ? this.IsConnectedAndReady : this.IsConnectedAndReady;
                }
            }
        }

        public void WaitUntillReadyOrThrow(int iTimeOutMilliseconds)
        {
            int iStart = 0;

            while (!this.IsAlive)
            {
                System.Threading.Thread.Sleep(WAITFOR_TIMEOUT);
                iStart += WAITFOR_TIMEOUT;

                ExcpHelper.ThrowIf(iStart >= iTimeOutMilliseconds, "LiveClient is not ready too long time (Requested TimeOut = {0})", iTimeOutMilliseconds);
            }
        }

        public TcpClient Client { get { return m_tcpClient; } }

        public bool HaveData
        {
            get { return m_tcpClient.Available > 0; }
        }

        private static string LimitLine(string sOriginal)
        {
            const int LIMIT = 256;
            return string.IsNullOrEmpty(sOriginal) || sOriginal.Length <= LIMIT ? sOriginal : sOriginal.Substring(0, LIMIT) + "...";
        }

        public void Write(string sText)
        {
            WaitUntillReadyOrThrow(DEFAULT_TIMEOUT);

            try
            {
                //_logger.DebugFormat("WriteLine(To = '{0}',  sLine.Length = {1}):\r\n{2}", m_sHost, sLine.Length, LimitLine(sLine));

                byte[] buffer = encoder.GetBytes(sText);
                m_sr.Write(buffer, 0, buffer.Length);
                m_sr.Flush();
                /*
#if DEBUG
                if (this.SendMessageHistory != null)
                {
                    this.SendMessageHistory.AddMessage(sText);
                    _logger.Debug("Send" + this.SendMessageHistory);
                }
#endif
                */
            }
            catch (Exception excp)
            {
                _logger.ErrorFormat("WriteLine(WriteLine(To = '{0}',  sLine.Length = {1}) ERROR {2}:\r\n{3}\r\n{4}",excp, m_sHost, sText.Length, this.IsAlive ? string.Empty : NOT_READY_ERROR, excp.Message, excp.StackTrace);
            }
        }

        public void WriteLine(string sLine)
        {
            WaitUntillReadyOrThrow(DEFAULT_TIMEOUT);

            try
            {
                //_logger.DebugFormat("WriteLine(To = '{0}',  sLine.Length = {1}):\r\n{2}", m_sHost, sLine.Length, LimitLine(sLine));
                byte[] buffer = encoder.GetBytes(sLine + "\r\n");

                m_sr.Write(buffer, 0, buffer.Length);
                m_sr.Flush();

                /*
#if DEBUG
                if (this.SendMessageHistory != null)
                {
                    this.SendMessageHistory.AddMessage(sLine);
                    _logger.Debug("Send" + this.SendMessageHistory);
                }
#endif
                */
            }
            catch (Exception excp)
            {
                _logger.ErrorFormat("WriteLine(WriteLine(To = '{0}',  sLine.Length = {1}) ERROR {2}:\r\n{3}\r\n{4}",excp, m_sHost, sLine.Length, this.IsAlive ? string.Empty : NOT_READY_ERROR, excp.Message, excp.StackTrace);
            }
        }

        UTF8Encoding encoder = new UTF8Encoding();

        public string ReadLine()
        {
            WaitUntillReadyOrThrow(DEFAULT_TIMEOUT);

            byte[] message = new byte[1];
            var bytesRead = m_sr.Read(message, 0, message.Length);
            //string sData = m_sr.Read(message, 0, 4096);
            StringBuilder sData = new StringBuilder();
            sData.Append(encoder.GetString(message, 0, bytesRead));
            while (bytesRead > 0)
            {
                bytesRead = m_sr.Read(message, 0, message.Length);
                sData.Append(encoder.GetString(message, 0, bytesRead));
                if (message[0] == 10)
                    return sData.ToString().TrimEnd('\r', '\n');
            }
            ExcpHelper.ThrowUp(new Exception("Read line exception"), "LiveClient is not ready too long time (Requested TimeOut = {0})", DEFAULT_TIMEOUT);
            return null;
        }

        public static void SafelyDispose(MyStreamClass dsp)
        {
            if (dsp != null)
            {
                try
                {
                    dsp.Dispose();
                }
                catch
                {
                }
            }
        }

        public bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            return true;

            _logger.Warn("Certificate error: " + sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        public void Disconnect()
        {
            this.Dispose();
            Thread.Sleep(1000);

        }

        public void Dispose()
        {
            _logger.Debug("LiveClient is disposing...");
            //_logger.DebugFormat("LiveClient is disposing...\r\n{0}", new StackTrace());

            if (m_tcpClient != null && m_tcpClient.Connected)
            {
                m_tcpClient.Close();
            }

            m_tcpClient = null;

            SafelyDispose(m_sr);
        }
    }

#if DEBUG

    public class MessageHistory
    {
        public static readonly TimeSpan DEFAULT_HISTORY_PERIOD = new TimeSpan(0, 10, 0); // 10 minutes

        protected object m_oLocker = new object();
        protected TimeSpan m_tsHistoryPeriod = DEFAULT_HISTORY_PERIOD;
        protected List<DateTime> m_lDateTime = new List<DateTime>();
        protected Dictionary<DateTime, string> m_diHistory = new Dictionary<DateTime, string>();

        public MessageHistory()
        {
        }

        public MessageHistory(TimeSpan tsHistoryPeriod)
        {
            m_tsHistoryPeriod = tsHistoryPeriod;
        }

        protected DateTime DetachOldMessages()
        {
            lock (m_oLocker)
            {
                DateTime dtMin = DateTime.Now - m_tsHistoryPeriod;

                while (m_lDateTime.Count > 0 && m_lDateTime[0] < dtMin)
                {
                    //                    Debug.Assert (m_diHistory.ContainsKey(m_lDateTime[0]));
                    //                    m_diHistory.Remove(m_lDateTime[0]);
                    m_lDateTime.RemoveAt(0);

                    //                    Debug.Assert(m_lDateTime.Count == m_diHistory.Count);
                }

                return m_lDateTime.Count > 0 ? m_lDateTime[0] : DateTime.Now;
            }
        }

        public void AddMessage(string sMessage)
        {
            DetachOldMessages();

            lock (m_oLocker)
            {
                DateTime dtNow = DateTime.Now;
                m_lDateTime.Add(dtNow);
                //                m_diHistory.Add(dtNow, sMessage);

                //                Debug.Assert(m_lDateTime.Count == m_diHistory.Count);
            }
        }

        public override string ToString()
        {
            DateTime dtLastMessageTime = DetachOldMessages();
            return string.Format("MessageHistory {0} messages for last {1} (Max Period = {2})", m_lDateTime.Count, DateTime.Now - dtLastMessageTime, m_tsHistoryPeriod);
        }
    }

#endif
}
