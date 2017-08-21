using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Qoden.Util;

namespace Qoden.Auth
{
    /// <summary>
    /// OAuth Grant code flow. This flow exhange grant token from 'authorize' endpoint to access token.
    /// </summary>
    public class OAuthGrantCodeFlow : OAuthStrategy
    {
        private Dictionary<string, string> _tokenRequestParams;

        public OAuthGrantCodeFlow(OAuthApi api, IOAuthLoginUI loginPage) : base(api, loginPage)
        {
        }

        protected override async Task<Dictionary<string, object>> OAuthAuthorizeAction(HttpValueCollection response)
        {
            if (!response.ContainsKey(GrantCodeKey))
            {
                throw new OAuthException(response);
            }        
            var code = response[GrantCodeKey];
            return await Api.LoginWithGrantCode(code, TokenQuery);
        }

        public string GrantCodeKey { get; set; } = "code";

        /// <summary>
        /// Additional query parameters for token request.
        /// </summary>
        public Dictionary<string, string> TokenQuery
        {
            get { return _tokenRequestParams; }
            set { _tokenRequestParams = value; }
        }
    }
}
