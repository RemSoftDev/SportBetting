using System;
using IocContainer;
using Ninject;
using PostSharp.Aspects;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects.WaitOverlayProvider;
using SportRadar.Common.Logs;
using WsdlRepository.WsdlServiceReference;


namespace SportBetting.WPF.Prism.Modules.Aspects
{
    [Serializable]
    public class WsdlServiceAsyncAspect : OnMethodBoundaryAspect
    {

        private static ILog Log = LogFactory.CreateLog(typeof(WsdlServiceAsyncAspect));

        protected IWaitOverlayProvider WaitOverlayProvider
        {
            get { return IoCContainer.Kernel.Get<IWaitOverlayProvider>(); }
        }

        //private static readonly ILog Log = LogManager.GetLogger(typeof(WsdlServiceAsyncAspect));

        /// <summary>
        /// Gets the mediator.
        /// </summary>
        public IMediator Mediator
        {
            get { return IoCContainer.Kernel.Get<IMediator>(); }
        }


        private bool haveError = false;
        public override void OnExit(MethodExecutionArgs args)
        {
            if (!haveError)
            {
                var lostConnection = new Tuple<string, string, bool, int>("GotInternetConnection", "", false, 0);
                Mediator.SendMessage(lostConnection, "Error");
            }
            haveError = false;

            base.OnExit(args);
        }

        public override void OnException(MethodExecutionArgs args)
        {
            WaitOverlayProvider.DisposeAll();
            haveError = true;
            bool bIsHandled = false;
            var lostConnection = new Tuple<string, string, bool, int>("LostInternetConnection", "", false, 0);
            if (args.Exception is System.ServiceModel.EndpointNotFoundException
                || args.Exception.GetType().ToString().Contains("ParseError")
                || args.Exception is System.ServiceModel.ServerTooBusyException
                || args.Exception is System.ServiceModel.ProtocolException
                || args.Exception is System.TimeoutException)
            {
                bIsHandled = true;
                Mediator.SendMessage(lostConnection, "Error");
            }
            else if (args.Exception is System.ServiceModel.FaultException)
            {
                var HubException = args.Exception as System.ServiceModel.FaultException<HubServiceException>;
                if (args.Exception.Message == "Station cannot be verified")
                {
                    var stationVerification = new Tuple<string, string>("StationVerificationFail", "");
                    Mediator.SendMessage(stationVerification, "Error");
                }
                else if (HubException != null)
                {
                    if (HubException.Detail.code == 121)
                    {
                        Mediator.SendMessage("SessionClosed", "SessionClosed");
                    }
                    else if (HubException.Detail.code == 119)
                    {
                        Mediator.SendMessage("UserBlocked", "UserBlocked");
                    }
                }
                else
                {
                    Mediator.SendMessage(lostConnection, "Error");
                }
            }

            Log.Error(args.Exception.Message, args.Exception);

            args.FlowBehavior = FlowBehavior.Continue;


        }
    }
}