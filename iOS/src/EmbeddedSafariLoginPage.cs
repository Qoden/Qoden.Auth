using System;
using System.Threading.Tasks;
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
                    var display = _controller.PresentingViewController;
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
            UIViewController root = _root;
            if (root == null)
            {
                root = UIApplication.SharedApplication.KeyWindow.RootViewController;
            }

            if (root == null)
            {
                throw new InvalidOperationException("Cannot display login page - " +
                                                    "no root view controller specified and " +
                                                    "there is no displayed window or " +
                                                    "displayed window does not have root view controller");
            }

            //Find topmost modal dialog to present login page
            while (root.PresentedViewController != null && root != root.PresentedViewController && !root.PresentedViewController.IsBeingDismissed)
                root = root.PresentedViewController;

            _controller.ModalPresentationStyle = UIModalPresentationStyle.Popover;
            root.PresentViewController(_controller, true, null);
        }

        class LoginPageDelgate : SFSafariViewControllerDelegate
        {
            EmbeddedSafariLoginPage _page;

            public LoginPageDelgate(EmbeddedSafariLoginPage page)
            {
                _page = page;
            }

            public override async void DidFinish(SFSafariViewController controller)
            {
                var c = _page._controller;
                //Don't cancel pending login right away. Wait a little bit to 
                //give Safari a chance to hide itself. Otherwise login error 
                //handling code might present other modals and they might 
                //interfere with Safari and result in a broken dialog.
                await Task.Delay(10);
                if (c == _page._controller)
                {
                    _page._controller?.Dispose();
                    _page._controller = null;
                    _page.CancelPendingLogin();
                }
            }
        }
    }
}
