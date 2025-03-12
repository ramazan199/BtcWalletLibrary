namespace BtcWalletLibrary.Interfaces
{
    public interface ILoggingService
    {
        void LogInformation(string message);
        void LogError(System.Exception ex, string message);
        void LogWarning(string message);
    }
}
