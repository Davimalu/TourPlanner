using TourPlanner.Infrastructure.Interfaces;

namespace TourPlanner.Infrastructure
{
    public static class LoggerFactory
    {
        private const string DefaultConfigPath = "./config/log4net.config";

        public static ILoggerWrapper GetLogger<T>()
        {
            return Log4NetWrapper.CreateLogger(typeof(T), DefaultConfigPath);
        }
    }
}
