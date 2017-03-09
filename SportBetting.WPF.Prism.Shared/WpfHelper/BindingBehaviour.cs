using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SportBetting.WPF.Prism.WpfHelper;
using System.Windows.Input;

namespace SportBetting.WPF.Prism.WpfHelper
{
    public static class BindingBehaviour
    {
        public static readonly DependencyProperty TargetUpdatedCommand = EventBehaviourFactory.CreateCommandExecutionEventBehaviour(System.Windows.Data.Binding.TargetUpdatedEvent, "TargetUpdatedCommand", typeof(BindingBehaviour));

        public static void SetTargetUpdatedCommand(DependencyObject o, ICommand value)
        {
            o.SetValue(TargetUpdatedCommand, value);
        }

        public static ICommand GetTargetUpdatedCommand(DependencyObject o)
        {
            return o.GetValue(TargetUpdatedCommand) as ICommand;
        }
    }
}
