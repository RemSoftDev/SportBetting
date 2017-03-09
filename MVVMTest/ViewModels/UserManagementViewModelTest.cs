using System;
using IocContainer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.DAL.Connection;
using TranslationByMarkupExtension;
using ViewModels.ViewModels;

namespace MVVMTest
{
    [TestClass]
    public class UserManagementViewModelTest : BaseTestClass
    {

        [TestMethod]
        [DeploymentItem("DatabaseResources\\PgSrbsClient.config")]
        public void CashHistoryUpdateOnEmptyBox()
        {

            DatabaseManager.EnsureDatabase(false);

            var changeTracker = new ChangeTracker();
            IoCContainer.Kernel.Rebind<IChangeTracker>().ToConstant<IChangeTracker>(changeTracker).InSingletonScope();
            var ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
            ChangeTracker.CurrentUser = new AnonymousUser("", 1);

            IoCContainer.Kernel.Rebind<IQuestionWindowService>().To<QuestionYesMock>().InSingletonScope();

            decimal locationCashPosition, totalStationCash, totalLocationCash, totalLocationPaymentBalance, cashoutCurrentAmount = 0, _cashinCurrentAmount = 0;

            WsdlRepository.Setup(x => x.GetCashInfo(StationRepository.Object.StationNumber, out totalStationCash,
                                                                           out locationCashPosition,
                                                                           out totalLocationCash, out totalLocationPaymentBalance)).Returns(1);
            decimal money = 0;
            DateTime last;

            DateTime start;
            WsdlRepository.Setup(x => x.CashOut(null, null, out last,It.IsAny<string>(),out start)).Returns(300);

            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_COLLECT_CASH)).Returns("{0}");

            StationRepository.Setup(x => x.PrinterStatus).Returns(1);
            var vm = new UserManagementViewModel();

            Assert.AreEqual(1, ChangeTracker.TerminalBalance);

            WsdlRepository.Setup(x => x.GetCashInfo(StationRepository.Object.StationNumber, out totalStationCash,
                                                                           out locationCashPosition, out totalLocationPaymentBalance,
                                                                           out totalLocationCash)).Returns(0);
            WsdlRepository.Setup(x => x.CashOut(null, null, out last, It.IsAny<string>(), out start)).Returns(300);
            vm.CloseBalance.Execute(null);
            vm.askWindow_YesClick(null, null);

            WsdlRepository.Verify(x => x.GetCashInfo(StationRepository.Object.StationNumber, out totalStationCash,
                                                               out locationCashPosition, out totalLocationPaymentBalance,
                                                               out totalLocationCash), Times.Exactly(2));


            Assert.AreEqual(0, ChangeTracker.TerminalBalance);
            DatabaseManager.DropDatabase(false);

        }

    }
}