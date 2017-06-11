using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Qoden.Util;

namespace Qoden.Auth.Test
{
    public class FakeLoginPage : IOAuthLoginUI
    {
        private CancellationTokenSource _cancelTokens;
        public Uri Uri { get; private set; }

        public async Task<HttpValueCollection> Display(Uri uri)
        {
            Trace.WriteLine("Open Page " + uri.AbsoluteUri);
            Uri = uri;
            var http = new HttpClient(new HttpClientHandler()
            {
                AllowAutoRedirect = false
            });
            _cancelTokens = new CancellationTokenSource();
            var response = await http.GetAsync(uri, _cancelTokens.Token);
            var location = response.Headers.Location;
            var query = location.Query;
            if (query.StartsWith("?", StringComparison.Ordinal))
                query = query.Substring(1);
            return new HttpValueCollection(query);
        }

        public void Cancel()
        {
            if (_cancelTokens != null)
            {
                _cancelTokens.Cancel();
            }
        }
    }
}
