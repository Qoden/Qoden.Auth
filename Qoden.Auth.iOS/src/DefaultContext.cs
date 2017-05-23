using System;
using Qoden.Validation;

namespace Qoden.Auth.iOS
{
    public class DefaultContext : AuthContext
    {
        public override ISecureStore CreateAccountStore()
        {
            return new SecureStore();
        }

        public override IOAuthLoginUI CreateLoginPage(OAuthApi api)
        {
            Assert.Argument(api, nameof(api)).NotNull();
            return new SafariLoginPage(api.Config);
        }
    }
}
