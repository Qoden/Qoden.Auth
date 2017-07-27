using System;
using Qoden.Validation;

namespace Qoden.Auth
{
    public abstract class AbstractPlatform
    {
        /// <summary>
        /// Crate secure store to be used to store oauth tokens
        /// </summary>
        public abstract ISecureStore CreateAccountStore();

        private static object mutex = new object();
        private static volatile AbstractPlatform defaultContext;

        public static AbstractPlatform Instance
        {
            get
            {
                if (defaultContext == null)
                {
                    lock (mutex)
                    {
                        if (defaultContext == null)
                        {
                            defaultContext = Util.Plugin.Load<AbstractPlatform>("Qoden.Auth", "Platform");
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
