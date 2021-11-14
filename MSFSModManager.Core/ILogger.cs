namespace MSFSModManager.Core
{

    public enum LogLevel
    {
        Info,
        Output,
        Warning,
        Error,
        CriticalError
    }

    public interface ILogger
    {
        void Log(LogLevel level, string message);
    }

}