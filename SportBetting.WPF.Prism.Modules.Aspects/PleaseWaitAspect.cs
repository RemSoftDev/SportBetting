using System;
using IocContainer;
using Ninject;
using PostSharp.Aspects;
using SportBetting.WPF.Prism.Modules.Aspects.WaitOverlayProvider;
using SportRadar.Common.Logs;


namespace SportBetting.WPF.Prism.Modules.Aspects
{
    [Serializable]
    public class PleaseWaitAspect : OnMethodBoundaryAspect
    {
        private static ILog Log = LogFactory.CreateLog(typeof(PleaseWaitAspect));
        protected IWaitOverlayProvider WaitOverlayProvider
        {
            get { return IoCContainer.Kernel.Get<IWaitOverlayProvider>(); }
        }

        public override void OnException(MethodExecutionArgs args)
        {
            Log.Error("", args.Exception);
            args.FlowBehavior = FlowBehavior.Continue;
            this.WaitOverlayProvider.DisposeAll();
        }
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

    }
}