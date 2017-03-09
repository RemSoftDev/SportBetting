using System;
using PostSharp.Aspects;

namespace SportBetting.WPF.Prism.Modules.Aspects
{
    [Serializable]
    public class WsdlServiceSyncAspectSilent : WsdlServiceAsyncAspect
    {
        public override void OnEntry(MethodExecutionArgs args)
        {
            base.OnEntry(args);
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            base.OnExit(args);
        }
    }
}