using System;

namespace SharedInterfaces
{
    public interface IQuestionWindowService
    {
        void ShowMessage(string text, string yesButtonText, string noButtonText, EventHandler yesClick,
            EventHandler noClick, bool IsVisibleNoButton = true, int yesButtonTimer = 0, bool clearCashToTransfer = false, bool warning = false);

        void ShowMessageSync(string text, string yesButtonText, string noButtonText, EventHandler yesClick, EventHandler noClick, bool IsVisibleNoButton = true, int yesbuttonTimer = 0, bool clearCashToTransfer = false, bool warning = false);
        void ShowMessage(string text);
        void Close();
    }
}
