using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared.Models;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using System.Globalization;

namespace ViewModels.ViewModels
{

    /// <summary>
    /// MainWindow view model.
    /// </summary>
    [ServiceAspect]
    public class UserTicketsViewModel : BaseViewModel
    {
        #region Variables

        public int Pagesize = 0;
        double GridHeight { get; set; }
        double RowHeight { get; set; }

        #endregion

        #region Constructor & destructor

        public UserTicketsViewModel()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            HidePleaseWait = false;
            ItemCreated = new Command<UIElement>(OnRowItemCreated);
            GridCreated = new Command<UIElement>(OnGridCreated);
            ShowTicketCommand = new Command<TicketView>(OnShowTicket);
            TicketTypeCommand = new Command<string>(OnTicketTypeCommand);

            PreviousPage = new Command(OnPreviousPage);
            FirstPage = new Command(OnFirstPage);
            NextPage = new Command(OnNextPage);
            LastPage = new Command(OnLastPage);
            TicketsStartPage = ChangeTracker.TicketsStartPage;

            ShowTickets(new[] { new UserTicket() });

        }

        #endregion

        #region Properties

        public int AllPages
        {
            get { return _allPages; }
            set
            {
                _allPages = value;
                OnPropertyChanged("AllPages");
            }
        }

        public int SelectedType
        {
            get { return ChangeTracker.SelectedTycketType; }
            set 
            {
                ChangeTracker.SelectedTycketType = value;
                OnPropertyChanged("SelectedType");
                TicketsStartPage = 1;
                ChangeTracker.TicketsStartPage = TicketsStartPage;
                UpdateTickets();
            }
        }

        //public ComboBoxItem SelectedType
        //{
        //    get { return ChangeTracker.SelectedType; }
        //    set
        //    {
        //        ChangeTracker.SelectedType = value;
        //        OnPropertyChanged("SelectedType");
        //        TicketsStartPage = 1;
        //        ChangeTracker.TicketsStartPage = TicketsStartPage;
        //        UpdateTickets();
        //    }
        //}

        public int TicketsStartPage
        {
            get { return _ticketsStartPage; }
            set
            {
                _ticketsStartPage = value;
                OnPropertyChanged();
            }
        }

        protected TicketWS CurrentTicket
        {
            get { return ChangeTracker.CurrentTicket; }
            set { ChangeTracker.CurrentTicket = value; }
        }

        //protected ObservableCollection<TicketDetailsWS> CurrentTicketDetails
        //{
        //    get { return ChangeTracker.CurrentTicketDetails; }
        //    set { ChangeTracker.CurrentTicketDetails = value; }
        //}


        public ObservableCollection<TicketView> Tickets
        {
            get { return ChangeTracker.Tickets; }
            set
            {
                ChangeTracker.Tickets = value;
                OnPropertyChanged();
            }
        }

        private string _errorLabel;
        private int _allPages;
        private int _ticketsStartPage;

        public string ErrorLabel
        {
            get { return _errorLabel; }
            set
            {
                _errorLabel = value;
                OnPropertyChanged("ErrorLabel");
                OnPropertyChanged("ErrorVisible");
            }
        }

        public bool ErrorVisible
        {
            get
            {
                return string.IsNullOrEmpty(ErrorLabel);
            }
        }


        #endregion

        #region Commands
        public Command PreviousPage { get; set; }
        public Command FirstPage { get; set; }
        public Command NextPage { get; set; }
        public Command LastPage { get; set; }
        public Command<UIElement> ItemCreated { get; set; }
        public Command<UIElement> GridCreated { get; set; }
        public Command<TicketView> ShowTicketCommand { get; set; }
        public Command<string> TicketTypeCommand { get; private set; }

        #endregion


        #region Methods

        public override void OnNavigationCompleted()
        {
            base.OnNavigationCompleted();
        }

        [WsdlServiceSyncAspect]
        private void OnShowTicket(TicketView ticketView)
        {
            WaitOverlayProvider.ShowWaitOverlay();
            try
            {
                CurrentTicket = WsdlRepository.LoadTicket(ticketView.Number, ticketView.CheckSum, StationRepository.StationNumber, SelectedLanguage, SelectedLanguage, true);

                MyRegionManager.NavigateUsingViewModel<TicketDetailsViewModel>(RegionNames.UserProfileContentRegion);
            }
            catch (System.ServiceModel.FaultException<HubServiceException> exception)
            {
                ShowError(exception.Detail.message);
            }
        }



