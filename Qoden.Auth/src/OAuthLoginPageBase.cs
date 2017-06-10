using System;
using System.Threading.Tasks;
using Qoden.Util;
using Qoden.Validation;

namespace Qoden.Auth
{
    /// <summary>
    /// Base class for login page adaptors
    /// </summary>
    public abstract class OAuthLoginPageBase : IOAuthLoginUI
    {
        private SingletonOperation<HttpValueCollection> loginOperation;
        private TaskCompletionSource<HttpValueCollection> loginTask;
        private OAuthConfig config;
        private Uri uri;

        public OAuthLoginPageBase(OAuthConfig config)
        {
            Assert.Argument(config, nameof(config)).NotNull();
            this.config = config;
            loginOperation = new SingletonOperation<HttpValueCollection>(() => OpenLoginPage());
        }

        public Task<HttpValueCollection> GetResult(Uri uri)
        {
            Assert.Argument(uri, nameof(uri)).NotNull();
            Assert.State(uri, nameof(uri))
                          .NotEqualsTo(this.uri, "Login page with different uri already opened");
            this.uri = uri;
            return loginOperation.Start();
        }

        private Task<HttpValueCollection> OpenLoginPage()
        {
            loginTask = new TaskCompletionSource<HttpValueCollection>();
            OpenLoginPage(uri);
            return loginTask.Task;
        }

        protected abstract void OpenLoginPage(Uri uri);

        /// <summary>
        /// Notifies login page that system received redirect uri.
        /// </summary>
        /// <param name="returnUrl">OAuth redirect uri</param>
        public void NotifyOpenUrl(Uri returnUrl)
        {
            Assert.Argument(returnUrl, nameof(returnUrl)).NotNull();

            if (!loginOperation.Started) return;

            if (returnUrl.AbsoluteUri.StartsWith(config.ReturnUrl, StringComparison.Ordinal))
            {
                var query = HttpUtility.ParseQueryString(returnUrl.Query);
                uri = null;
                loginTask.SetResult(query);
            }
        }
    }
}
