using System;
using PostSharp.Aspects;
//using Catel.Logging;

namespace SportBetting.WPF.Prism.Modules.Aspects
{
    [Serializable]
    public class WsdlServiceSyncAspect : WsdlServiceAsyncAspect
    {

        public override void OnEntry(MethodExecutionArgs args)
        {
            this.WaitOverlayProvider.ShowWaitOverlay();
            base.OnEntry(args);
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            this.WaitOverlayProvider.DisposeAll();

            base.OnExit(args);
        }
        public override void OnException(MethodExecutionArgs args)
        {
            base.OnException(args);
            this.WaitOverlayProvider.DisposeAll();
        }
    }

}
    
