using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Qoden.Util;
using Qoden.Validation;

namespace Qoden.Auth
{
    using UserProfile = Dictionary<string, object>;

    public abstract class OAuthStrategy : IAuthStrategy
    {
        private OAuthApi _api;
        private Dictionary<string, string> _refreshTokenQuery;
        private Dictionary<string, string> _authPageParams;
        private IOAuthLoginUI _loginPage;

        public OAuthStrategy(OAuthApi api, IOAuthLoginUI loginPage)
        {
            Assert.Argument(api, nameof(api)).NotNull();
            Assert.Argument(loginPage, nameof(loginPage)).NotNull();
            _api = api;
            _loginPage = loginPage;
        }

        public OAuthApi Api => _api;

        public async Task<UserProfile> Refresh(UserProfile savedProfile)
        {
            var refreshToken = savedProfile.GetValue(OAuthApi.RefreshToken);
            if (refreshToken != null)
            {
                var profile = await _api.LoginWithRefreshToken(refreshToken.ToString(), RefreshTokenQuery);
                if (profile != null)
                {
                    profile[LastLoggedInKey] = DateTime.UtcNow;
                }
                return profile;
            }
            return null;
        }

        public async Task<UserProfile> Authorize()
        {
            var uri = _api.GetAuthorizationPageUrl(AuthorizationPageQuery);
            var result = await LoginPage.GetResult(uri);
            var profile = await OAuthAuthorizeAction(result);
            if (profile != null)
            {
                profile[LastLoggedInKey] = DateTime.UtcNow;
            }
            return profile;
        }

        public Task<bool> ProfileExpired(UserProfile savedProfile)
        {
            var lastLoggedIn = savedProfile.GetValue(LastLoggedInKey);
            if (!(lastLoggedIn is IConvertible))
                return Task.FromResult(true);
            var savedAt = Convert.ToDateTime(lastLoggedIn, CultureInfo.InvariantCulture);
            var expiration = savedProfile.GetValue(OAuthApi.ExpiresIn);
            if (expiration != null)
            {
                var expirationTimspan = Convert.ToInt32(savedProfile.GetValue(OAuthApi.ExpiresIn, 0));
                if (DateTime.UtcNow > savedAt + TimeSpan.FromSeconds(expirationTimspan))
                {
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        /// <summary>
        /// Gets or sets the refresh token query.
        /// </summary>
        public Dictionary<string, string> RefreshTokenQuery
        {
            get { return _refreshTokenQuery; }
            set { _refreshTokenQuery = value; }
        }

        /// <summary>
        /// Additional query parameters for authorization page
        /// </summary>
        public Dictionary<string, string> AuthorizationPageQuery
        {
            get { return _authPageParams; }
            set { _authPageParams = value; }
        }

        /// <summary>
        /// Login page to collect users credentials.
        /// </summary>
        public IOAuthLoginUI LoginPage
        {
            get { return _loginPage; }
            set { _loginPage = Assert.Property(value).NotNull().Value; }
        }

        /// <summary>
        /// Key to store last login time inside user profile.
        /// </summary>
        public string LastLoggedInKey { get; set; } = "LastLoggedIn";

        /// <summary>
        /// Template method which makes request to OAuth 'authorize' endpoint 
        /// </summary>
        protected abstract Task<Dictionary<string, object>> OAuthAuthorizeAction(HttpValueCollection response);
    }
}
