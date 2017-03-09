using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SportBetting.WPF.Prism.WpfHelper
{
    public static class ButtonBehaviour
    {
        public static readonly DependencyProperty PreviewMouseDownCommand = EventBehaviourFactory.CreateCommandExecutionEventBehaviour(System.Windows.Controls.Button.PreviewMouseDownEvent, "PreviewMouseDownCommand", typeof(ButtonBehaviour));
        public static readonly DependencyProperty PreviewMouseUpCommand = EventBehaviourFactory.CreateCommandExecutionEventBehaviour(System.Windows.Controls.Button.PreviewMouseUpEvent, "PreviewMouseUpCommand", typeof(ButtonBehaviour));

        public static void SetPreviewMouseDownCommand(DependencyObject o, ICommand value)
        {
            o.SetValue(PreviewMouseDownCommand, value);
        }

        public static ICommand GetPreviewMouseDownCommand(DependencyObject o)
        {
            return o.GetValue(PreviewMouseDownCommand) as ICommand;
        }

        public static void SetPreviewMouseUpCommand(DependencyObject o, ICommand value)
        {
            o.SetValue(PreviewMouseUpCommand, value);
        }

        public static ICommand GetPreviewMouseUpCommand(DependencyObject o)
        {
            return o.GetValue(PreviewMouseUpCommand) as ICommand;
        }
    }
}
