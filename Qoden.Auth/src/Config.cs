using Microsoft.Extensions.Logging;
using Qoden.Validation;

namespace Qoden.Auth
{
    /// <summary>
    /// Qoden Auth library context
    /// </summary>
    public static class Config
    {
        static Config()
        {
            LoggerFactory = new LoggerFactory();
            LoggerFactory.AddDebug();
        }

        private static ILoggerFactory loggerFactory;
        /// <summary>
        /// Auth library logger factory
        /// </summary>
        public static ILoggerFactory LoggerFactory
        {
            get { return loggerFactory; }
            set { loggerFactory = Assert.Property(value).NotNull().Value; }
        }

    }
}
