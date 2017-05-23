using System;
using Foundation;
using UIKit;

namespace Qoden.Auth.iOS
{
    /// <summary>
    /// Default iOS OAuth login page which opens login page in Safari
    /// </summary>
    public class SafariLoginPage : OAuthLoginPageBase 
    {
        public SafariLoginPage(OAuthConfig config) : base(config)
        {
        }

        protected override void OpenLoginPage(Uri uri)
        {
            var nsUrl = new NSUrl(uri.AbsoluteUri);
            UIApplication.SharedApplication.OpenUrl(nsUrl);
        }
    }
}
