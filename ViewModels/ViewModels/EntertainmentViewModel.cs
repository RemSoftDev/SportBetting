using System.Diagnostics;
using System.IO;
using System.Windows;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;
using SportRadar.Common.Logs;
using System;

namespace ViewModels.ViewModels
{
    public enum Virtuals
    {
        VFL = 0,
        VHC = 1
    }
    [ServiceAspect]
    public class EntertainmentViewModel : BaseViewModel
    {
        private Virtuals SelectedVirtual = Virtuals.VFL;

        private static ILog _logger = LogFactory.CreateLog(typeof(EntertainmentViewModel));
        //private const string OurURI = "http://vflsrterminal.aitcloud.de/vflterminal/vleague_terminal.php?clientid=642&lang=en&resolution=1024";
        private string _sVideoURI = "";

        public EntertainmentViewModel()
        {
            HidePleaseWait = false;
            WaitOverlayProvider.ShowWaitOverlay();

            ShowVFLCommand = new Command(ShowVFLButtonPress);
            ShowVHCCommand = new Command(ShowVHC);

            if (!StationRepository.AllowVfl)
            {
                SelectedVirtual = Virtuals.VHC;
            }
            else
            {
                SelectedVirtual = Virtuals.VFL;
            }

            //SetWebAddress();
        }

        private void SetWebAddress()
        {
            if (SelectedVirtual == Virtuals.VFL)
            {
                int iVFLSource = StationRepository.VFLSource;
                if (iVFLSource == 0)
                {
                    // load from server 
                    WebAddress = "http://vflsrterminal.aitcloud.de/vflterminal/vleague_terminal.php?clientid=642&lang=" + SelectedLanguage.ToLowerInvariant() + "&resolution=1024&channel=0";
                    Mediator.SendMessage<string>("0|" + WebAddress, MsgTag.SetUpBrowser);
                }
                else if (iVFLSource == 1)
                {
                    // read locally
                    //WebAddress = "http://localhost/w/Start.html?lang=" + SelectedLanguage.ToLowerInvariant();// +"&resolution=800";
                    if (File.Exists("file:///W:/start.html"))
                    {
                        WebAddress = "file:///W:/start.html";
                        Mediator.SendMessage<string>("0|" + WebAddress, MsgTag.SetUpBrowser);
                    }
                    else
                    {
                        WebAddress = "about:blank";
                        Mediator.SendMessage<string>("0|" + WebAddress, MsgTag.SetUpBrowser);
                    }

                }
            }
            else if (SelectedVirtual == Virtuals.VHC)
            {
                WebAddress = "http://vhcsrterminal.aitcloud.de/vhcshop/terminal/1024/clientid:642/lang:" + SelectedLanguage.ToLowerInvariant() + "/channel:0";
                Mediator.SendMessage<string>("0|" + WebAddress, MsgTag.SetUpBrowser);
                //WebAddress = "http://vhcdev.aitcloud.de/vhctesting/vhc/index/clientid:642/lang:" + SelectedLanguage.ToLowerInvariant(); //no hosted solution yet
            }
            else
            {
                WebAddress = "about:blank"; //no hosted solution yet
                Mediator.SendMessage<string>("0|" + WebAddress, MsgTag.SetUpBrowser);
            }

            _sVideoURI = WebAddress;
            OnPropertyChanged("WebAddress");
        }

        public override void OnNavigationCompleted()
        {
            Mediator.Register<string>(this, HeaderShowFirstView, MsgTag.RefreshStation);
            ChangeTracker.HeaderVisible = false;

            SelectAndShow(ChangeTracker.SourceType);

            try
            {
                Dispatcher.Invoke((Action)(() => ((Window)GetActiveWindow()).Focus()));
            }
            catch
            { }

            base.OnNavigationCompleted();
        }

        private void SelectAndShow(eServerSourceType? type)
        {
            ChangeTracker.SelectedVirtualSports = true;
            if ((type == eServerSourceType.BtrVfl || type == null) && StationRepository.AllowVfl)
            {
                SelectedVirtual = Virtuals.VFL;
                VFLSelected = true;
                ShowVFL();
            }
            else
            {
                SelectedVirtual = Virtuals.VHC;
                VHCSelected = true;
                ShowVHC();
            }
        }

