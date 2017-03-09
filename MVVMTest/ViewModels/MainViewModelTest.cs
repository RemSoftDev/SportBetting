using BaseObjects.ViewModels;
using IocContainer;
using MainWpfWindow.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharedInterfaces;
using SportBetting.WPF.Prism.Shared;
using TranslationByMarkupExtension;
using ViewModels.ViewModels;

namespace MVVMTest
{
    [TestClass]
    public class MainViewModelTest : BaseTestClass
    {

        [TestMethod]
        public void AskCAshpoolDisabledAnonymousBetting()
        {
            IoCContainer.Kernel.Unbind<IMediator>();
            var MessageMediator = new MyMessageMediator();
            IoCContainer.Kernel.Bind<IMediator>().ToConstant<IMediator>(MessageMediator).InSingletonScope();
            TranslationProvider.Setup(x => x.Translate(MultistringTags.TERMINAL_CREDITNOTE_MOVECREDITTOBALANCE_LOGIN)).Returns("move {0}");
            QuestionWindowService.Setup(x => x.ShowMessage(It.IsAny<string>()));
            var model = new MainViewModel();
            MessageMediator.SendMessage<decimal>(20m, MsgTag.AskAboutCashPool);

            QuestionWindowService.Verify(x => x.ShowMessage(It.IsAny<string>()), Times.Never);


        }


        [TestMethod]
        public void RefreshPropertyEventByNumber()
        {

            var model = new MainViewModel();
            StationRepository.Setup(x => x.Refresh());
            StationRepository.Setup(x => x.StationNumber).Returns("0100");
            model.stationPropertyService_MessageReceived("{\"station_number\": [\"0191\",\"0100\"], \"location_id\": [2], \"franchiser_id\": [1], \"event_name\": \"station_properties\"}");
            StationRepository.Verify(x => x.Refresh(), Times.Exactly(1));
            StationRepository.Verify(x => x.StationNumber, Times.Exactly(2));

        }        
        [TestMethod]
        public void RefreshPropertyEventByLocation()
        {

            var model = new MainViewModel();
            StationRepository.Setup(x => x.Refresh());
            StationRepository.Setup(x => x.StationNumber).Returns("0101");
            StationRepository.Setup(x => x.LocationID).Returns(2);
            model.stationPropertyService_MessageReceived("{\"station_number\": [\"0100\"], \"location_id\": [2], \"franchiser_id\": [1], \"event_name\": \"station_properties\"}");
            StationRepository.Verify(x => x.Refresh(), Times.Exactly(1));

        }        
        [TestMethod]
        public void RefreshPropertyEventByFranchizor()
        {

            var model = new MainViewModel();
            StationRepository.Setup(x => x.Refresh());
            StationRepository.Setup(x => x.StationNumber).Returns("0101");
            StationRepository.Setup(x => x.LocationID).Returns(2);
            StationRepository.Setup(x => x.FranchisorID).Returns(2);
            model.stationPropertyService_MessageReceived("{\"station_number\": [\"0100\"], \"location_id\": [3], \"franchisor_id\": [2], \"event_name\": \"station_properties\"}");
            StationRepository.Verify(x => x.Refresh(), Times.Exactly(1));

        }       
        [TestMethod]
        public void NotRefreshPropertyEventByLocation()
        {

            var model = new MainViewModel();
            StationRepository.Setup(x => x.Refresh());
            StationRepository.Setup(x => x.StationNumber).Returns("0101");
            StationRepository.Setup(x => x.LocationID).Returns(3);
            model.stationPropertyService_MessageReceived("{\"station_number\": [\"0100\"], \"location_id\": [2], \"franchiser_id\": [1], \"event_name\": \"station_properties\"}");
            StationRepository.Verify(x => x.Refresh(), Times.Never);

        }
        [TestMethod]
        public void NotRefreshPropertyEventByEvent()
        {

            var model = new MainViewModel();
            StationRepository.Setup(x => x.Refresh());
            StationRepository.Setup(x => x.StationNumber).Returns("0101");
            StationRepository.Setup(x => x.LocationID).Returns(3);
            model.stationPropertyService_MessageReceived("{\"station_number\": [\"0101\"], \"location_id\": [2], \"franchiser_id\": [1], \"event_name\": \"flags\"}");
            StationRepository.Verify(x => x.Refresh(), Times.Never);

        }        
        [TestMethod]
        public void invalidJson()
        {

            var model = new MainViewModel();
            StationRepository.Setup(x => x.Refresh());
            StationRepository.Setup(x => x.StationNumber).Returns("0101");
            StationRepository.Setup(x => x.LocationID).Returns(3);
            model.stationPropertyService_MessageReceived("");
            StationRepository.Verify(x => x.Refresh(), Times.Never);

        }

    }
}