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
    }
}
