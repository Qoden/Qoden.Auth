using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Qoden.Auth
{
    public class OAuthException : Exception
    {
        public OAuthException(HttpResponseMessage responseMessage, Dictionary<string, object> response) : base(ErrorMessage(responseMessage, response))
        {
            Response = response;
            ResponseMessage = responseMessage;
        }

        public Dictionary<string, object> Response { get; private set; }
        public HttpResponseMessage ResponseMessage { get; private set; }

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
