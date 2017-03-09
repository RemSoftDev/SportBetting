using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.Util;
using IocContainer;
using Ninject;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using WsdlRepository;

namespace SportBetting.WPF.Prism.Shared.Models.Repositories
{
    [ServiceAspect]
    public class ActiveMQClient : IActiveMQClient
    {


        private int _syncInterval = Convert.ToInt32(ConfigurationManager.AppSettings["STATIONPROPERTY_SYNC_INTERVAL"]) * 1000;
        public void StartService()
        {
            ThreadHelper.RunThread("ActiveMQClient", Connect);
            ThreadHelper.RunThread("RegistrationFormService", RunRegistrationFormRefresh);
            ThreadHelper.RunThread("DevicesService", DevicesServiceRefresh);

        }
        private IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }

        private void DevicesServiceRefresh(ThreadContext tc)
        {
            while (!tc.IsToStop)
            {
                if (!StationRepository.CheckDevicesChange())
                    StationRepository.Refresh();

                if (StationRepository.SyncInterval > 0)
                {
                    _syncInterval = StationRepository.SyncInterval * 1000 * 3;
                    Thread.Sleep(_syncInterval);
                }
                else
                {
                    Thread.Sleep(3000);
                }
            }
        }

        private void RunRegistrationFormRefresh(ThreadContext tc)
        {
            while (!tc.IsToStop)
            {
                var result = StationRepository.GetRegistrationForm();

                if (StationRepository.SyncInterval > 0)
                    _syncInterval = StationRepository.SyncInterval * 1000 * 3;
                if (result != null)
                {
                    Thread.Sleep(_syncInterval);
                }
                else
                {
                    Thread.Sleep(3000);
                }
            }
        }

        private static ILog m_logger = LogFactory.CreateLog(typeof(ActiveMQClient));
        protected static AutoResetEvent semaphore = new AutoResetEvent(false);
        protected ITextMessage _message = null;
        protected static TimeSpan receiveTimeout = TimeSpan.FromSeconds(10);
        public delegate void DelegateMessageReceived(string message);
        public event DelegateMessageReceived MessageReceived = null;

        public void Connect(ThreadContext tc)
        {
            var url = ConfigurationManager.AppSettings["activemq_url"];
            Uri connecturi = new Uri(url);

            m_logger.Debug("About to connect to " + connecturi);

            // NOTE: ensure the nmsprovider-activemq.config file exists in the executable folder.

            var factory = new ConnectionFactory(connecturi);
            while (!tc.IsToStop)
            {
                try
                {

                    _reconnect = false;
                    using (IConnection connection = factory.CreateConnection())
                    using (ISession session = connection.CreateSession())
                    {


                        IDestination destination = SessionUtil.GetDestination(session, ConfigurationManager.AppSettings["activemq_topic_name"]);
                        connection.ConnectionInterruptedListener+=connection_ConnectionInterruptedListener;
                        m_logger.Debug("Using destination: " + destination);

                        // Create a consumer and producer
                        using (IMessageConsumer consumer = session.CreateConsumer(destination))
                        using (IMessageProducer producer = session.CreateProducer(destination))
                        {
                            // Start the connection so that messages will be processed.
                            connection.Start();
                            //producer.Persistent = true;
                            producer.RequestTimeout = receiveTimeout;
                            consumer.Listener += new MessageListener(OnMessage);
                            

                            while (!tc.IsToStop)
                            {
                                semaphore.WaitOne((int)receiveTimeout.TotalMilliseconds, true);
                                if(_reconnect)
                                    break;

                            }

                        }
                    }
                }
                catch (Exception e)
                {
                    m_logger.Error(e.Message, e);
                }
            }
        }

        private bool _reconnect = false;
        void connection_ConnectionInterruptedListener()
        {
            _reconnect = true;
        }
        protected void OnMessage(IMessage receivedMsg)
        {
            var message = receivedMsg as ITextMessage;
            semaphore.Set();
            if (message != null)
            {
                m_logger.Debug("Received message with ID:   " + message.NMSMessageId);
                m_logger.Debug("Received message with text: " + message.Text);

                //m_logger.Debug(message.Text);
                if (MessageReceived != null)
                {

                    MessageReceived(message.Text);
                }
            }
        }
    }

    public interface IActiveMQClient
    {
    }
}
