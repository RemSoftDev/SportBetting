using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SportBetting.WPF.Prism.WpfHelper;
using System.Windows.Input;

namespace SportBetting.WPF.Prism.WpfHelper
{
    public static class TextBlockBehaviour
    {
        public static readonly DependencyProperty LoadedCommand = EventBehaviourFactory.CreateCommandExecutionEventBehaviour(System.Windows.Controls.TextBlock.LoadedEvent, "LoadedCommand", typeof(TextBlockBehaviour));

        public static void SetLoadedCommand(DependencyObject o, ICommand value)
        {
            o.SetValue(LoadedCommand, value);
        }

        public static ICommand GetLoadedCommand(DependencyObject o)
        {
            return o.GetValue(LoadedCommand) as ICommand;
        }
    }
}
