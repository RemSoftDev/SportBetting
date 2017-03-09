using System;
using PostSharp.Aspects;
using SportRadar.Common.Logs;
using WsdlRepository.WsdlServiceReference;

namespace SportBetting.WPF.Prism.Modules.Aspects
{
    [Serializable]
    public class ServiceAspect : OnMethodBoundaryAspect
    {
        private static readonly ILog Log = LogFactory.CreateLog(typeof(ServiceAspect));

        public override void OnException(MethodExecutionArgs args)
        {
            Log.Error(args.Exception.Message, args.Exception);

            if (args.Exception is System.ServiceModel.FaultException)
            {
                throw args.Exception;
            }

            //args.FlowBehavior = FlowBehavior.Continue;

            //base.OnException(args);
        }
    }
}
