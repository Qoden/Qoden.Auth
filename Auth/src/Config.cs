using Microsoft.Extensions.Logging;
using Qoden.Validation;

namespace Qoden.Auth
{
    /// <summary>
    /// Qoden Auth library context
    /// </summary>
    public static class Config
    {
        private static ILoggerFactory _loggerFactory;
        /// <summary>
        /// Auth library logger factory to be used by various components.
        /// </summary>
        public static ILoggerFactory LoggerFactory
        {
            get => _loggerFactory;
            set => _loggerFactory = Assert.Property(value).NotNull().Value;
        }

    }
}
