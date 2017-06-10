using System;
namespace Qoden.Auth
{
    public class GoogleOAuthApi : OAuthApi
    {
        public static readonly Uri GoogleAuthUrl = new Uri("https://accounts.google.com/o/oauth2/v2/auth");
        public static readonly Uri GoogleTokenUrl = new Uri("https://www.googleapis.com/oauth2/v4/token");

        public GoogleOAuthApi(OAuthConfig config) : base(config, GoogleTokenUrl, GoogleAuthUrl)
        {
        }
    }
}
