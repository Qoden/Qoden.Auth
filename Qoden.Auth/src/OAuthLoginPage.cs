using System;
using System.Threading.Tasks;
using Qoden.Util;
using Qoden.Validation;

namespace Qoden.Auth
{
    /// <summary>
    /// Base class for OAuth login page adaptors
    /// </summary>
    public abstract class OAuthLoginPage : IOAuthLoginUI
    {
        private TaskCompletionSource<HttpValueCollection> _task;
        private string _returnUri;
        private bool _hasLeft = false;

        public string ExpectedRedirectUriPrefix
        {
            get { return _returnUri; }
            set
            {
                Assert.State(_task).IsNull("Cannot change return url while login page running");
                _returnUri = value;
            }
        }

        public Task<HttpValueCollection> Display(Uri uri)
        {
            Assert.Argument(uri, nameof(uri)).NotNull();
            if (_task != null)
            {
                _task.SetCanceled();
                HideLoginPage();
            }
            _task = new TaskCompletionSource<HttpValueCollection>();
            DisplayLoginPage(uri);
            return _task.Task;
        }

        protected abstract void DisplayLoginPage(Uri uri);
        protected abstract void HideLoginPage();

        /// <summary>
        /// Notifies login page that system received redirect uri.
        /// </summary>
        /// <remarks>
        /// Method does nothing if page is not opened or if provided URL does not match ExpectedRedirectUriPrefix.
        /// </remarks>
        /// <param name="redirectUri">OAuth redirect uri</param>
        /// <returns>true is page handled redirectUri or false otherwise</returns>
        public bool UserOpenedUrl(Uri redirectUri)
        {
            Assert.Argument(redirectUri, nameof(redirectUri)).NotNull();

            if (_task != null && 
                (ExpectedRedirectUriPrefix == null || redirectUri.AbsoluteUri.StartsWith(ExpectedRedirectUriPrefix, StringComparison.OrdinalIgnoreCase)))
            {
                var query = HttpUtility.ParseQueryString(redirectUri.Query);
                _task.SetResult(query);
                _task = null;
                HideLoginPage();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Notifies login page that user returned back to application.
        /// </summary>
        public virtual void UserActivatedApplication()
        {
            if (_task != null && _hasLeft)
            {
                _task.SetCanceled();
                _task = null;
                HideLoginPage();
            }
            _hasLeft = false;
        }

        /// <summary>
        /// Notifies login page that user has left application.
        /// </summary>
        public virtual void UserHasLeftApplication()
        {
            _hasLeft = true;
        }
    }
}
