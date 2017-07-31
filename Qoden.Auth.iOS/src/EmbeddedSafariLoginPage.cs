using System;
using Qoden.Validation;
using SafariServices;
using UIKit;

namespace Qoden.Auth.iOS
{
    public class EmbeddedSafariLoginPage : OAuthLoginPage
    {
        private SFSafariViewController _controller;
        private UIViewController _root;

        public EmbeddedSafariLoginPage(UIViewController root = null)
        {
            _root = root;
        }

        public override bool FinishLogin(Uri redirectUri)
        {
            var processed = base.FinishLogin(redirectUri);
            if (processed)
            {
                if (_controller != null)
                {
                    var controller = _controller;
                    _controller = null;
                    controller.DismissViewController(true, ()=>
                    {
                        controller.Dispose();
                    });
                }
            }
            return processed;
        }

        protected override void DisplayLoginPage(Uri uri)
        {
            Assert.State(_controller).IsNull("Login page still displaying another page");

            _controller = new SFSafariViewController(uri, false);
            _controller.Delegate = new LoginPageDelgate(this);
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

        class LoginPageDelgate : SFSafariViewControllerDelegate
        {
            EmbeddedSafariLoginPage _page;

            public LoginPageDelgate(EmbeddedSafariLoginPage page)
            {
                _page = page;
            }

            public override void DidFinish(SFSafariViewController controller)
            {
                _page._controller = null;
                _page.CancelPendingLogin();
            }
        }
    }
}
