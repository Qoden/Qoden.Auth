using System;
namespace Qoden.Auth
{
    public class Platform : AbstractPlatform
    {
        public override ISecureStore CreateAccountStore()
        {
            return new SecureStore();
        }
    }
}
