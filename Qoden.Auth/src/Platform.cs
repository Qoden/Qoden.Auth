using System;
using Qoden.Validation;

namespace Qoden.Auth
{
    public abstract class Platform
    {
        /// <summary>
        /// Crate secure store to be used to store oauth tokens
        /// </summary>
        public abstract ISecureStore CreateAccountStore();

        private static object mutex = new object();
        private static volatile Platform defaultContext;

        public static Platform Instance
        {
            get
            {
                if (defaultContext == null)
                {
                    lock (mutex)
                    {
                        if (defaultContext == null)
                        {
                            defaultContext = Util.Plugin.Load<Platform>("Qoden.Auth", "Platform");
                        }
                    }
                }
                return defaultContext;
            }
            set
            {
                lock (mutex)
                {
                    Assert.Property(value).NotNull();
                    defaultContext = value;
                }
            }
        }
    }
}
