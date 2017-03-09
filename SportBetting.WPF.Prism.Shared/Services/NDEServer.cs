using System;
using System.Collections.Generic;
using System.Timers;
using DdeNet.Server;
using IocContainer;
using Ninject;
using SharedInterfaces;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;

namespace SportBetting.WPF.Prism.Shared.Services
{
    public class NDEServer : DdeServer, INDEServer
    {

        private static IMediator Mediator
        {
            get { return IoCContainer.Kernel.Get<IMediator>(); }
        }
        private static ILog Log = LogFactory.CreateLog(typeof(NDEServer));
        private System.Timers.Timer _Timer = new System.Timers.Timer();
        private IList<string> items = new List<string>();


        public NDEServer(string service)
            : base(service)
        {
            _Timer.Elapsed += OnTimerElapsed;
            _Timer.Interval = 1000;
            _Timer.Start();

        }


        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            itemToSend = "*";
            if (valuesTosend.Count > 0)
                itemToSend = valuesTosend.Dequeue();

            foreach (var item in items)
            {
                try
                {
                    Advise("*", item);

                }
                catch (Exception)
                {
                }
            }
        }


        protected override ExecuteResult OnExecute(DdeConversation conversation, string command)
        {
            Log.Debug("OnExecute:".PadRight(16)
                + " Service='" + conversation.Service + "'"
                + " Topic='" + conversation.Topic + "'"
                + " Handle=" + conversation.Handle.ToString()
                + " Command='" + command + "'");
            Mediator.SendMessage("", command);

            // Tell the client that the command was processed.
            return ExecuteResult.Processed;
        }

        protected override bool OnBeforeConnect(string topic)
        {
            Log.Debug("OnBeforeConnect:".PadRight(16)
                + " Service='" + base.Service + "'"
                + " Topic='" + topic + "'");

            return true;
        }


        protected override bool OnStartAdvise(DdeConversation conversation, string item, int format)
        {
            Log.Debug("OnStartAdvise:".PadRight(16)
                + " Service='" + conversation.Service + "'"
                + " Topic='" + conversation.Topic + "'"
                + " Handle=" + conversation.Handle.ToString()
                + " Item='" + item + "'"
                + " Format=" + format.ToString());

            // Initiate the advisory loop only if the format is CF_TEXT.
            items.Add(item);
            return format == 1;
        }

        private SyncQueue<string> valuesTosend = new SyncQueue<string>();
        public void AdviseALL(string values)
        {
            valuesTosend.Enqueue(values);
        }

        private string itemToSend = "*";

        protected override byte[] OnAdvise(string topic, string item, int format)
        {


            // Send data to the client only if the format is CF_TEXT.
            //Log.Debug("OnAdvise:".PadRight(16) + " Service='" + this.Service + "'" + " Topic='" + topic + "'" + " Item='" + item + "'" + " Format=" + format.ToString() + " Values=" + itemToSend);

            return System.Text.Encoding.UTF8.GetBytes(itemToSend);
        }

        public int NumberOfClients()
        {
            return items.Count;
        }
    }

    public interface INDEServer
    {
        void Register();
        void Unregister();
        void AdviseALL(string message);
        int NumberOfClients();
    }

// class

    // class
}
