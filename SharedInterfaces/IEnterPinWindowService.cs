using System;
using Shared;

namespace SharedInterfaces
{
    public interface IEnterPinWindowService
    {

        void Close();

        void AskPin(EventHandler<EventArgs<string>> enterpinviewModelOkClick1, EventHandler<EventArgs<string>> viewModelOkClick = null);
    }
}