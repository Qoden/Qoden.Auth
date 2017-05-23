using Qoden.Validation;

namespace Qoden.Auth
{
    /// <summary>
    /// A container for OAuth client configuration.
    /// </summary>
    public class OAuthConfig
    {
        /// <summary>
        /// Client ID, usually generated usign Oauth server developer portal.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Client Secret usually generated with OAuth server developer portal.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// URL to redirect to after successfull authentication.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Root Url for the API, could have different 3rd-level subdomains.
        /// </summary>
        public string ApiRootUrl { get; set; }
    }

    public static class OAuthConfigExtensions
    {
        public static void Validate(this OAuthConfig config)
        {
            Validate(config, ArgumentValidator.Instance);
        }

        public static void Validate(this OAuthConfig config, IValidator errors)
        {
            errors.CheckValue(config, "config").NotNull();
            errors.CheckValue(config.ClientId, "clientId").NotEmpty();
        }
    }
}
