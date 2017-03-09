using System;
using System.Threading;
using BaseObjects.ViewModels;
using IocContainer;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using WsdlRepository;


namespace SportBetting.WPF.Prism.Modules.Services.Implementation
{

    [ServiceAspect]
    public class IdCardService : IClosable
    {
        private static MessageStorage CatelMediator { get; set; }


        private static IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }
        public string CardNumber
        {
            get { return IoCContainer.Kernel.Get<IChangeTracker>().CardNumber; }
            set { IoCContainer.Kernel.Get<IChangeTracker>().CardNumber = value; }
        }

        private object _locker = new object();
        //TODO conf parameter

        #region IService Members

        private static SmartCardReader.SmartCardManager scm = SmartCardReader.SmartCardManager.GetManager ();

        public IdCardService (bool TestIdReader)
        {
            CatelMediator = new MessageStorage();
            if (!TestIdReader)
            {
                if (!StationRepository.IsIdCardEnabled)
                    return;
            }

            //ID cards reader initialization
            CatelMediator.Register<string>(this, WriteCardNumber, MsgTag.WriteNumber);
            CatelMediator.ApplyRegistration();


            scm.SmartCardInsertEventHandler += new EventHandler<SmartCardReader.CardEventArgs<string>>(daemonThread_InsertCard);
            scm.SmartCardRemoveEventHandler += new EventHandler(daemonThread_EjectCard);
            scm.SmartCardErrorEventHandler += new EventHandler<SmartCardReader.CardEventArgs<string>>(daemonThread_CardError);

            if (!scm.SmartCardReaderDetected)
            {
                CatelMediator.SendMessage<long>(0, MsgTag.IdCardError);
            }
        }

        [AsyncMethod]
        void daemonThread_StartedCardReading(object sender, EventArgs e)
        {
            CatelMediator.SendMessage<string>("", MsgTag.StartedCardReading);
        }

        [AsyncMethod]
        void daemonThread_CardError(object sender, SmartCardReader.CardEventArgs<string> e)
        {
          //  daemonThread_EjectCard(sender, e);
            if (!writing)
                CatelMediator.SendMessage<long>(0, MsgTag.IdCardError);
        }

        [AsyncMethod]
        void daemonThread_EjectCard(object sender, EventArgs e)
        {
            CatelMediator.SendMessage<long>(0, MsgTag.CardRemoved);
        }

        [AsyncMethod]
        void daemonThread_InsertCard(object sender, SmartCardReader.CardEventArgs<string> e)
        {
            lock (_locker)
            {
                CardNumber = e.Value;
                Monitor.PulseAll(_locker);
                if (!writing)
                {
                    CatelMediator.SendMessage(true, MsgTag.ClosePinWindow);
                    CatelMediator.SendMessage<string>(e.Value, MsgTag.CardInserted);
                }
            }

        }

        private bool writing = false;
        public void WriteCardNumber(string number)
        {
            writing = true;
            lock (_locker)
            {
                //daemonThread.WriteDataToCard(number);
                scm.WrireData(number);
                Monitor.Wait(_locker, 10000);

            }
            writing = false;
        }


        //TODO Thinking about common IService interface
        public void GetData(string lang) { }
        #endregion

        public bool IsClosed { get; set; }
    }
}
