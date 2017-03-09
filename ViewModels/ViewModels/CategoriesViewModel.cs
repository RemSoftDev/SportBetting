using System;
using System.Linq;
using System.Windows.Data;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using System.Collections.Generic;
using SportRadar.DAL.OldLineObjects;

namespace ViewModels.ViewModels
{

    /// <summary>
    /// Categories view model.
    /// </summary>
    [ServiceAspect]
    public class CategoriesViewModel : BaseViewModel
    {

        #region Constructors
        private SortableObservableCollection<IMatchVw> _matches = new SortableObservableCollection<IMatchVw>();
        private static object _itemsLock = new object();

        public CategoriesViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(_categories, _itemsLock);

            Choice = new Command<long>(OnChoiceExecute);
            ScrollChangedCommand = new Command<double>(ScrollChanged);
            LayoutUpdatedCommand = new Command<double>(LayoutUpdated);
            Mediator.Register<string>(this, OnLanguageChosenExecute, MsgTag.LanguageChosen);
            Mediator.Register<bool>(this, Refresh, MsgTag.Refresh);
            Mediator.Register<string>(this, HeaderShowFirstView, MsgTag.RefreshStation);

            LineSr.SubsribeToEvent(LineSr_DataSqlUpdateSucceeded);
        }

        void LineSr_DataSqlUpdateSucceeded(SportRadar.DAL.CommonObjects.eUpdateType eut, string sProviderDescription)
        {
            if (eut == eUpdateType.PreMatches)
            {
                FillCategories();
            }
        }

        public void LayoutUpdated(double width)
        {
            ColumnsAmount = (Int32)(width / 259);
        }

        public override void OnNavigationCompleted()
        {
            Mediator.SendMessage("", MsgTag.ResetFilters);
            FillCategories();
            //Mediator.SendMessage<bool>(false, MsgTag.BlockSportFilter);
            //Mediator.SendMessage<bool>(false, MsgTag.BlockTimeFilter);
            Mediator.SendMessage(true, MsgTag.ActivateForwardSelected);
            ChangeTracker.SelectedSports = true;
            base.OnNavigationCompleted();
            Mediator.SendMessage<bool>(true, MsgTag.UpdateLiveMonitorTemplates);
        }




        #endregion

        #region Properties

        protected SortableObservableCollection<IMatchVw> Matches
        {
            get { return _matches; }
            set { _matches = value; }
        }


        public SortableObservableCollection<Category> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        private int _columnsAmount = 4;
        public int ColumnsAmount
        {
            get 
            {
                return _columnsAmount;
            }
            set 
            {
                if (_columnsAmount != value)
                {
                    _columnsAmount = value;
                    OnPropertyChanged("ColumnsAmount");
                }
            }
        }

        #endregion

        #region Commands
        /// <summary>
        /// Gets the Choice command.
        /// </summary>
        public Command<long> Choice { get; private set; }
        public Command<double> ScrollChangedCommand { get; private set; }
        public Command<double> LayoutUpdatedCommand { get; set; }

        #endregion

        #region Methods

        public override void Close()
        {
            LineSr.UnsubscribeFromEnent(LineSr_DataSqlUpdateSucceeded);
            base.Close();
        }
        object _lockerTimer = new object();
        private SortableObservableCollection<Category> _categories = new SortableObservableCollection<Category>();

        private void FillCategories()
        {
            lock (_lockerTimer)
            {

                Repository.FindMatches(Matches, "", SelectedLanguage, MatchFilter, delegate(IMatchVw m1, IMatchVw m2) { return 0; });

                var groups = Matches.Where(x => x.CategoryView != null).Select(x => x.CategoryView).Distinct().ToList();

                foreach (var group in groups)
                {
                    //string descriptor = Matches.Where(x => x.CategoryView != null && x.CategoryView.LineObject.GroupId == group.LineObject.GroupId).FirstOrDefault().SportDescriptor;

                    //set icon for mixed sports
                    List<IMatchVw> CategorieMatches = Matches.Where(x => x.CategoryView != null && x.CategoryView.LineObject.GroupId == group.LineObject.GroupId).ToList();
                    string descriptor = CategorieMatches[0].SportDescriptor;
                    foreach (IMatchVw match in CategorieMatches)
                    {
                        if (match.SportDescriptor != descriptor)
                        {
                            descriptor = SportSr.SPORT_DESCRIPTOR_MIXED;
                            break;
                        }
                    }

                    if (Categories.Count(x => x.Id == group.LineObject.GroupId) == 0)
                        Categories.Add(new Category() { Name = group.DisplayName, Id = group.LineObject.GroupId, Sort = group.LineObject.Sort.Value, SportDescriptor = descriptor });
                }
                Categories.Sort(delegate(Category m1, Category m2) { return m1.Sort.CompareTo(m2.Sort); });
                for (int i = 0; i < Categories.Count; )
                {
                    var comboBoxItem = Categories[i];

                    if (groups.Count(x => x.LineObject.GroupId == comboBoxItem.Id) == 0)
                    {
                        Categories.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }

            }

        }

        private bool MatchFilter(IMatchLn matchLn)
        {

            if (!matchLn.Active.Value)
                return false;

            if (matchLn.IsLiveBet.Value)
                return false;

            if (matchLn.MatchView.CategoryView == null)
                return false;

            if (matchLn.MatchView.VisibleBetDomainCount == 0)
                return false;

            if (matchLn.MatchView.AllVisibleOddCount == 0)
                return false;

            if (matchLn.ExpiryDate.Value.LocalDateTime < DateTime.Now)
                return false;

            if(matchLn.MatchView.TournamentView != null)
                if (!LineSr.IsTournamentVisible(matchLn.MatchView.TournamentView.LineObject.SvrGroupId.ToString()))
                    return false;

            return true;
        }


        private void OnChoiceExecute(long id)
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.NavigateUsingViewModel<TournamentsViewModel>(RegionNames.ContentRegion, id);
        }


        private void OnLanguageChosenExecute(string lang)
        {
            FillCategories();
        }

        private void Refresh(bool state)
        {
            Categories.Clear();
            FillCategories();
        }

        private void HeaderShowFirstView(string obj)
        {
            if (!StationRepository.IsPrematchEnabled)
                Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);
        }

        #endregion
    }
}