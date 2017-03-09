using System;
using SportRadar.Common;
using SportRadar.Common.Enums;

namespace SharedInterfaces
{
    public interface IErrorWindowService
    {
        void ShowError(string obj, EventHandler okClick = null, bool bCreateButtonEvent = false, int iAddCounterSeconds = 0, ErrorLevel errorLevel = ErrorLevel.Normal);
        void Close();
        void ShowError(string obj, ErrorSettings errorSettings);
    }
}
