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

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Qoden.Auth.OAuthLoginPage"/> class.
        /// </summary>
        /// <param name="returnUri">Expected Return URI</param>
        public OAuthLoginPage(string returnUri)
        {
            Assert.Argument(returnUri, nameof(returnUri)).NotEmpty();
            _returnUri = returnUri;
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
        /// <param name="redirectUri">OAuth redirect uri</param>
        public void OnOpenUrl(Uri redirectUri)
        {
            Assert.Argument(redirectUri, nameof(redirectUri)).NotNull();

            if (_task != null && redirectUri.AbsoluteUri.StartsWith(_returnUri, StringComparison.OrdinalIgnoreCase))
            {
                var query = HttpUtility.ParseQueryString(redirectUri.Query);
                _task.SetResult(query);
                _task = null;
                HideLoginPage();
            }
        }

        /// <summary>
        /// Notifies login page that control returned back to application and login process finished 
        /// </summary>
        public void OnAppActivated()
        {
            if (_task != null)
            {
                _task.SetCanceled();
                _task = null;
                HideLoginPage();
            }
        }
    }
}
