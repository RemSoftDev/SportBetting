using System;
using System.ComponentModel;
using System.Windows;
using SharedInterfaces;
using UserControl = System.Windows.Controls.UserControl;

namespace BaseObjects.ViewModels
{
    public interface IBaseViewModel :  IClosable
    {
        void Close();
        void OnNavigationCompleted();
        UserControl View { get; set; }
        bool IsReady { get; }
        Window ViewWindow { get; set; }
        void ShowMessage(string msg1);
        void ShowError(string obj, EventHandler okClick = null, bool bCreateButtonEvent = false, int iAddCounterSeconds = 0);
    }
}
