using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class NoContentViewModel : BaseViewModel
    {
        public NoContentViewModel()
        {
            Mediator.Register<string>(this, HeaderShowFirstView, MsgTag.RefreshStation);
        }

        private void HeaderShowFirstView(string obj)
        {
            Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);
        }
    }
}
