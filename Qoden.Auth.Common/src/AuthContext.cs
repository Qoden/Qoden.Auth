using Microsoft.Extensions.Logging;
using Qoden.Validation;

namespace Qoden.Auth
{
    /// <summary>
    /// Qoden Auth library context
    /// </summary>
    public abstract class AuthContext
    {
        protected AuthContext()
        {
            LoggerFactory = new LoggerFactory();
            LoggerFactory.AddDebug();
        }

        private ILoggerFactory loggerFactory;
        /// <summary>
        /// Auth library logger factory
        /// </summary>
        public ILoggerFactory LoggerFactory
        {
            get { return loggerFactory; }
            set { loggerFactory = Assert.Property(value).NotNull().Value; }
        }

        /// <summary>
        /// Crate secure store to be used to store oauth tokens
        /// </summary>
        public abstract ISecureStore CreateAccountStore();

        /// <summary>
        /// Create oauth login page
        /// </summary>
        public abstract IOAuthLoginUI CreateLoginPage(OAuthApi api);

        private static object mutex = new object();
        private static volatile AuthContext defaultContext;
        /// <summary>
        /// Default auth context
        /// </summary>
        public static AuthContext Default
        {
            get
            {
                if (defaultContext == null)
                {
                    lock (mutex)
                    {
                        if (defaultContext == null)
                        {
                            defaultContext = Util.Plugin.Load<AuthContext>("Qoden.Auth", "DefaultContext");
                        }
                    }
                }
                return defaultContext;
            }
            set
            {
                Assert.Property(value).NotNull();
                defaultContext = value;
            }
        }
    }
}
