using System;
using Qoden.Validation;

namespace Qoden.Auth.iOS
{
    public class Platform : Qoden.Auth.Platform
    {
        public override ISecureStore CreateAccountStore()
        {
            return new SecureStore();
        }
    }
}
