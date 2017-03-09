using System;
using System.Windows.Input;

namespace BaseObjects
{
    public class Command : ICommand
    {
        /// <summary>
        /// Create a command that can always be executed
        /// </summary>
        /// <param name="executeMethod">The method to execute when the command is called</param>
        public Command(Action<object> executeMethod) : this(executeMethod, null) { }
        public Command(Action executeMethod)
        {
            _executeParameterlessMethod = executeMethod;
        }
        /// <summary>
        /// Create a delegate command which executes the canExecuteMethod before executing the executeMethod
        /// </summary>
        /// <param name="executeMethod"></param>
        /// <param name="canExecuteMethod"></param>
        public Command(Action<object> executeMethod, Predicate<object> canExecuteMethod)
        {
            if (null == executeMethod)
                throw new ArgumentNullException("executeMethod");

            this._executeMethod = executeMethod;
            this._canExecuteMethod = canExecuteMethod;
        }

        public bool CanExecute(object parameter)
        {
            return (null == _canExecuteMethod) ? true : _canExecuteMethod(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (_executeMethod != null)
                _executeMethod(parameter);
            if (_executeParameterlessMethod != null)
                _executeParameterlessMethod();
        }

        private Predicate<object> _canExecuteMethod;
        private Action<object> _executeMethod;
        private Action _executeParameterlessMethod;
    }

    public class Command<T> : ICommand
    {
        /// <summary>
        /// Create a command that can always be executed
        /// </summary>
        /// <param name="executeMethod">The method to execute when the command is called</param>
        public Command(Action<T> executeMethod) : this(executeMethod, null) { }

        /// <summary>
        /// Create a delegate command which executes the canExecuteMethod before executing the executeMethod
        /// </summary>
        /// <param name="executeMethod"></param>
        /// <param name="canExecuteMethod"></param>
        public Command(Action<T> executeMethod, Predicate<T> canExecuteMethod)
        {
            if (null == executeMethod)
                throw new ArgumentNullException("executeMethod");

            this._executeMethod = executeMethod;
            this._canExecuteMethod = canExecuteMethod;
        }

        public bool CanExecute(object parameter)
        {
            return (null == _canExecuteMethod) ? true : _canExecuteMethod((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (parameter == null)
            {
                _executeMethod(default(T));
            }
            else
            {
                _executeMethod((T) parameter);
            }
        }

        private Predicate<T> _canExecuteMethod;
        private Action<T> _executeMethod;
    }

}
