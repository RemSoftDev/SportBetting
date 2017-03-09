using System;
using System.Windows;
using BaseObjects.ViewModels;

namespace BaseObjects
{
    public interface IMyRegionManager
    {
        Type CurrentViewModelType(string contentRegion);
        Type PreviousViewModelType(string regionName);
        IBaseViewModel CurrentViewModelInRegion(string contentRegion);
        void ClearHistory(string contentRegion);
        T NavigateUsingViewModel<T>(string contentRegion, params object[] args);
        Window FindWindowByViewModel<T>(bool init = true);
        object NavigatBack(string contentRegion);
        void NavigateForvard(string usermanagementContentRegion);
        void CloseAllViewsInRegion(string regionName);
        void ClearForwardHistory(string regionName);
    }
}