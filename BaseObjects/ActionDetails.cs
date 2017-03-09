using System;
using SharedInterfaces;

namespace BaseObjects.ViewModels
{
    internal class ActionDetails<T> : IActionDetails
    {
        public IClosable ViewModel { get; set; }

        public Action<T> Action { get; set; }

        public string MsgTag { get; set; }

        public Type Type { get; set; }

        public string MethodName { get { return Action.Method.Name; } }

        public bool Execute(object value)
        {
            Action.Invoke((T)value);
            return true;
        }

    }
}