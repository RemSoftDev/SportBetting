using System;
using System.Linq;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// UserProfile view model.
    /// </summary>
    [ServiceAspect]
    public class OperatorProfileViewModel : BaseViewModel
    {

        #region Constructors

        public OperatorProfileViewModel()
        {
            BackCommand = new Command(OnBackCommand);
            BlockOperatorCommand = new Command(OnBlockOperator);
            BlockCardCommand = new Command(OnBlockCardOperator);


        }

        #endregion

        #region Properties


        public bool EnabledEnableOperator
        {
            get { return !ChangeTracker.FoundOperator.Active && ChangeTracker.CurrentUser.BlockOperator; }
        }

        public bool EnabledBlockOperator
        {
            get { return ChangeTracker.FoundOperator.Active && ChangeTracker.CurrentUser.BlockOperator; }
        }

        public bool EnabledBlockIdCardOperator
        {
            get { return ChangeTracker.FoundOperator.ActiveCard && ChangeTracker.CurrentUser.BlockOperatorCard; }

        }



        #endregion

        #region Commands

        public Command BackCommand { get; private set; }
        public Command BlockOperatorCommand { get; private set; }
        public Command BlockCardCommand { get; private set; }

        #endregion

        #region Methods

        public override void OnNavigationCompleted()
        {
            MyRegionManager.NavigateUsingViewModel<ChangeOperatorViewModel>(RegionNames.OperatorManagementProfileRegion);

            base.OnNavigationCompleted();
        }

        public override void Close()
        {
            MyRegionManager.CloseAllViewsInRegion(RegionNames.OperatorManagementProfileRegion);
            base.Close();
        }


        [AsyncMethod]
        private void OnBlockCardOperator()
        {
            var text = TranslationProvider.Translate(MultistringTags.TERMINAL_BLOCK_ID_CONFIRMATION).ToString();
            if (!StationRepository.IsIdCardEnabled)
                text = TranslationProvider.Translate(MultistringTags.TERMINAL_BLOCK_BARCODE_ID_CONFIRMATION).ToString();
            QuestionWindowService.ShowMessage(text, null, null, questionViewModel_YesClick, null, true);
        }

        void questionViewModel_YesClick(object sender, EventArgs e)
        {
            BlockCardOperator();
        }

        [WsdlServiceSyncAspect]
        private void BlockCardOperator()
        {
            try
            {
                IdCardInfoItem[] idCards = WsdlRepository.GetIdCardInfo(ChangeTracker.FoundOperator.AccountId, Role.Operator);

                if (idCards != null)
                {
                    var idCard = idCards.Where(x => x.active == "1").FirstOrDefault();

                    if (idCard != null)
                    {
                        WsdlRepository.UpdateIdCard(idCard.number, "0", false, null);

                        ChangeTracker.FoundOperator.ActiveCard = false;
                        OnPropertyChanged("EnabledBlockIdCardOperator");
                        ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_DONE) as string);
                    }
                }

            }
            catch (System.ServiceModel.FaultException<HubServiceException> exception)
            {
                switch (exception.Detail.code)
                {
                    case 169:
                        if (StationRepository.IsIdCardEnabled)
                            ShowError(TranslationProvider.Translate(MultistringTags.USER_DONT_HAVE_ACTIVE_CARD));
                        else
                            ShowError(TranslationProvider.Translate(MultistringTags.USER_DONT_HAVE_ACTIVE_BARCODECARD));
                        return;
                    default: // 113, 114, 172
                        ShowError(exception.Detail.message);
                        return;
                }
            }
        }

        [AsyncMethod]
        protected void OnBlockOperator()
        {
            BlockOperator();
        }

        [WsdlServiceSyncAspect]
        protected void BlockOperator()
        {
            try
            {
                WsdlRepository.UpdateOperator(ChangeTracker.FoundOperator.AccountId, new OperatorCriterias() { active = !ChangeTracker.FoundOperator.Active, activeSpecified = true });
                ChangeTracker.FoundOperator.Active = !ChangeTracker.FoundOperator.Active;
                OnPropertyChanged("EnabledEnableOperator");
                OnPropertyChanged("EnabledBlockOperator");

            }
            catch (System.ServiceModel.FaultException<HubServiceException> exception)
            {
                switch (exception.Detail.code)
                {
                    default: // 113, 114, 172
                        ShowError(exception.Detail.message);
                        return;
                }
            }
        }


        private void OnBackCommand()
        {
            Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
            MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
        }

        #endregion
    }
}