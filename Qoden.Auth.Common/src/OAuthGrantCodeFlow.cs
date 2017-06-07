using System.Collections.Generic;
using System.Threading.Tasks;
using Qoden.Util;
using Qoden.Validation;

namespace Qoden.Auth
{
    using LoginOperation = SingletonOperation<Dictionary<string, object>>;
    public delegate Task<Dictionary<string, object>> GrantCodeExchangeDelegate(OAuthGrantCodeFlow flow, string grantCode);

    /// <summary>
    /// OAuth grant code authorization flow which can be used with both OpenID Connect and OAuth.
    /// </summary>
    /// <remarks>
    /// Flow 
    /// <list type="number">
    ///     <item>Display login page.</item>
    ///     <item>Receive "grant code" after succesfull login.</item>
    ///     <item>
    ///     Exchange "grant code" for "access code" and other tokens. This is done with <see cref="GrantCodeExchange"/> 
    ///     function which is by default calls <see cref="M:OAuthApi.LoginWithGrantCode"/>.
    ///     <list type="number"> 
    ///         <item>In case of OpenID Connect default behavior just works.</item>
    ///         <item>
    ///         In case of OAuth call to <see cref="M:OAuthApi.LoginWithGrantCode"/> might 
    ///         require <see cref="OAuthApi"/> to be configured with <see cref="OAuthConfig.ClientSecret"/>.
    ///         <list type="number">
    ///             <item>If you don't care you can keep client secret inside mobile app and set <see cref="OAuthConfig.ClientSecret"/>.</item>
    ///             <item>Or you can override <see cref="GrantCodeExchange"/> to call your server with "grant code" to perform exchange. 
    ///             This way you can keep client secret on your server.</item>
    ///         </list>
    ///         </item>
    ///     </list>
    ///     </item>
    /// </list>
    /// </remarks>
    public class OAuthGrantCodeFlow
    {
        private DefaultValue<IOAuthLoginUI> _loginPage;
        private readonly LoginOperation loginOperation;
        private Dictionary<string, string> authPageParams;
        private Dictionary<string, string> tokenRequestParams;
        private readonly OAuthApi oauth;
        private GrantCodeExchangeDelegate _grantCodeExchange;

        public OAuthGrantCodeFlow(OAuthApi oauth)
        {
            Assert.Argument(oauth, nameof(oauth)).NotNull();

            _loginPage = Default.Value(() => AuthContext.Default.CreateLoginPage(oauth));
            loginOperation = new LoginOperation(RunFlow);
            this.oauth = oauth;
            _grantCodeExchange = (sender, grantCode) => oauth.LoginWithGrantCode(grantCode, sender.TokenQuery);
        }

        public IOAuthLoginUI LoginPage
        {
            get { return _loginPage.Value; }
            set { _loginPage.Value = Assert.Property(value).NotNull().Value; }
        }

        /// <summary>
        /// Start OAuth grant code flow.         
        /// </summary>
        /// <remarks>
        /// Flow can be started multiple times from different threads. 
        /// In this case same tasks returned and all threads resume at the same time 
        /// when login flow finish.
        /// </remarks>
        /// <returns>
        /// Information returned by grant code exchange procedure (see <see cref="GrantCodeExchange"/> 
        /// and <see cref="OAuthGrantCodeFlow"> remarks section)
        /// </returns>
        public Task<Dictionary<string, object>> Run()
        {
            return loginOperation.Start();
        }

        /// <summary>
        /// Additional query parameters for authorization page
        /// </summary>
        public Dictionary<string, string> AuthorizationPageQuery
        {
            get { return authPageParams; }
            set
            {
                Assert.State(loginOperation.Started, "Started")
                   .IsFalse("Cannot change authorization query parameters when flow started");
                authPageParams = value;
            }
        }

        /// <summary>
        /// Additional query parameters for token request (user by default <see cref="GrantCodeExchange"/> procedure).
        /// </summary>
        public Dictionary<string, string> TokenQuery
        {
            get { return tokenRequestParams; }
            set
            {
                Assert.State(loginOperation.Started, "Started")
                   .IsFalse("Cannot change token query parameters when flow started");
                tokenRequestParams = value;
            }
        }

        private async Task<Dictionary<string, object>> RunFlow()
        {
            var uri = oauth.GetAuthorizationPageUrl(AuthorizationPageQuery);
            var grantCode = await LoginPage.GetGrantCode(uri);
            return await GrantCodeExchange(this, grantCode);
        }

        /// <summary>
        /// Function to be used to exchange grant code for access code 
        /// (see <see cref="OAuthGrantCodeFlow"> remarks section for discussion).
        /// Calls <see cref="M:OAuthApi.LoginWithGrantCode"/> by default.
        /// </summary>
        public GrantCodeExchangeDelegate GrantCodeExchange
        {
            get { return _grantCodeExchange; }
            set { _grantCodeExchange = Assert.Property(value).NotNull().Value; }
        }
    }
}
