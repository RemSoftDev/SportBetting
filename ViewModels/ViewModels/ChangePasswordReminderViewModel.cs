using BaseObjects;
using System.Windows;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class ChangePasswordReminderViewModel : ChangePasswordViewModel
    {
        public bool isForced {get; set;}
        public ChangePasswordReminderViewModel(params object[] args)
        {
            CancelCommand = new BaseObjects.Command(OnCancelCommand);
            SelectionChanged = new Command<object>(OnSelectionChanged);
            this.ChangePasswordCommandForced = new Command(this.OnChangePasswordExecuteForced);

            if (args.Length > 0)
            {
                if (args[0] is bool)
                    isForced = (bool)args[0];
            }
        }
        public Command ChangePasswordCommandForced { get; private set; }
        public Command<object> SelectionChanged { get; private set; }

        [WsdlServiceSyncAspect]
        private void OnChangePasswordExecuteForced()
        {

            if (ValidateViewModel())
            {
                try
                {
                    if (IsLoggedInUser)
                    {
                        bool result = WsdlRepository.ChangePasswordFromTerminal(StationRepository.GetUid(ChangeTracker.CurrentUser), OldPassword.Value, NewPassword.Value);

                        if (result)
                        {
                            OldPassword.ValueMasked = "";
                            NewPassword.ValueMasked = "";
                            NewPasswordConfirmed.ValueMasked = "";
                            IsEnabledChange = false;

                            Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                            Mediator.SendMessage<long>(0, MsgTag.HideLogin);
                            Mediator.SendMessage(true, MsgTag.AskLoginAnonymous);
                            ShowMessage(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_DONE).ToString());
                        }
                    }
                    else
                    {
                        var result = WsdlRepository.ChangePasswordFromShop((int)ChangeTracker.CurrentUser.AccountId, StationRepository.GetUid(ChangeTracker.CurrentUser), NewPassword.Value);
                        if (result)
                        {
                            Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                            Mediator.SendMessage<long>(0, MsgTag.HideLogin);
                            Mediator.SendMessage(true, MsgTag.AskLoginAnonymous);
                            ShowMessage(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_DONE).ToString());
                        }
                    }
                }
                catch (System.ServiceModel.FaultException<HubServiceException> exception)
                {
                    switch (exception.Detail.code)
                    {
                        case 107:
                            ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALID_OLD_PASSWORD).ToString());
                            break;
                        case 1007:
                            ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_PASSWORD_WAS_ALREADY_USED).ToString());
                            break;
                        default:
                            ShowError(exception.Detail.message);
                            break;
                    }
                }
            }

        }

        public Visibility WarningTextVisibility
        {
            get 
            {
                if (isForced)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public void OnCancelCommand()
        {

            if (isForced)
            {
                ClearAndOpenAnonymousSession();
                Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                Mediator.SendMessage<long>(0, MsgTag.HideLogin);
                Mediator.SendMessage(true, MsgTag.AskLoginAnonymous);
            }
            else
            {
                Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                Mediator.SendMessage<long>(0, MsgTag.HideLogin);
                Mediator.SendMessage(true, MsgTag.AskLoginAnonymous);
            }
        }

        public Command CancelCommand { get; private set; }
    }    
}
