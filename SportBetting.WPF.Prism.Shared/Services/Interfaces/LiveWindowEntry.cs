using System.Threading;
using System.Windows;
using SharedInterfaces;

namespace SportBetting.WPF.Prism.Shared.Services.Interfaces
{
    public class LiveWindowEntry
    {
        public object locker = new object();
        public Window Window { get; set; }

        public bool Loaded { get; set; }

        public object DataContext { get; set; }
    }
}