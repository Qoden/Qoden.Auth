using System;
using Foundation;
using UIKit;

namespace Qoden.Auth.iOS
{
    /// <summary>
    /// Default iOS OAuth login page which opens login page in Safari
    /// </summary>
    public class SafariLoginPage : OAuthLoginPage 
    {
        protected override void DisplayLoginPage(Uri uri)
        {
            var nsUrl = new NSUrl(uri.AbsoluteUri);
            UIApplication.SharedApplication.OpenUrl(nsUrl);
        }

        protected override void HideLoginPage()
        {
        }
    }
}
