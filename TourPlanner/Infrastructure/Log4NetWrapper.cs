using log4net;
using System.IO;
using System.Reflection;
using TourPlanner.Infrastructure.Interfaces;

namespace TourPlanner.Infrastructure
{
    public class Log4NetWrapper : ILoggerWrapper
    {
        private readonly ILog _logger;

        private Log4NetWrapper(ILog logger)
        {
            _logger = logger;
        }


        /// <summary>
        /// Creates a logger using the specified configuration file
        /// </summary>
        /// <param name="configPath">Path to the log4net configuration file</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">Thrown when the config file is not found</exception>
        public static ILoggerWrapper CreateLogger(Type callerType, string configPath)
        {
            // Check if the config file exists
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException(
                    $"Log4Net configuration file not found at: {configPath}",
                    configPath
                );
            }

            // Configure Log4Net using the specified config file
            log4net.Config.XmlConfigurator.Configure(new FileInfo(configPath));

            // Create a concrete Log4Net logger using the caller type as the logger name
            ILog log4NetLogger = LogManager.GetLogger(callerType);

            return new Log4NetWrapper(log4NetLogger);
        }


        // ILoggerWrapper methods delegate to the underlying Log4Net logger:
        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }
        public void Warn(string message)
        {
            _logger.Warn(message);
        }

        public void Warn(string message, Exception ex)
        {
            _logger.Warn(message, ex);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Error(string message, Exception ex)
        {
            _logger.Error(message, ex);
        }

        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        public void Fatal(string message, Exception ex)
        {
            _logger.Fatal(message, ex);
        }
    }
}
