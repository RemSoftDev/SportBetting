using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using BaseObjects;
using BaseObjects.ViewModels;
using IocContainer;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common.Collections;
using WsdlRepository;

namespace ViewModels.ViewModels
{

    [ServiceAspect]
    public class TestInputViewModel : IBaseViewModel, INotifyPropertyChanged
    {

        private string _text = "";
        private SyncObservableCollection<string> _cardNumbers = new SyncObservableCollection<string>();
        private bool _isTicket = true;
        private bool _isCredit;
        private bool _isPayment;
        private bool _isRegistration;
        private object _itemsLock1 = new object();
        public MessageStorage Mediator { get; private set; }
        private IChangeTracker _changeTracker;
        private bool _isNetworkLost;

        public IChangeTracker ChangeTracker
        {
            get
            {
                return _changeTracker ?? (_changeTracker = IoCContainer.Kernel.Get<IChangeTracker>());
            }
        }
        #region Constructors

        private IStationSettings _stationSettings;
        public IStationSettings StationSettings
        {
            get
            {
                return _stationSettings ?? (_stationSettings = IoCContainer.Kernel.Get<IStationSettings>());
            }
        }


        public TestInputViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(_cardNumbers, _itemsLock1);
            ChangeTracker.TestInputActive = true;
            Mediator = new MessageStorage();
            BarcodeCommand = new Command(BarcodeClick);
            InsertCardCommand = new Command(InsertCard);
            EjectCardCommand = new Command(EjectCard);
            Mediator.Register<string>(this, WriteCardNumber, MsgTag.TestWriteNumber);
            var timer = new System.Timers.Timer(100);
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            MoneyEnabled = StationSettings.IsCashInEnabled;
        }




        [AsyncMethod]
        private void EjectCard()
        {
            Mediator.SendMessage<long>(0, MsgTag.CardRemoved);

        }
        [AsyncMethod]
        private void InsertCard()
        {
            Mediator.SendMessage<string>("", MsgTag.StartedCardReading);
            Thread.Sleep(1000);
            Mediator.SendMessage(true, MsgTag.ClosePinWindow);
            Mediator.SendMessage<string>(Text, MsgTag.CardInserted);


        }

        [AsyncMethod]
        private void BarcodeClick()
        {
            BarCodeConverter.Clear();

            Mediator.SendMessage(((char)2), MsgTag.EmulateBarcode);

            Thread.Sleep(10);


            if (IsTicket)
            {
                Mediator.SendMessage(((int)BarCodeConverter.BarcodeType.TICKET).ToString()[0], MsgTag.EmulateBarcode);

            }
            if (IsCredit)
            {
                Mediator.SendMessage(((int)BarCodeConverter.BarcodeType.CREDIT_NOTE).ToString()[0], MsgTag.EmulateBarcode);
            }
            if (IsPayment)
            {
                Mediator.SendMessage(((int)BarCodeConverter.BarcodeType.PAYMENT_NOTE).ToString()[0], MsgTag.EmulateBarcode);
            }
            if (IsRegistration)
            {
                Mediator.SendMessage(((int)BarCodeConverter.BarcodeType.REGISTRATION_NOTE).ToString()[0], MsgTag.EmulateBarcode);
            }
            if (IsCardBarcode)
            {
                Mediator.SendMessage(((int)BarCodeConverter.BarcodeType.CARDBARCODE).ToString()[0], MsgTag.EmulateBarcode);
            }

            Thread.Sleep(10);
            foreach (var str in Text)
            {
                Mediator.SendMessage(str, MsgTag.EmulateBarcode);


                Thread.Sleep(10);

            }
            Mediator.SendMessage("\r"[0], MsgTag.EmulateBarcode);

        }

        private void WriteCardNumber(string obj)
        {
            CardNumbers.Insert(0, obj);

            Thread.Sleep(100);
            ChangeTracker.CardNumber = obj;

        }

        #endregion


