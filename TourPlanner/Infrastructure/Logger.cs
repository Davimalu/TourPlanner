using log4net;
using System.IO;
using TourPlanner.Infrastructure.Interfaces;

namespace TourPlanner.Infrastructure
{
    public class Logger<T> : ILogger<T>
    {
        private readonly ILog _logger;

        
        /// <summary>
        /// Initializes a new instance of the Logger class using the default log4net configuration file
        /// </summary>
        public Logger()
        {
            // Configure Log4Net using the default config file (if not already configured)
            if (!LogManager.GetRepository().Configured)
            {
                var configPath = "./config/log4net.config"; // TODO: Don't hardcode this
                if (File.Exists(configPath))
                {
                    log4net.Config.XmlConfigurator.Configure(new FileInfo(configPath));
                }
            }
            
            _logger = LogManager.GetLogger(typeof(T));
        }

        public void Info(string message) => _logger.Info(message);
        public void Debug(string message) => _logger.Debug(message);
        public void Warn(string message) => _logger.Warn(message);
        public void Warn(string message, Exception ex) => _logger.Warn(message, ex);
        public void Error(string message) => _logger.Error(message);
        public void Error(string message, Exception ex) => _logger.Error(message, ex);
        public void Fatal(string message) => _logger.Fatal(message);
        public void Fatal(string message, Exception ex) => _logger.Fatal(message, ex);
    }
}
