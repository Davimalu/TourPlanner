namespace TourPlanner.Infrastructure.Interfaces
{
    public interface ILogger<T>
    {
        void Info(string message);
        void Debug(string message);
        void Warn(string message);
        void Warn(string message, Exception ex);
        void Error(string message);
        void Error(string message, Exception ex);
        void Fatal(string message);
        void Fatal(string message, Exception ex);
    }
}
