using System;
using Qoden.Validation;

namespace Qoden.Auth
{
    public class AzureOAuthApi : OAuthApi
    {
        public static readonly Uri AzureCommonTokenUrl = new Uri("https://login.microsoftonline.com/common/oauth2/v2.0/token");
        public static readonly Uri AzureCommonAuthUrl = new Uri("https://login.microsoftonline.com/common/oauth2/v2.0/authorize");

        public AzureOAuthApi(OAuthConfig config) : base(config, AzureCommonTokenUrl, AzureCommonAuthUrl)
        {
        }

        public void SetTenantId(string tenantId)
        {
            Assert.Argument(tenantId, nameof(tenantId)).NotNull();
            TokenUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token");
            AuthUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize");
        }
    }
}