        #region Properties

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged("Text");
            }
        }

        public bool IsTicket
        {
            get { return _isTicket; }
            set
            {
                _isTicket = value;
                OnPropertyChanged();
            }
        }

        public bool IsCredit
        {
            get { return _isCredit; }
            set
            {
                _isCredit = value;
                OnPropertyChanged();
            }
        }

        public bool IsPayment
        {
            get { return _isPayment; }
            set
            {
                _isPayment = value;
                OnPropertyChanged();
            }
        }

        public bool MoneyEnabled
        {
            get { return _moneyEnabled; }
            set
            {
                _moneyEnabled = value;
                OnPropertyChanged();
            }
        }

        private IStationRepository _stationRepository;
        private bool _isCardBarcode;
        private int _selectedIndex;
        private bool _moneyEnabled;

        public IStationRepository StationRepository
        {
            get
            {
                return _stationRepository ?? (_stationRepository = IoCContainer.Kernel.Get<IStationRepository>());
            }
        }


        public bool IsCardBarcode
        {
            get { return _isCardBarcode; }
            set
            {
                _isCardBarcode = value;
                OnPropertyChanged();
            }
        }

        public bool IsRegistration
        {
            get { return _isRegistration; }
            set
            {
                _isRegistration = value;
                OnPropertyChanged();
            }
        }
        public bool IsNetworkLost
        {
            get { return _isNetworkLost; }
            set
            {
                _isNetworkLost = value;
                new Thread(() =>
                    {
                        while (_isNetworkLost)
                        {
                            DisconnectWrapper.CloseRemoteIP("10.11.26.74");
                        }
                    }).Start();

                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public Command BarcodeCommand { get; set; }
        public Command InsertCardCommand { get; set; }
        public Command EjectCardCommand { get; set; }

        public SyncObservableCollection<string> CardNumbers
        {
            get { return _cardNumbers; }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsClosed { get; set; }
        public void Close()
        {
            ChangeTracker.TestInputActive = false;
            Mediator.UnregisterRecipientAndIgnoreTags(this);

        }

        public void OnNavigationCompleted()
        {

        }

        public UserControl View { get; set; }

        public bool IsReady { get; private set; }

        public Window ViewWindow { get; set; }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                if (value >= 0)
                {
                    Text = CardNumbers[value];
                }

            }
        }

        public void ShowMessage(string msg1)
        {

        }

        public void ShowError(string obj, EventHandler okClick = null, bool bCreateButtonEvent = false, int iAddCounterSeconds = 0)
        {

        }
    }
    /// <summary>
    /// This is a generic class for disconnecting TCP connections.
    /// This class can be used to
    /// 1. Get a list of all connections.
    /// 2. Cloas a connection
    /// </summary>
    public static class DisconnectWrapper
    {
        /// <summary>
        /// Enumeration of connection states
        /// </summary>
        public enum ConnectionState
        {
            All = 0,
            Closed = 1,
            Listen = 2,
            Syn_Sent = 3,
            Syn_Rcvd = 4,
            Established = 5,
            Fin_Wait1 = 6,
            Fin_Wait2 = 7,
            Close_Wait = 8,
            Closing = 9,
            Last_Ack = 10,
            Time_Wait = 11,
            Delete_TCB = 12
        }

        /// <summary>
        /// Connection information
        /// </summary>
        private struct ConnectionInfo
        {
            public int dwState;
            public int dwLocalAddr;
            public int dwLocalPort;
            public int dwRemoteAddr;
            public int dwRemotePort;
        }

        /// <summary>
        /// Win 32 API for get all connection
        /// </summary>
        /// <param name="pTcpTable">Pointer to TCP table</param>
        /// <param name="pdwSize">Size</param>
        /// <param name="bOrder">Order</param>
        /// <returns>Number</returns>
        [DllImport("iphlpapi.dll")]
        private static extern int GetTcpTable(IntPtr pTcpTable, ref int pdwSize, bool bOrder);

        /// <summary>
        /// Set the connection state
        /// </summary>
        /// <param name="pTcprow">Pointer to TCP table row</param>
        /// <returns>Status</returns>
        [DllImport("iphlpapi.dll")]
        private static extern int SetTcpEntry(IntPtr pTcprow);

        /// <summary>
        /// Convert 16-bit value from network to host byte order
        /// </summary>
        /// <param name="netshort">network host</param>
        /// <returns>host byte order</returns>
        [DllImport("wsock32.dll")]
        private static extern int ntohs(int netshort);

        /// <summary>
        /// //Convert 16-bit value back again
        /// </summary>
        /// <param name="netshort"></param>
        /// <returns></returns>
        [DllImport("wsock32.dll")]
        private static extern int htons(int netshort);

        /// <summary>
        /// Close all connection to the remote IP
        /// </summary>
        /// <param name="IP">IP to close</param>
        public static void CloseRemoteIP(string IP)
        {
            ConnectionInfo[] rows = getTcpTable();
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i].dwRemoteAddr == IPStringToInt(IP))
                {
                    rows[i].dwState = (int)ConnectionState.Delete_TCB;
                    IntPtr ptr = GetPtrToNewObject(rows[i]);
                    int ret = SetTcpEntry(ptr);
                }
            }
        }

        /// <summary>
        /// Close all connections at current local IP
        /// </summary>
        /// <param name="IP">IP to close</param>
        public static void CloseLocalIP(string IP)
        {
            ConnectionInfo[] rows = getTcpTable();
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i].dwLocalAddr == IPStringToInt(IP))
                {
                    rows[i].dwState = (int)ConnectionState.Delete_TCB;
                    IntPtr ptr = GetPtrToNewObject(rows[i]);
                    int ret = SetTcpEntry(ptr);
                }
            }
        }

        /// <summary>
        /// //Closes all connections to the remote port
        /// </summary>
        /// <param name="port">Port to close</param>
        public static void CloseRemotePort(int port)
        {
            ConnectionInfo[] rows = getTcpTable();
            for (int i = 0; i < rows.Length; i++)
            {
                if (port == ntohs(rows[i].dwRemotePort))
                {
                    rows[i].dwState = (int)ConnectionState.Delete_TCB;
                    IntPtr ptr = GetPtrToNewObject(rows[i]);
                    int ret = SetTcpEntry(ptr);
                }
            }
        }

        /// <summary>
        /// //Closes all connections to the local port
        /// </summary>
        /// <param name="port">Local port</param>
        public static void CloseLocalPort(int port)
        {
            ConnectionInfo[] rows = getTcpTable();
            for (int i = 0; i < rows.Length; i++)
            {
                if (port == ntohs(rows[i].dwLocalPort))
                {
                    rows[i].dwState = (int)ConnectionState.Delete_TCB;
                    IntPtr ptr = GetPtrToNewObject(rows[i]);
                    int ret = SetTcpEntry(ptr);
                }
            }
        }

        /// <summary>
        /// Close a connection by returning the connectionstring
        /// </summary>
        /// <param name="connectionstring">Connection to close</param>
        public static void CloseConnection(string connectionstring)
        {
            try
            {
                //Split the string to its subparts
                string[] parts = connectionstring.Split('-');
                if (parts.Length != 4) throw new Exception("Invalid connectionstring - use the one provided by Connections.");
                string[] loc = parts[0].Split(':');
                string[] rem = parts[1].Split(':');
                string[] locaddr = loc[0].Split('.');
                string[] remaddr = rem[0].Split('.');
                //Fill structure with data
                ConnectionInfo row = new ConnectionInfo();
                row.dwState = 12;
                byte[] bLocAddr = new byte[] { byte.Parse(locaddr[0]), byte.Parse(locaddr[1]), byte.Parse(locaddr[2]), byte.Parse(locaddr[3]) };
                byte[] bRemAddr = new byte[] { byte.Parse(remaddr[0]), byte.Parse(remaddr[1]), byte.Parse(remaddr[2]), byte.Parse(remaddr[3]) };
                row.dwLocalAddr = BitConverter.ToInt32(bLocAddr, 0);
                row.dwRemoteAddr = BitConverter.ToInt32(bRemAddr, 0);
                row.dwLocalPort = htons(int.Parse(loc[1]));
                row.dwRemotePort = htons(int.Parse(rem[1]));
                //Make copy of the structure into memory and use the pointer to call SetTcpEntry
                IntPtr ptr = GetPtrToNewObject(row);
                int ret = SetTcpEntry(ptr);
                if (ret == -1) throw new Exception("Unsuccessful");
                if (ret == 65) throw new Exception("User has no sufficient privilege to execute this API successfully");
                if (ret == 87) throw new Exception("Specified port is not in state to be closed down");
                if (ret != 0) throw new Exception("Unknown error (" + ret + ")");
            }
            catch (Exception ex)
            {
                throw new Exception("CloseConnection failed (" + connectionstring + ")! [" + ex.GetType().ToString() + "," + ex.Message + "]");
            }
        }

        /// <summary>
        /// Get all connection
        /// </summary>
        /// <returns>Array of connection string</returns>
        public static string[] Connections()
        {
            return Connections(ConnectionState.All);
        }

        /// <summary>
        /// Get connections based on the state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string[] Connections(ConnectionState state)
        {
            ConnectionInfo[] rows = getTcpTable();

            ArrayList arr = new ArrayList();

            foreach (ConnectionInfo row in rows)
            {
                if (state == ConnectionState.All || state == (ConnectionState)row.dwState)
                {
                    string localaddress = IPIntToString(row.dwLocalAddr) + ":" + ntohs(row.dwLocalPort);
                    string remoteaddress = IPIntToString(row.dwRemoteAddr) + ":" + ntohs(row.dwRemotePort);
                    arr.Add(localaddress + "-" + remoteaddress + "-" + ((ConnectionState)row.dwState).ToString() + "-" + row.dwState);
                }
            }

            return (string[])arr.ToArray(typeof(System.String));
        }

        /// <summary>
        /// The function that fills the ConnectionInfo array with connectioninfos
        /// </summary>
        /// <returns>ConnectionInfo</returns>
        private static ConnectionInfo[] getTcpTable()
        {
            IntPtr buffer = IntPtr.Zero; bool allocated = false;
            try
            {
                int iBytes = 0;
                GetTcpTable(IntPtr.Zero, ref iBytes, false); //Getting size of return data
                buffer = Marshal.AllocCoTaskMem(iBytes); //allocating the datasize

                allocated = true;
                GetTcpTable(buffer, ref iBytes, false); //Run it again to fill the memory with the data
                int structCount = Marshal.ReadInt32(buffer); // Get the number of structures
                IntPtr buffSubPointer = buffer; //Making a pointer that will point into the buffer
                buffSubPointer = (IntPtr)((int)buffer + 4); //Move to the first data (ignoring dwNumEntries from the original MIB_TCPTABLE struct)
                ConnectionInfo[] tcpRows = new ConnectionInfo[structCount]; //Declaring the array
                //Get the struct size
                ConnectionInfo tmp = new ConnectionInfo();
                int sizeOfTCPROW = Marshal.SizeOf(tmp);
                //Fill the array 1 by 1
                for (int i = 0; i < structCount; i++)
                {
                    tcpRows[i] = (ConnectionInfo)Marshal.PtrToStructure(buffSubPointer, typeof(ConnectionInfo)); //copy struct data
                    buffSubPointer = (IntPtr)((int)buffSubPointer + sizeOfTCPROW); //move to next structdata
                }

                return tcpRows;
            }
            catch (Exception ex)
            {
                throw new Exception("getTcpTable failed! [" + ex.GetType().ToString() + "," + ex.Message + "]");
            }
            finally
            {
                if (allocated) Marshal.FreeCoTaskMem(buffer); //Free the allocated memory
            }
        }

        /// <summary>
        /// Object pointer
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Pointer</returns>
        private static IntPtr GetPtrToNewObject(object obj)
        {
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(obj));
            Marshal.StructureToPtr(obj, ptr, false);
            return ptr;
        }

        /// <summary>
        /// IP to Int
        /// </summary>
        /// <param name="IP">IP Address</param>
        /// <returns>Integer</returns>
        private static int IPStringToInt(string IP)
        {
            if (IP.IndexOf(".") < 0) throw new Exception("Invalid IP address");
            string[] addr = IP.Split('.');
            if (addr.Length != 4) throw new Exception("Invalid IP address");
            byte[] bytes = new byte[] { byte.Parse(addr[0]), byte.Parse(addr[1]), byte.Parse(addr[2]), byte.Parse(addr[3]) };
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// IP int to String
        /// </summary>
        /// <param name="IP">IP</param>
        /// <returns>String</returns>
        private static string IPIntToString(int IP)
        {
            byte[] addr = System.BitConverter.GetBytes(IP);
            return addr[0] + "." + addr[1] + "." + addr[2] + "." + addr[3];
        }
    }
}