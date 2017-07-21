using System;
using Qoden.Validation;

namespace Qoden.Auth
{
    /// <summary>
    /// Azure OAuth API
    /// </summary>
    /// <remarks>
    /// There are two versions of API which behave slightly differently.
    /// Recommended is V2, while V1 still in use as of June 2017.
    /// 
    /// </remarks>
    public class AzureOAuthApi : OAuthApi
    {
        public static readonly string V2TokenUrlTemplate = "https://login.microsoftonline.com/{0}/oauth2/v2.0/token";
        public static readonly string V2AuthUrlTemplate = "https://login.microsoftonline.com/{0}/oauth2/v2.0/authorize";

        public static readonly string V1TokenUrlTemplate = "https://login.microsoftonline.com/{0}/oauth2/token";
        public static readonly string V1AuthUrlTemplate = "https://login.microsoftonline.com/{0}/oauth2/authorize";

        private string _authUrlTemplate, _tokenUrlTemplate;

        /// <summary>
        /// Create V2 Azure OAuth API.
        /// </summary>
        public AzureOAuthApi(OAuthConfig config) : this(config, V2TokenUrlTemplate, V2AuthUrlTemplate)
        {
        }

        /// <summary>
        /// Create Azure OAuth API with provided URL templates.
        /// </summary>
        /// <param name="config">OAuth configuration.</param>
        /// <param name="tokenUrl">Token URL template.</param>
        /// <param name="authUrl">Auth URL template.</param>
        public AzureOAuthApi(OAuthConfig config, string tokenUrl, string authUrl) 
            : base(config, MakeTenantUrl(tokenUrl, "common"), MakeTenantUrl(authUrl, "common"))
        {
            Assert.Argument(tokenUrl, nameof(tokenUrl)).NotEmpty();
            Assert.Argument(authUrl, nameof(authUrl)).NotEmpty();

            _authUrlTemplate = authUrl;
            _tokenUrlTemplate = tokenUrl;
        }

        private static Uri MakeTenantUrl(string template, string tenant)
        {
            return new Uri(string.Format(template, tenant));
        }

        /// <summary>
        /// Create V1 version of Azure OAuth API.
        /// </summary>
        public static AzureOAuthApi V1Api(OAuthConfig config)
        {
            return new AzureOAuthApi(config, V1TokenUrlTemplate, V1AuthUrlTemplate);
        }

        /// <summary>
        /// Create V2 version of Azure OAuth API.
        /// </summary>
        public static AzureOAuthApi V2Api(OAuthConfig config)
        {
            return new AzureOAuthApi(config, V2TokenUrlTemplate, V2AuthUrlTemplate);
        }

        /// <summary>
        /// Set tenant id. By default it is 'common'.
        /// </summary>
        public void SetTenantId(string tenantId)
        {
            Assert.Argument(tenantId, nameof(tenantId)).NotNull();
            TokenUrl = MakeTenantUrl(_tokenUrlTemplate, tenantId);
            AuthUrl = MakeTenantUrl(_authUrlTemplate, tenantId);
        }
    }
}
