using System.Security.Cryptography.X509Certificates;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportRadar.Common.Windows;
using SportRadar.DAL.Connection;
using TranslationByMarkupExtension;
using System;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class RestartViewModel : BaseViewModel
    {

        #region Constructors

        public RestartViewModel()
        {
           
            SoftRestartCommand = new Command(OnSoftRestartCommand);
            HardRestartCommand = new Command(OnHardRestartCommand);
            ShutdownCommand = new Command(OnShutdownCommand);
            ResetDbShutdownCommand = new Command(OnResetDB);
            //Mediator.Register<string>(this, ShowSystemInfo, MsgTag.ShowSystemInfo);
        }



        #endregion


        #region Commands

     
        public Command SoftRestartCommand { get; set; }
        public Command ResetDbShutdownCommand { get; set; }
        public Command HardRestartCommand { get; set; }
        public Command ShutdownCommand { get; set; }
        #endregion


        #region Methods

        public override void OnNavigationCompleted()
        {
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_PRINT_SYSTEM;
            ChangeTracker.AdminTitle2 = MultistringTags.TERMINAL_FORM_RESTART_TERMINAL;
            ChangeTracker.RestartChecked = true;

            base.OnNavigationCompleted();
        }


        private void OnSoftRestartCommand()
        {
            QuestionWindowService.ShowMessage(TranslationProvider.Translate(MultistringTags.TERMINAL_SOFT_RESTART_QUESTION), null, null, SoftRestartYes, null);
        }

        private void OnHardRestartCommand()
        {
            QuestionWindowService.ShowMessage(TranslationProvider.Translate(MultistringTags.TERMINAL_HARD_RESTART_QUESTION), null, null, HardRestartYes, null);
        }
        private void OnShutdownCommand()
        {
            QuestionWindowService.ShowMessage(TranslationProvider.Translate(MultistringTags.TERMINAL_SHUTDOWN_QUESTION), null, null, ShutdownYes, null);
        }

        private void ShutdownYes(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("shutdown.exe", "-s -t 0");

        }

        private void SoftRestartYes(object sender, EventArgs e)
        {
            Mediator.SendMessage<long>(0, MsgTag.RestartApplication);

        }

        private void HardRestartYes(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");

        }

        private void ResetDBYes(object sender, EventArgs e)
        {
            OnResetDBPleaseWait();

        }

        [AsyncMethod]
        private void OnResetDB()
        {
            QuestionWindowService.ShowMessage(TranslationProvider.Translate(MultistringTags.TERMINAL_RESET_DB_QUESTION), null, null, ResetDBYes, null);
        }

        [PleaseWaitAspect]
        private void OnResetDBPleaseWait()
        {
            ThreadHelper.StopAll();
            var serviceRuntimeUserCertificateStore = new X509Store(StoreName.My);
            serviceRuntimeUserCertificateStore.Open(OpenFlags.ReadWrite);
            for (int i = 0; i < serviceRuntimeUserCertificateStore.Certificates.Count;)
            {
                var certificate = serviceRuntimeUserCertificateStore.Certificates[i];
                if (certificate.Subject.Contains("Sportradar AG"))
                {
                    serviceRuntimeUserCertificateStore.Remove(certificate);
                    continue;
                }
                i++;
            }
            PrinterHandler.DeleteAllPrinterObjects();
            ConnectionManager.CloseAll();
            DatabaseManager.DropDatabase(false);
            System.Diagnostics.Process.Start("shutdown.exe", "-s -t 0");

        }

        #endregion

    }
}
