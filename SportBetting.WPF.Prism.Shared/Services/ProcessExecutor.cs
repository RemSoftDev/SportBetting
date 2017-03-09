using System.Diagnostics;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;

namespace SportBetting.WPF.Prism.Shared.Services
{
    public class ProcessExecutor : IProcessExecutor
    {
        public void Run(string location, string args)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = location;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            proc.Start();
        }
    }
}