namespace SportBetting.WPF.Prism.Database
{
    public interface IDatabaseManager
    {
        void EnsureDatabase(bool isTestMode);
        void DropDatabase(bool isTestMode);
        bool DeleteOldObjects();
    }
}