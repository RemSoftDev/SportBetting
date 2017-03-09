using System;
using System.Windows.Threading;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportRadar.Common.Logs;

namespace BaseObjects
{
    [ServiceAspect]
    public class MyDispatcher : IDispatcher
    {

        private static ILog Log = LogFactory.CreateLog(typeof(MyDispatcher));
        public MyDispatcher(Dispatcher dispatcher)
        {
            CurrentDispatcher = dispatcher;
        }

        public Dispatcher CurrentDispatcher;

        public void Invoke(Action action)
        {
            //Log.Debug(Environment.StackTrace);
            if (CurrentDispatcher.Thread.IsAlive)
                CurrentDispatcher.Invoke(action);
        }

        public void BeginInvoke(Action action)
        {
            CurrentDispatcher.BeginInvoke(action);
        }

        public bool CheckAccess()
        {
            return CurrentDispatcher.CheckAccess();
        }
    }
}