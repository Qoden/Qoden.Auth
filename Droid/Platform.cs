using System;
namespace Qoden.Auth
{
    public class Platform : AbstractPlatform
    {
        public string SecureStorePassword { get; set; } = "-6&A4\"IABFL#";

        public override ISecureStore CreateAccountStore()
        {
            return new SecureStore(SecureStorePassword.ToCharArray());
        }
    }
}