        private string _currentPageToShow;
        public string CurrentPageToShow
        {
            get
            {
                //if (currentPage == 0 && totalPages == 0)
                //    return "";

                //return (currentPage + 1).ToString();
                return _currentPageToShow;
            }
            set
            {
                _currentPageToShow = value;
                OnPropertyChanged("CurrentPageToShow");
            }
        }

        public override void Close()
        {
            MyRegionManager.CloseAllViewsInRegion(RegionNames.VirtualContentRegion);
            MyRegionManager.ClearHistory(RegionNames.VirtualContentRegion);
            ChangeTracker.HeaderVisible = true;
            IsClosed = true;
            Mediator.UnregisterRecipientAndIgnoreTags(this);

            WebAddress = "about:blank";


            Process proc = new Process();
            proc.StartInfo.FileName = "sfc";
            proc.StartInfo.Arguments = "/scannow";
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Start();





        }

        public Command<IMatchVw> OpenMatch { get; private set; }
        public Command<IOddVw> PlaceBet { get; private set; }
        public Command<double> ScrollChangedCommand { get; private set; }
        public Command NextPageCmd { get; private set; }
        public Command PreviousPageCmd { get; private set; }
        public Command ShowVFLCommand { get; private set; }
        public Command ShowVHCCommand { get; private set; }

        public void ShowVFLButtonPress()
        {
            ChangeTracker.CurrentMatchOrRaceDay = null;
            ChangeTracker.CurrentSeasonOrRace = null;
            ChangeTracker.VhcSelectedType = null;
            ShowVFL();
        }

        public void ShowVFL()
        {
            ChangeTracker.SourceType = eServerSourceType.BtrVfl;
            SelectedVirtual = Virtuals.VFL;
            SetWebAddress();

            if (ChangeTracker.IsLandscapeMode || ChangeTracker.Is34Mode)
            {
                Mediator.SendMessage<long>(0, MsgTag.ShowVFL);
            }

            MyRegionManager.NavigateUsingViewModel<VFLViewModel>(RegionNames.VirtualContentRegion);

        }

        public void ShowVHC()
        {
            ChangeTracker.SourceType = eServerSourceType.BtrVhc;
            SelectedVirtual = Virtuals.VHC;
            SetWebAddress();

            if (ChangeTracker.IsLandscapeMode || ChangeTracker.Is34Mode)
            {
                Mediator.SendMessage<bool>(true, MsgTag.ShowVHC);
            }

            MyRegionManager.NavigateUsingViewModel<VHCViewModel>(RegionNames.VirtualContentRegion);
        }

        private string _webAddress;
        public string WebAddress
        {
            get
            {
                return _webAddress;
            }
            set
            {
                _webAddress = value;
                OnPropertyChanged("WebAddress");
            }
        }

        private bool _vflSelected = false;
        public bool VFLSelected
        {
            get
            {
                return _vflSelected;
            }
            set
            {
                _vflSelected = value;
                OnPropertyChanged("VFLSelected");
            }
        }

        private bool _vhcSelected = false;
        public bool VHCSelected
        {
            get { return _vhcSelected; }
            set { _vhcSelected = value; OnPropertyChanged("VHCSelected"); }
        }

        private Visibility _webBrowserVisibility = Visibility.Hidden;

        public Visibility WebBrowserVisibility
        {
            get { return _webBrowserVisibility; }
            set
            {
                _webBrowserVisibility = value;
                if (value == Visibility.Visible)
                {
                    _logger.Debug("hide PleaseWait entertaiment");
                    WaitOverlayProvider.DisposeAll();
                }
                OnPropertyChanged();
            }
        }

        private void HeaderShowFirstView(string obj)
        {
            if (!(StationRepository.AllowVfl || StationRepository.AllowVhc))
                Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);
        }
    }
}
