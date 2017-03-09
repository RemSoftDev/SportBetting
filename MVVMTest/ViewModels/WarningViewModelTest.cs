using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using BaseObjects.ViewModels;
using IocContainer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Shared.Models.Repositories;
using WsdlRepository;

namespace MVVMTest
{
    [TestClass]
    public class WarningViewModelTest : BaseTestClass
    {
       
        [TestMethod]
        public void StopTimer()
        {
            IoCContainer.Kernel.Unbind<IStationRepository>();
            IoCContainer.Kernel.Bind<IStationRepository>().To<StationRepository>().InSingletonScope();
            var model = new WarningViewModel();
            //Dispatcher.InvokeShutdown();
            Assert.IsTrue(model._timer.IsEnabled);
            //model.MouseDownCommand.Execute();
            //Assert.IsTrue(!model._timer.IsEnabled);


        }
        [TestMethod]
        public void UpdateCounter()
        {

            IoCContainer.Kernel.Unbind<IStationRepository>();
            IoCContainer.Kernel.Bind<IStationRepository>().To<StationRepository>().InSingletonScope();
            IoCContainer.Kernel.Get<IStationRepository>().AutoLogoutWindowLiveTimeInSec = 5;

            var model = new WarningViewModel();
            var firedEvents = new List<string>();
            model.PropertyChanged += ((sender, e) => firedEvents.Add(e.PropertyName));

            model.UpdateCounter(null,null);
            Assert.IsTrue(firedEvents.Contains("Counter"));

        }

    }
}