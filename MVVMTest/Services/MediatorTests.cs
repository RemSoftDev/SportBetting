using BaseObjects.ViewModels;
using IocContainer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Shared;

namespace MVVMTest.Services
{
    [TestClass]
    public class MediatorTests
    {
        [TestMethod]
        public void MediatorTest()
        {



            MyMessageMediator mediator = new MyMessageMediator();

            IoCContainer.Kernel = new StandardKernel();
            IoCContainer.Kernel.Bind<IMediator>().ToConstant<IMediator>(mediator).InSingletonScope();

            var testClass = new TestClass();

            var storage = IoCContainer.Kernel.Get<IMediator>();

            storage.SendMessage<long>(1, MsgTag.CloseLogin);

            Assert.AreEqual("1", testClass.testValue);
            storage.SendMessage<string>("2", MsgTag.CloseLogin);

            Assert.AreEqual("12", testClass.testValue);
            Assert.AreEqual(2, ((MessageStorage)testClass.Mediator).actions.Count);


            testClass.Close();
            TestClass testClass2 = new TestClass();

            storage.SendMessage<long>(10, MsgTag.CloseLogin);
            storage.SendMessage<string>("10", MsgTag.CloseLogin);

            Assert.AreEqual("1010", testClass2.testValue);
            Assert.AreEqual("12", testClass.testValue);
            Assert.AreEqual(2, ((MessageStorage)testClass2.Mediator).actions.Count);


        }


    }
    public class TestClass : IClosable
    {
        public string testValue;

        private MessageStorage _mediator;
        public MessageStorage Mediator { get; set; }


        public TestClass()
        {
            Mediator = new MessageStorage();
            Mediator.Register<long>(this, Test, MsgTag.CloseLogin);
            Mediator.Register<string>(this, Test, MsgTag.CloseLogin);
            Mediator.Register<string>(this, Test, MsgTag.CloseLogin);
            Mediator.ApplyRegistration();

        }

        public void Close()
        {
            Mediator.UnregisterRecipientAndIgnoreTags(this);
            IsClosed = true;
        }

        private void Test(string obj)
        {
            testValue += obj;
        }

        private void Test(long obj)
        {
            testValue += obj.ToString();

        }

        public bool IsClosed { get; set; }
    }


}
