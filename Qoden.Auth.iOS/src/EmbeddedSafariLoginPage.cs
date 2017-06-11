using System;
using SafariServices;
using UIKit;

namespace Qoden.Auth.iOS
{
    public class EmbeddedSafariLoginPage : OAuthLoginPage
    {
        private SFSafariViewController _controller;
        private UIViewController _root;

        public EmbeddedSafariLoginPage(string returnUri, UIViewController root = null) : base(returnUri)
        {
            _root = root;
        }

        protected override void DisplayLoginPage(Uri uri)
        {
            _controller = new SFSafariViewController(uri, false);
            _controller.Delegate = new SafariDelegate(this);

            UIViewController displayController = _root;
            if (displayController == null)
            {
                displayController = UIApplication.SharedApplication.KeyWindow.RootViewController;
            }

            if (displayController == null)
            {
                throw new InvalidOperationException("Cannot display login page - " +
                                                    "no root view controller specified and " +
                                                    "there is no displated windows or " +
                                                    "displayed window does not have root view controller");
            }

            displayController.PresentViewController(_controller, true, null);
        }

        protected override void HideLoginPage()
        {
            _controller.DismissViewController(true, null);
        }

        private class SafariDelegate : SFSafariViewControllerDelegate
        {
            private EmbeddedSafariLoginPage _page;

            public SafariDelegate(EmbeddedSafariLoginPage page)
            {
                _page = page;
            }

            public override void DidFinish(SFSafariViewController controller)
            {
                _page.OnAppActivated();
            }
        }
    }
}
