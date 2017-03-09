using System;
using Shared;

namespace SharedInterfaces
{
    public interface IIdCardRepository
    {
        void StartService();
        bool WriteCard(string cardNumber);
        event EventHandler<EventArgs<string>> InsertCard;
        event EventHandler EjectCard;
        event EventHandler CardError;
    }
}