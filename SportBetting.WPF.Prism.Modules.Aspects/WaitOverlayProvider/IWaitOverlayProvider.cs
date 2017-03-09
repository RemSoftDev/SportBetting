using System;

namespace SportBetting.WPF.Prism.Modules.Aspects.WaitOverlayProvider
{
    public interface IWaitOverlayProvider
    {
        void ShowWaitOverlay();
        void ShowWaitOverlay(bool isTicketPrinting);
        void DisposeAll();
    }
}
