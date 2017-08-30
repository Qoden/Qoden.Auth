using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Qoden.Util;
using Qoden.Validation;

namespace Qoden.Auth
{
    using UserProfile = Dictionary<string, object>;

    /// <summary>
    /// Custodian check users access rights and performs login or refresh operation as needed.
    /// </summary>
    public class Principal : INotifyPropertyChanged
    {
        private readonly SingletonOperation<UserProfile> _authOperation;
        private ILogger _logger;
        private ISecureStore _store;
        private string _profileKey;
        private bool _force;
        private readonly IAuthStrategy _strategy;

        public Principal(IAuthStrategy strategy)
        {
            _strategy = Assert.Argument(strategy, nameof(strategy)).NotNull().Value;
            _profileKey = "Qoden.Auth.Custodian.Profile";
            _authOperation = new SingletonOperation<UserProfile>(RunFlow);
        }

        /// <summary>
        /// Secure store to keep data returned by auth process.
        /// </summary>
        public ISecureStore SecureStore
        {
            get
            {
                if (_store == null)
                {
                    _store = AbstractPlatform.Instance.CreateAccountStore();
                }
                return _store;
            }
            set => _store = Assert.Property(value).NotNull().Value;
        }

        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = Config.LoggerFactory?.CreateLogger(typeof(Principal));
                }
                return _logger;
            }
            set => _logger = Assert.Property(value).NotNull().Value;
        }

        /// <summary>
        /// Get or set <see cref="ISecureStore"/> key to find profile data.
        /// </summary>
        public string ProfileKey
        {
            get => _profileKey;
            set
            {
                Assert.State(_authOperation.Started, "Started")
                   .IsFalse("Cannot change profile key flow started");
                _profileKey = Assert.Property(value).NotEmpty().Value;
            }
        }

        UserProfile _profile;
        public UserProfile Info
        {
            get => _profile;
            set
            {
                _profile = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Info"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsAuthorized"));
            }
        }

        public bool IsAuthorized => Info != null;

        public Task<UserProfile> Authenticate(bool force = false)
        {
            return Authenticate(CancellationToken.None, force); 
        }

        public async Task<UserProfile> Authenticate(CancellationToken token, bool force = false)
        {
            if (Logger.IsEnabled(LogLevel.Debug))
                Logger.LogDebug("User authentication started in {force} mode", force ? "Forced" : "Non-Forced");
            bool isUpgrade = force && !_force && _authOperation.Started;

            //if client requested forced login while non-forced login running 
            //then wait for previous operation to complete and run it again in 
            //forced mode
            if (isUpgrade)
            {
                Logger.LogDebug("Waiting for non-forced operation to complete");
                try
                {
                    await _authOperation.Start();
                }
                catch (AuthException)
                {
                    //ignore
                }
            }

            try
            {
                if (_authOperation.Started)
                {
                    Logger.LogDebug("Join already running authentication operation");
                    return await _authOperation.Start();
                }
                else
                {
                    try
                    {
                        _force = force;
                        Logger.LogDebug("Start authentication operation");
                        return await _authOperation.Start();
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("Authentication finished with error. {error}", e);
                        throw;
                    }
                    finally
                    {
                        _force = false;
                    }
                }
            }
            finally
            {
                Logger.LogDebug("Finish authentication operation");
            }
        }

        private async Task<Dictionary<string, object>> RunFlow()
        {
            Dictionary<string, object> profile = null;
            try
            {
                var savedProfile = SecureStore.Get<Dictionary<string, object>>(ProfileKey);
                if (savedProfile != null)
                {
                    if (_force || await _strategy.ProfileExpired(savedProfile))
                    {
                        profile = await _strategy.Refresh(savedProfile);
                    }
                    else
                    {
                        return savedProfile;
                    }
                }
            }
            catch (AuthException refreshError)
            {
                Logger.LogInformation("Refresh token failed: {error}", refreshError);
            }

            if (profile == null)
            {
                try
                {
                    profile = await _strategy.Authorize();
                }
                catch (AuthException grantCodeError)
                {
                    Logger.LogInformation("Login failed: {error}", grantCodeError);
                    throw;
                }
            }

            if (profile != null)
            {
                SecureStore.Set(ProfileKey, profile);
            }

            Info = profile;

            return profile;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static Principal OAuthGrantCode(OAuthApi api, IOAuthLoginUI loginPage, Action<OAuthGrantCodeFlow> config = null)
        {
            var flow = new OAuthGrantCodeFlow(api, loginPage);
            config?.Invoke(flow);
            return new Principal(flow);
        }

        public static Principal OAuthClientSide(OAuthApi api, IOAuthLoginUI loginPage, Action<OAuthClientSideFlow> config = null)
        {
            var flow = new OAuthClientSideFlow(api, loginPage);
            config?.Invoke(flow);
            return new Principal(flow);
        }
    }
}