        private void OnGridCreated(UIElement obj)
        {
            if (GridHeight > 0)
                return;

            GridHeight = obj.RenderSize.Height;

            if (GridHeight > 0 && RowHeight > 0 && Tickets[0].Hidden)
            {
                UpdateTickets();
            }
        }

        private void OnRowItemCreated(UIElement obj)
        {
            if (RowHeight > 0)
                return;
            RowHeight = obj.RenderSize.Height;

            if (GridHeight > 0 && RowHeight > 0 && Tickets[0].Hidden)
            {
                UpdateTickets();
            }
        }


        private void OnPreviousPage()
        {
            if (TicketsStartPage < 2)
                return;

            TicketsStartPage--;
            ChangeTracker.TicketsStartPage = TicketsStartPage;
            UpdateTickets();
        }

        private void OnFirstPage()
        {
            if (TicketsStartPage < 2)
                return;

            TicketsStartPage = 1;
            ChangeTracker.TicketsStartPage = TicketsStartPage;
            UpdateTickets();
        }

        private void OnNextPage()
        {
            if (TicketsStartPage < AllPages)
                TicketsStartPage++;
            ChangeTracker.TicketsStartPage = TicketsStartPage;

            UpdateTickets();
        }

        private void OnLastPage()
        {
            if (TicketsStartPage < AllPages)
                TicketsStartPage = AllPages;
            ChangeTracker.TicketsStartPage = TicketsStartPage;
            UpdateTickets();
        }

        private void ShowTickets(UserTicket[] obj)
        {

            Tickets.Clear();
            foreach (var ticket in obj)
            {
                string name = "";
                int id = (int)ticket.ticketCategory;
                switch (id)
                {
                    case 0: 
                        name = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_ALL).ToString();
                        break;
                    case 1:
                        name = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKETWON).ToString();
                        break;
                    case 2:
                        name = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKETLOST).ToString();
                        break;
                    case 3:
                        name = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CANCELLED).ToString();
                        break;
                    case 4:
                        name = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKETOPEN).ToString();
                        break;
                    case 5:
                        name = TranslationProvider.Translate(MultistringTags.TERMINAL_PENDING_APPROVAL).ToString();
                        break;
                }
                    var ticketView = new TicketView(ticket.ticketNumber, ticket.checkSum, name, id, ticket.createdAt, Currency);
                    Tickets.Add(ticketView);
            }
            if (RowHeight == 0 && Tickets.Count > 0)
            {
                if (Tickets[0].CreatedAt == DateTime.MinValue)
                    Tickets[0].Hidden = true;
            }
        }


        private void OnTicketTypeCommand(string day)
        {
            SelectedType = Int16.Parse(day, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
        }


        private void UpdateTickets()
        {
            PleaseWaitOnUpdateTickets();
        }

        [WsdlServiceSyncAspect]
        private void PleaseWaitOnUpdateTickets()
        {

            ErrorLabel = string.Empty;
            if (GridHeight > 0 && RowHeight > 0)
                Pagesize = (int)(GridHeight / RowHeight);
            else
            {
                Pagesize = 100;
            }


            UserTicket[] tickets = new UserTicket[0];
            long total = 1;
            string tempTotal = "1";
            try
            {
                tickets = WsdlRepository.GetUserTickets(ChangeTracker.CurrentUser.AccountId.ToString(), (ticketCategory)SelectedType, new AccountTicketSorting() { field = AccountTicketSortingFields.DateCreated, value = AccountTicketSortingValues.Desc }, (int)((TicketsStartPage - 1) * Pagesize), (int)Pagesize, out tempTotal);


            }
            catch (System.ServiceModel.FaultException<HubServiceException> exception)
            {

                if (exception.Detail.code == 131)
                {
                    tickets = new UserTicket[0];
                    ErrorLabel = TranslationProvider.Translate(MultistringTags.TERMINAL_NO_TICKET_FOUND).ToString();
                }
                else
                {
                    ShowError(exception.Detail.message);
                }
            }
            total = Convert.ToInt64(tempTotal);
            if (tickets != null)
            {
                ShowTickets(tickets);
            }

            double pagesCount = total / (double)Pagesize;
            if (pagesCount < 1)
            {
                pagesCount = 1;
            }
            if (pagesCount % 1 > 0)
            {
                pagesCount++;
            }
            AllPages = (int)pagesCount;


        }

        #endregion
    }
}