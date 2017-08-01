using System;
using Android.Content;

namespace Qoden.Auth
{
    public class BrowserLoginPage : OAuthLoginPage
    {
        Context _context;

        public BrowserLoginPage(Context context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected override void DisplayLoginPage(Uri uri)
        {
            var browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(uri.AbsoluteUri));
            _context.StartActivity(browserIntent);
        }

        public bool UserHasLeft { get; private set; }

        public void UserHasLeftApplication()
        {
            UserHasLeft = true;
        }

        public override void CancelPendingLogin()
        {
            base.CancelPendingLogin();
            UserHasLeft = false;
        }

        public override bool FinishLogin(Uri redirectUri)
        {
            var finished = base.FinishLogin(redirectUri);
            UserHasLeft = false;
            return finished;
        }
    }
}
