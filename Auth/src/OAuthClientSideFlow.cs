using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Qoden.Util;

namespace Qoden.Auth
{
    /// <summary>
    /// OAuth implict or client flow. This flow loads authentication data directly from 'authorize' enpoint.
    /// </summary>
    public class OAuthClientSideFlow : OAuthStrategy
    {
        public OAuthClientSideFlow(OAuthApi api, IOAuthLoginUI loginPage) : base(api, loginPage)
        {
        }

        protected override Task<Dictionary<string, object>> OAuthAuthorizeAction(HttpValueCollection response)
        {
            var result = new Dictionary<string, object>();
            foreach (var kv in response)
            {
                result[kv.Key] = kv.Value;
            }
            return Task.FromResult(result);
        }
    }
}
