using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Qoden.Util;
using Qoden.Validation;

namespace Qoden.Auth
{
    /// <summary>
    /// Models most common OAuth flow
    /// <br/>
    /// <list type="number">
    /// <item>Refresh access token using stored refrsh token (if any)</item>
    /// <item>If previous step failed - login with grant code</item>
    /// <item>If any of above steps succeeded - updated saved oauth user data</item>
    /// </list>
    /// </summary>
    public class SimpleOAuth
    {
        private Dictionary<string, string> _refreshTokenQuery;
        private readonly OAuthApi _api;
        private OAuthGrantCodeFlow _grantCodeFlow;
        private DefaultValue<ILogger> _logger;
        private DefaultValue<ISecureStore> _store;
        private string _profileKey;

        public SimpleOAuth(OAuthApi api)
        {
            Assert.Argument(api, nameof(api)).NotNull();

            _api = api;
            _store = Default.Value(() => AuthContext.Default.CreateAccountStore());
            _logger = Default.Value(() => AuthContext.Default.LoggerFactory.CreateLogger(GetType().Name));
            ProfileKey = "Qoden.SimpleOAuth.Profile";
        }

        /// <summary>
        /// OAuth flow instance to authenticate user.
        /// </summary>
        public OAuthGrantCodeFlow Flow
        {
            get
            {
                if (_grantCodeFlow == null)
                {
                    _grantCodeFlow = new OAuthGrantCodeFlow(Api);
                }
                return _grantCodeFlow;
            }
            set => _grantCodeFlow = Assert.Property(value).NotNull().Value;
        }

        /// <summary>
        /// Secure store to keep data returned by auth process.
        /// </summary>
        public ISecureStore SecureStore
        {
            get { return _store.Value; }
            set => _store.Value = value;
        }

        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Logger
        {
            get { return _logger.Value; }
            set { _logger.Value = value; }
        }

        /// <summary>
        /// OAuth api
        /// </summary>
        public OAuthApi Api => _api;

        /// <summary>
        /// Gets or sets the authorization page query.
        /// </summary>
        public Dictionary<string, string> AuthorizationPageQuery
        {
            get { return Flow.AuthorizationPageQuery; }
            set { Flow.AuthorizationPageQuery = value; }
        }

        /// <summary>
        /// Gets or sets the token query.
        /// </summary>
        public Dictionary<string, string> TokenQuery
        {
            get { return Flow.TokenQuery; }
            set { Flow.TokenQuery = value; }
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
        /// Get or set <see cref="ISecureStore"/> key to find profile data.
        /// </summary>
        public string ProfileKey
        {
            get { return _profileKey; }
            set { _profileKey = Assert.Property(value).NotEmpty().Value; }
        }

        /// <summary>
        /// Login page to be used during grant code flow (see <see cref="Flow"/>).
        /// </summary>
        public IOAuthLoginUI Page => Flow.LoginPage;

        /// <summary>
        /// Run authentication flow.
        /// </summary>
        /// <remarks>
        /// <list>
        /// <item>At first method tries to find stored profile data and check if it is not expired.</item>
        /// <item>Then if refresh token is available it tries to refresh auth data.</item>
        /// <item>If refresh fails or not possible then auth process starts from scratch.</item>
        /// </list>        
        /// </remarks>
        public async Task<Dictionary<string, object>> Auth(bool force = false)
        {
            Dictionary<string, object> profile = null;
            try
            {
                var savedProfile = SecureStore.Get<Dictionary<string, object>>(ProfileKey);
                if (savedProfile != null)
                {
                    if (force || ProfileExpired(savedProfile))
                    {
                        var refreshToken = savedProfile.GetValue(OAuthApi.RefreshToken);
                        if (refreshToken != null)
                        {
                            profile = await _api.LoginWithRefreshToken(refreshToken.ToString(), RefreshTokenQuery);
                        }
                    }
                    else
                    {
                        return savedProfile;
                    }
                }
            }
            catch (OAuthException refreshError)
            {
                Logger.LogInformation("Refresh token failed: {error}", refreshError);
            }

            if (profile == null)
            {
                try
                {
                    profile = await _grantCodeFlow.Run();
                }
                catch (OAuthException grantCodeError)
                {
                    Logger.LogInformation("Login failed: {error}", grantCodeError);
                }
            }

            if (profile != null)
            {
                profile[LastLoggedInKey] = DateTime.UtcNow;
                SecureStore.Set(ProfileKey, profile);
            }
            return profile;
        }

        /// <summary>
        /// Key to store last login time inside user profile.
        /// </summary>
        public string LastLoggedInKey { get; set; } = "LastLoggedIn";

        private bool ProfileExpired(Dictionary<string, object> savedProfile)
        {
            var lastLoggedIn = savedProfile.GetValue(LastLoggedInKey);
            if (!(lastLoggedIn is IConvertible))
                return true;
            var savedAt = Convert.ToDateTime(lastLoggedIn, CultureInfo.InvariantCulture);
            var expiration = savedProfile.GetValue(OAuthApi.ExpiresIn);
            if (expiration != null)
            {
                var expirationTimspan = Convert.ToInt32(savedProfile.GetValue(OAuthApi.ExpiresIn, 0));
                if (DateTime.UtcNow > savedAt + TimeSpan.FromSeconds(expirationTimspan))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
