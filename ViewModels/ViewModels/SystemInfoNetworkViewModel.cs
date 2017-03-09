using System;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using TranslationByMarkupExtension;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// Categories view model.
    /// </summary>
    ///  

    [ServiceAspect]
    public class SystemInfoNetworkViewModel : BaseViewModel
    {
            
        #region Constructors

        public SystemInfoNetworkViewModel()
        {
            ChangeTracker.SystemInfoNetworkChecked = true;
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_PRINT_SYSTEM;
            ChangeTracker.AdminTitle2 =MultistringTags.TERMINAL_SYSTEM_INFO;
        }

        #endregion

        #region Properties

       
        public bool IsNetworkConnected
        {
            get { return _isNetworkConnected; }
            set { _isNetworkConnected = value; OnPropertyChanged("IsNetworkConnected"); }
        }

        public string IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; OnPropertyChanged("IpAddress"); }
        }

       

        public string MaskAddress
        {
            get { return _maskAddress; }
            set { _maskAddress = value; OnPropertyChanged("MaskAddress"); }
        }

        public string GatewayAddress
        {
            get { return _gatewayAddress; }
            set { _gatewayAddress = value; OnPropertyChanged("GatewayAddress"); }
        }

        public string DnsAddress
        {
            get { return _dnsAddress; }
            set { _dnsAddress = value; OnPropertyChanged("DnsAddress"); }
        }

        public string ComputerName
        {
            get { return _computerName; }
            set { _computerName = value; OnPropertyChanged("ComputerName"); }
        }

        public string FullComputerName
        {
            get { return _fullComputerName; }
            set { _fullComputerName = value; OnPropertyChanged("FullComputerName"); }
        }

        public string Workgroup
        {
            get { return _workgroup; }
            set { _workgroup = value; OnPropertyChanged("Workgroup"); }
        }
      
        private bool _isNetworkConnected;
        private string _ipAddress;
        private string _maskAddress;
        private string _gatewayAddress;
        private string _dnsAddress;
        private string _computerName;
        private string _fullComputerName;
        private string _workgroup;
  
        #endregion

        #region Commands

       
        #endregion

        #region Methods
        public override void OnNavigationCompleted()
        {
            ChangeTracker.SystemInfoNetworkChecked = true;
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_PRINT_SYSTEM;
            ChangeTracker.AdminTitle2 = MultistringTags.TERMINAL_INFO_NETWORK_TITLE;
            ShowSystemInfoNetwork("");
            base.OnNavigationCompleted();
        }

        public override void Close()
        {
            base.Close();
        }

        private void ShowSystemInfoNetwork(String s)
        {
            try
            {
                LoadNetworkInfo();
            }
            catch
            {
            }
        
        }

        private void LoadNetworkInfo()
        {
            IsNetworkConnected = NetworkInterface.GetIsNetworkAvailable();
          
            if (IsNetworkConnected)
            {
                string hostName = Dns.GetHostName();
                IPHostEntry local = Dns.GetHostByName(hostName);
                //_ipAddress = local.AddressList.First().ToString();
                ComputerName = hostName;
                FullComputerName = local.HostName;

                string query = "SELECT * FROM Win32_NetworkAdapterConfiguration"
                     + " WHERE IPEnabled = 'TRUE'";
                ManagementObjectSearcher moSearch = new ManagementObjectSearcher(query);
                ManagementObjectCollection moCollection = moSearch.Get();

                // Every record in this collection is a network interface
                foreach (ManagementObject mo in moCollection)
                { 
                    
                    IpAddress = ((string[])mo["IPAddress"])[0];
                    MaskAddress = ((string[])mo["IPSubnet"])[0];
                    GatewayAddress = ((string[])mo["DefaultIPGateway"])[0];
                    DnsAddress = (string)mo["DNSDomain"];
                    break;
                }
                Workgroup = System.Environment.GetEnvironmentVariable("USERDOMAIN");
              
            }
        }


        #endregion
    }
}