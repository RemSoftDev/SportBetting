using System;

namespace SharedInterfaces
{
    public interface IDispatcher
    {
        void Invoke(Action action);
        void BeginInvoke(Action action);
        bool CheckAccess();
    }
}