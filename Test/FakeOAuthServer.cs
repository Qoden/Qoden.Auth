using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Qoden.Auth.Test
{
    public class FakeOAuthServer : IDisposable
    {
        private IWebHost _host;

        public Uri BaseUri => new Uri("http://localhost:9987");

        public FakeOAuthServer()
        {
            _host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(BaseUri.AbsoluteUri)
                .ConfigureServices(s => s.AddRouting())
                .Configure(app =>
                {
                    app.UseRouter(router =>
					{
                        router.MapGet("authorize", c => Authorize(c));
                        router.MapPost("token", c => Token(c));
					});
                })
                .Build();
            _host.Start();
            //var server = new Thread(_host.Run);
            //server.Start();
        }

        public HttpClient CreateClient()
        {
            return new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:9987")
            };
        }

        public RequestDelegate Authorize { get; set; }

        public RequestDelegate Token { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (_host != null)
            {
                if (disposing)
                {
                    _host.Dispose();
                }
                _host = null;
            }
        }

        ~FakeOAuthServer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
