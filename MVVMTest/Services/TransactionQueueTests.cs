using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using IocContainer;
using MainWpfWindow.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SportBetting.WPF.Prism.Shared;
using SportRadar.DAL;
using SportRadar.DAL.Connection;
using SportRadar.DAL.NewLineObjects;
using ViewModels.ViewModels;
using WcfService;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;

namespace MVVMTest.Services
{
    [TestClass]
    public class TransactionQueueTests : BaseTestClass
    {



        [TestMethod]
        [DeploymentItem("DatabaseResources\\PgSrbsClient.config")]
        public void LoadFromTransactionQueueAndSaveIncorectTicketsTest()
        {

            ConfigurationManager.AppSettings["CreateDatabase"] = "1";
            StationSettingsUtils.m_sStartupPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(DatabaseTests)).Location);
            DatabaseManager.EnsureDatabase(false);
            DatabaseCache.EnsureDatabaseCache();
            SportRadar.DAL.NewLineObjects.LineSr.EnsureFromCache();

            var uid = new uid() { account_id = "1", session_id = "1", station_id = "123" };
            var ticket = new TicketWS();



            long[] tiplock = new long[1];
            long[] tournamentLock = new long[1];
            WsdlRepository.Setup(x => x.SaveTicket("0123", uid, ticket, false, "1234", out tiplock, out tournamentLock)).Throws(new Exception("test"));

            StationRepository.Setup(x => x.StationNumber).Returns("1234");

            MessageMediator.Setup(x => x.SendMessage(It.IsAny<object>(), It.IsAny<string>())).Returns(true);
            var TransactionQueueHelper = new TransactionQueueHelper();

            TransactionQueueHelper.TrySaveTicketOnHub("0123", uid, ticket, "1234", false);


            int count = TransactionQueueHelper.GetCountTransactionQueue();
            Assert.IsTrue(count > 0);


            WsdlRepository.Setup(x => x.SaveTicket(It.IsAny<string>(), It.IsAny<uid>(), It.IsAny<TicketWS>(), It.IsAny<bool>(), It.IsAny<string>(), out tiplock, out tournamentLock)).Throws(new FaultException<HubServiceException>(new HubServiceException()));




            TransactionQueueHelper.TryToSaveTransactionObjectsOnHub();


            int count2 = TransactionQueueHelper.GetCountTransactionQueue();
            Assert.AreEqual(0, count2);


        }
		[TestMethod]
        [DeploymentItem("DatabaseResources\\PgSrbsClient.config")]
        public void NoConnectionTest()
        {

            ConfigurationManager.AppSettings["CreateDatabase"] = "1";
            StationSettingsUtils.m_sStartupPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(DatabaseTests)).Location);
            DatabaseManager.EnsureDatabase(false);
            DatabaseCache.EnsureDatabaseCache();
            SportRadar.DAL.NewLineObjects.LineSr.EnsureFromCache();
            TestWsdlRepository repository = new TestWsdlRepository();
            IoCContainer.Kernel.Rebind<IWsdlRepository>().ToConstant(repository).InSingletonScope();
            repository.ConnectionProblem = true;
            var TransactionQueueHelper = new TransactionQueueHelper();
            IoCContainer.Kernel.Rebind<ITransactionQueueHelper>().ToConstant(TransactionQueueHelper).InSingletonScope();
            var uid = new uid() { account_id = "1", session_id = "1", station_id = "123" };
            var ticket = new TicketWS();
            TransactionQueueHelper.TrySaveTicketOnHub("0123", uid, ticket, "1234", false);

            StationRepository.Setup(x => x.StationNumber).Returns("1234");

            MessageMediator.Setup(x => x.SendMessage(It.IsAny<object>(), "Error")).Returns(true);
            MainViewModel footerViewModel = new MainViewModel();
            int count = TransactionQueueHelper.GetCountTransactionQueue();
            Assert.IsTrue(count > 0);
            repository.ConnectionProblem = false;
            footerViewModel.TryToSaveTransactionObjectsOnHub();


            //TransactionQueueHelper.TryToSaveTransactionObjectsOnHub();

            count = TransactionQueueHelper.GetCountTransactionQueue();
            Assert.AreEqual(0, count);
            //MessageMediator.Verify(x => x.SendMessage(It.IsAny<object>(), "Error"));;



        }


        [TestMethod]
        [DeploymentItem("DatabaseResources\\PgSrbsClient.config")]
        public void TicketSavePassedTest()
        {

            StationSettingsUtils.m_sStartupPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(DatabaseTests)).Location);
            DatabaseManager.EnsureDatabase(false);
            DatabaseCache.EnsureDatabaseCache();
            SportRadar.DAL.NewLineObjects.LineSr.EnsureFromCache();

            var uid = new uid() { account_id = "1", session_id = "1", station_id = "123" };
            var ticket = new TicketWS();


            var TransactionQueueHelper = new TransactionQueueHelper();
            int countBeforeSaving = TransactionQueueHelper.GetCountTransactionQueue();

            long[] tiplock = new long[1];
            long[] tournamentLock = new long[1];
            WsdlRepository.Setup(x => x.SaveTicket("0123", uid, ticket, false, "1234", out tiplock, out tournamentLock)).Throws(new System.ServiceModel.FaultException<HubServiceException>(new HubServiceException()));

            StationRepository.Setup(x => x.StationNumber).Returns("1234");

            MessageMediator.Setup(x => x.SendMessage(It.IsAny<object>(), It.IsAny<string>())).Returns(true);

            try
            {
                TransactionQueueHelper.TrySaveTicketOnHub("0123", uid, ticket, "1234", false);

            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.GetType() == typeof(FaultException<HubServiceException>));
            }


            int count = TransactionQueueHelper.GetCountTransactionQueue();
            Assert.AreEqual(countBeforeSaving, count);


        }

    }
}
