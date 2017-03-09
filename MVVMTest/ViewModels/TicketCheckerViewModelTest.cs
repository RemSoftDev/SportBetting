using System.Collections.Generic;
using BaseObjects.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Shared;
using ViewModels.ViewModels;

namespace MVVMTest
{
    [TestClass]
    public class TicketCheckerViewModelTest : BaseTestClass
    {
        [TestMethod]
        public void OnPinButtonMethodTestWithNullValue()
        {
            IocContainer.IoCContainer.Kernel.Unbind<IMediator>();
            IocContainer.IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();

            ChangeTracker.Setup(x => x.LoadedTicketcheckSum).Returns("");
            ChangeTracker.Setup(x => x.LoadedTicket).Returns("");
            var mediator = (MyMessageMediator)IocContainer.IoCContainer.Kernel.Get<IMediator>();

            var model = new TicketCheckerViewModel();
            model.OnNavigationCompleted();

            string testString = null;
            model.OnPinButtonTest(testString);
        }

        [TestMethod]
        public void TestMessageRegistration()
        {
            IocContainer.IoCContainer.Kernel.Unbind<IMediator>();
            IocContainer.IoCContainer.Kernel.Bind<IMediator>().To<MyMessageMediator>().InSingletonScope();

            ChangeTracker.Setup(x => x.LoadedTicketcheckSum).Returns("");
            ChangeTracker.Setup(x => x.LoadedTicket).Returns("");
            var mediator = (MyMessageMediator)IocContainer.IoCContainer.Kernel.Get<IMediator>();

            var model = new TicketCheckerViewModel();
            model.OnNavigationCompleted();
            var count = 0;
            foreach (var handler in mediator._registeredHandlers)
            {
                foreach (var actionDetailse in handler.Value)
                {
                    count++;
                }
            }
            Assert.AreEqual(4, count);

            model.Close();


            count = 0;
            foreach (var handler in mediator._registeredHandlers)
            {
                foreach (var actionDetailse in handler.Value)
                {
                    count++;
                }
            }

            Assert.AreEqual(0, count);

            model = new TicketCheckerViewModel();
            model.OnNavigationCompleted();

            count = 0;
            foreach (var handler in mediator._registeredHandlers)
            {
                foreach (var actionDetailse in handler.Value)
                {
                    count++;
                }
            }

            Assert.AreEqual(4,count);
        }

    }
}