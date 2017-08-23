using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Qoden.Validation;

namespace Qoden.Auth
{
    /// <summary>
    /// Basic OAuth endpoint api
    /// </summary>
    public class OAuthApi
    {
        public const string AccessToken = "access_token";
        public const string RefreshToken = "refresh_token";
        public const string TokenType = "token_type";
        public const string ExpiresIn = "expires_in";
        public const string Error = "error";
        public const string ErrorMessage = "error_message";
        public const string IdToken = "id_token";

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Qoden.Auth.OAuthApi"/> class.
        /// </summary>
        /// <param name="config">OAuth configuration</param>
        /// <param name="tokenUrl">URL to exchange access code into access token and refresh token</param>
        /// <param name="authUrl">Login page URL</param>
        public OAuthApi(OAuthConfig config, Uri tokenUrl, Uri authUrl)
        {
            config.Validate();
            Config = config;
            TokenUrl = tokenUrl;
            AuthUrl = authUrl;
        }

        /// <summary>
        /// Optional ILogger instance to log what OAuth API is doing.  
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets the OAuth config.
        /// </summary>
        public OAuthConfig Config { get; }
        private Uri _tokenUrl;
        /// <summary>
        /// Gets or sets the token URL.
        /// </summary>
        public Uri TokenUrl
        {
            get => _tokenUrl;
            set
            {
                Assert.Argument(value, "TokenUrl").NotNull().IsAbsoluteUri();
                _tokenUrl = value;
            }
        }
        private Uri _authUrl;
        /// <summary>
        /// Gets or sets the auth URL.
        /// </summary>
        public Uri AuthUrl
        {
            get => _authUrl;
            set
            {
                Assert.Argument(value, "AuthUrl").NotNull().IsAbsoluteUri();
                _authUrl = value;
            }
        }

        /// <summary>
        /// Generate authorization page url.
        /// </summary>
        /// <param name="requestParameters">Query parmaetesr to be added in addition to default parameters</param>
        /// <returns></returns>
        public virtual Uri GetAuthorizationPageUrl(Dictionary<string, string> requestParameters = null)
        {
            if (requestParameters == null) requestParameters = new Dictionary<string, string>();
            AddRequestParameter(ref requestParameters, "client_id", Config.ClientId);
            AddRequestParameter(ref requestParameters, "redirect_uri", Config.ReturnUrl);
            AddRequestParameter(ref requestParameters, "base", AuthUrl.AbsoluteUri);
            AddRequestParameter(ref requestParameters, "response_type", "code");
            AddRequestParameter(ref requestParameters, "response_mode", "query");

            return AuthRequestUrl(AuthUrl, requestParameters);
        }

        /// <summary>
        /// Logins the with username password.
        /// </summary>
        /// <returns>The with username password.</returns>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="requestParameters">Request parameters.</param>
        public virtual async Task<Dictionary<string, object>> LoginWithUsernamePassword(string username, string password, Dictionary<string, string> requestParameters = null)
        {
            Assert.Argument(username, "username").NotEmpty();
            Assert.Argument(password, "password").NotNull();
            
            if (Logger != null && Logger.IsEnabled(LogLevel.Debug))
                Logger.LogDebug("LoginWithUsernamePassword {username} {requestParameters}", username, requestParameters);
            
            AddRequestParameter(ref requestParameters, "username", username);
            AddRequestParameter(ref requestParameters, "password", password);
            AddRequestParameter(ref requestParameters, "grant_type", "password");
            return await LoginWithClientCredentials(requestParameters);
        }

        /// <summary>
        /// Login with client id and secret specified in Config
        /// </summary>
        /// <param name="token">cancellation token</param>
        /// <param name="requestParameters">additional request parameters</param>
        /// <returns></returns>
        public virtual async Task<Dictionary<string, object>> LoginWithClientCredentials(
            CancellationToken token,
            Dictionary<string, string> requestParameters = null
        )
        {
            Assert.State(Config.ClientSecret, "ClientSecret").NotEmpty();
            
            if (Logger != null && Logger.IsEnabled(LogLevel.Debug))
                Logger.LogDebug("LoginWithClientCredentials {requestParameters}", requestParameters);
            
            AddRequestParameter(ref requestParameters, "grant_type", "client_credentials");
            return await Login(requestParameters, token);
        }

        /// <summary>
        /// Logins the with client credentials.
        /// </summary>
        /// <returns>The with client credentials.</returns>
        /// <param name="requestParameters">Request parameters.</param>
        public Task<Dictionary<string, object>> LoginWithClientCredentials(
            Dictionary<string, string> requestParameters = null)
        {
            return LoginWithClientCredentials(CancellationToken.None, requestParameters);
        }

