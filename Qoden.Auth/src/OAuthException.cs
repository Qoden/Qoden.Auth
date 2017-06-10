using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Qoden.Util;

namespace Qoden.Auth
{
    public class OAuthException : AuthException
    {
        public OAuthException(HttpResponseMessage responseMessage, Dictionary<string, object> response) : base(ErrorMessage(responseMessage, response))
        {
            Response = response;
            ResponseMessage = responseMessage;
        }

        public OAuthException(HttpValueCollection query) : base(ErrorMessage(query))
        {
            Query = query;
        }

        public OAuthException(Exception ex) : base("OAuth failed due to unexpected error", ex)
        {
        }

        public Dictionary<string, object> Response { get; private set; }
        public HttpResponseMessage ResponseMessage { get; private set; }
        public HttpValueCollection Query { get; private set; }

        private static string ErrorMessage(HttpValueCollection query)
        {
            var error = new StringBuilder();
            if (query != null)
            {
                foreach (var kv in query)
                {
                    error.Append(kv.Key).Append(" : ").Append(kv.Value).AppendLine();
                }
            }
            return error.ToString();
        }

        private static string ErrorMessage(HttpResponseMessage responseMessage, Dictionary<string, object> response)
        {
            if (response != null && response.ContainsKey("error"))
            {
                if (response.ContainsKey("error_description"))
                    return String.Format("{0}: {1}", response["error"], response["error_description"]);
                else
                    return String.Format("{0}", response["error"]);
            }
            else
            {
                return String.Format("{0}: {1}", responseMessage.StatusCode, responseMessage.ReasonPhrase);
            }
        }
    }
}
