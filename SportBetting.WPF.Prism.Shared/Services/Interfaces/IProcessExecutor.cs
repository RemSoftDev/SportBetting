namespace SportBetting.WPF.Prism.Shared.Services.Interfaces
{
    public interface IProcessExecutor
    {
        void Run(string location, string args);
    }
}