        /// <summary>
        /// Exchange grant code to access and refresh token.
        /// </summary>
        /// <param name="grantCode">grant code</param>
        /// <param name="token">cancellation token</param>
        /// <param name="requestParameters">additional request parameters</param>
        /// <returns></returns>
        public virtual async Task<Dictionary<string, object>> LoginWithGrantCode(
            string grantCode,
            CancellationToken token,
            Dictionary<string, string> requestParameters = null)
        {
            Assert.Argument(grantCode, "grantCode").NotEmpty();    
            
            if (Logger != null && Logger.IsEnabled(LogLevel.Debug))
                Logger.LogDebug("LoginWithGrantCode {requestParameters}", requestParameters);
            
            AddRequestParameter(ref requestParameters, "code", grantCode);
            AddRequestParameter(ref requestParameters, "grant_type", "authorization_code");
            return await Login(requestParameters, token);
        }

        /// <summary>
        /// Logins the with grant code.
        /// </summary>
        /// <param name="grantCode">Grant code.</param>
        /// <param name="requestParameters">Request parameters.</param>
        public Task<Dictionary<string, object>> LoginWithGrantCode(
            string grantCode,
            Dictionary<string, string> requestParameters = null)
        {            
            return LoginWithGrantCode(grantCode, CancellationToken.None, requestParameters);
        }

        /// <summary>
        /// Exchange refresh token from userCrmConfiguration to access and refresh tokens. Method will fail if CRM configuration does not have refresh token.
        /// </summary>
        /// <param name="refreshToken">refresh token</param>
        /// <param name="token">cancellation token</param>
        /// <param name="requestParameters">additional request parameters</param>
        public virtual async Task<Dictionary<string, object>> LoginWithRefreshToken(
            string refreshToken,
            CancellationToken token,
            Dictionary<string, string> requestParameters = null)
        {
            Assert.Argument(refreshToken, "refreshToken").NotEmpty();
            
            if (Logger != null && Logger.IsEnabled(LogLevel.Debug))
                Logger.LogDebug("LoginWithRefreshToken {refreshToken} {requestParameters}", refreshToken, requestParameters);
            
            AddRequestParameter(ref requestParameters, "grant_type", "refresh_token");
            AddRequestParameter(ref requestParameters, "refresh_token", refreshToken);
            return await Login(requestParameters, token);
        }

        public Task<Dictionary<string, object>> LoginWithRefreshToken(
            string refreshToken,
            Dictionary<string, string> requestParameters = null)
        {
            return LoginWithRefreshToken(refreshToken, CancellationToken.None, requestParameters);
        }

        /// <summary>
        /// Most generic login method. Client expected to specify app parmaetesr excep client_id, client_secret and redirect_url
        /// </summary>
        /// <param name="requestParameters">Login request parameters</param>
        /// <param name="token">cancellation token</param>
        /// <returns></returns>
        public virtual async Task<Dictionary<string, object>> Login(
            Dictionary<string, string> requestParameters,
            CancellationToken token)
        {
            if (Logger != null && Logger.IsEnabled(LogLevel.Debug))
                Logger.LogDebug("Login {requestParameters}", requestParameters);
            
            AddRequestParameter(ref requestParameters, "client_id", Config.ClientId);
            AddRequestParameter(ref requestParameters, "client_secret", Config.ClientSecret);
            AddRequestParameter(ref requestParameters, "redirect_uri", Config.ReturnUrl);

            var body = new FormUrlEncodedContent(requestParameters);
            using (var http = new HttpClient())
            {
                var responseMessage = await http.PostAsync(TokenUrl, body, token);
                return await ProcessResponseMessage(responseMessage);
            }
        }

        private async Task<Dictionary<string, object>> ProcessResponseMessage(HttpResponseMessage responseMessage)
        {            
            var json = await responseMessage.Content.ReadAsStringAsync();
            var response = json != null 
                ? JsonConvert.DeserializeObject<Dictionary<string, object>>(json)
                : new Dictionary<string, object>();
            if (!responseMessage.IsSuccessStatusCode)
                throw new OAuthException(responseMessage, response);
            return OnResponse(response);
        }

        protected virtual Dictionary<string, object> OnResponse(Dictionary<string, object> response)
        {
            return response;
        }

        protected static void AddRequestParameter(ref Dictionary<string, string> context, string paramName, string paramValue)
        {
            if (string.IsNullOrEmpty(paramValue)) return;
            if (context == null)
            {
                context = new Dictionary<string, string>();
            }

            if (!context.ContainsKey(paramName))
            {
                context.Add(paramName, paramValue);
            }
        }

        private Uri AuthRequestUrl(Uri baseUri, Dictionary<string, string> requestParameters)
        {
            var url = new StringBuilder(baseUri.AbsoluteUri);
            url.Append("?");

            var kvs = requestParameters.ToList();
            for (int i = 0; i < kvs.Count; ++i)
            {
                if (i > 0)
                    url.Append('&');

                if (kvs[i].Value != null)
                    url.Append(kvs[i].Key).Append('=').Append(System.Net.WebUtility.UrlEncode(kvs[i].Value));
            }

            return new Uri(url.ToString());
        }
    }
}
