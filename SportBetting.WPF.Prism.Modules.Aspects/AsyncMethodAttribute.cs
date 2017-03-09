using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using PostSharp.Aspects;
using SportRadar.DAL.Connection;

namespace SportBetting.WPF.Prism.Modules.Aspects
{
    [Serializable]
    public sealed class AsyncMethodAttribute : MethodInterceptionAspect
    {
        public override void OnInvoke(MethodInterceptionArgs eventArgs)
        {
            if (ConfigurationManager.AppSettings["disable_Async"] != null)
            {
                eventArgs.Proceed();
            }
            else
            {
                new Thread(() =>
                    {
                        eventArgs.Proceed();
                        ConnectionManager.CloseConnection();
                    }).Start();
            }
        }
    }
}