using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Qoden.Auth.Test
{
    [TestClass]
    public class CustodianTest : IDisposable
    {
        private FakeOAuthServer _oauthServer;
        private OAuthApi _api;
        private FakeLoginPage _loginPage;
        private int _tokensRequested;

        public CustodianTest()
        {
            _oauthServer = new FakeOAuthServer();
            var _config = new OAuthConfig
            {
                ClientId = "Test Client",
                ReturnUrl = "http://localhost/return_uri"
            };
            _api = new OAuthApi(_config,
                                   new Uri(_oauthServer.BaseUri, "token"),
                                   new Uri(_oauthServer.BaseUri, "authorize"));
            _loginPage = new FakeLoginPage();
            AbstractPlatform.Instance = new TestPlatform();
            _tokensRequested = 0;
        }

        public void Dispose()
        {
            _oauthServer.Dispose();
        }

        [TestMethod]
        public async Task GrantCodeFlow()
        {
            Custodian custodian = GrantCodeCustodian();
            var user = await custodian.Authenticate();

            Assert.AreEqual("access token 1", user[OAuthApi.AccessToken]);
            Assert.AreEqual("refresh token 1", user[OAuthApi.RefreshToken]);
            Assert.AreEqual("test_grant_code", user["request_code"]);
            Assert.AreEqual("authorization_code", user["request_grant_type"]);
        }

        [TestMethod]
        public async Task ClientFlow()
        {
            var custodian = IdTokenCustodian();
            var user = await custodian.Authenticate();

            Assert.AreEqual("test_id_token", user[OAuthApi.IdToken]);
            Assert.AreEqual(1, _tokensRequested, 
                            "Client flow expect to get token right from authorization page");
        }

        [TestMethod]
        public async Task MultipleAuthRequests()
        {
            Custodian custodian = IdTokenCustodian();
            var a1 = custodian.Authenticate();
            var a2 = custodian.Authenticate();
            await Task.WhenAll(a1, a2);
            Assert.AreSame(a1.Result, a2.Result);
            Assert.AreEqual(1, _tokensRequested,
                            "Parallel authentications wait for first one and reuse it results");
        }

        [TestMethod]
        public async Task UseCache()
        {
            var custodian = GrantCodeCustodian();
            var u1 = await custodian.Authenticate();
            var u2 = await custodian.Authenticate();
            Assert.AreEqual(1, _tokensRequested,
                           "Subsequent authentications use cache and does not call server");
        }

        [TestMethod]
        public async Task ForceDoesNotUseCache()
        {
            var custodian = GrantCodeCustodian();
            var u1 = await custodian.Authenticate();
            var u2 = await custodian.Authenticate(true);
            Assert.AreEqual(2, _tokensRequested,
                           "Forced authentication calls server even when there is cached profile");
            Assert.AreEqual("access token 2", u2[OAuthApi.AccessToken]);
        }

        [TestMethod]
        public async Task ForceDoesNotJoinsNonForce()
        {
            var custodian = GrantCodeCustodian();
            var u1 = custodian.Authenticate();
            var u2 = custodian.Authenticate(true);
            await Task.WhenAll(u1, u2);
            Assert.AreEqual(2, _tokensRequested, 
                            "Forced authentication does not join normal authentication and happens right after it");
            Assert.AreEqual("access token 2", u2.Result[OAuthApi.AccessToken]);
        }

        [TestMethod]
        public async Task ForceJoinsForce()
        {
            var custodian = GrantCodeCustodian();
            var u1 = custodian.Authenticate(true);
            var u2 = custodian.Authenticate(true);
            await Task.WhenAll(u1,   u2);
            Assert.AreEqual(1, _tokensRequested,
                           "Forced authentication joins another forced authentication if it is already running");
        }

        [TestMethod]
        public async Task OAuthError()
        {
            var custodian = GrantCodeCustodian();
            _oauthServer.Token = InternalError;
            await Assert.ThrowsExceptionAsync<OAuthException>(() => custodian.Authenticate());
        }

        [TestMethod]
        public async Task EveryCallerGetsItOwnError()
        {
            var custodian = GrantCodeCustodian();
            _oauthServer.Token = InternalError;
            var u1 = Assert.ThrowsExceptionAsync<OAuthException>(() => custodian.Authenticate());
            var u2 = Assert.ThrowsExceptionAsync<OAuthException>(() => custodian.Authenticate());
            await Task.WhenAll(u1, u2);

            Assert.AreEqual(1, _tokensRequested);
        }

        [TestMethod]
        public async Task UserCanceLogin()
        {
            var custodian = GrantCodeCustodian();
            var authTask = custodian.Authenticate();
            _loginPage.Cancel();
            await Assert.ThrowsExceptionAsync<OAuthException>(() => authTask);
        }

        private Custodian GrantCodeCustodian()
        {
            _oauthServer.Authorize = AuthorizeGrantCode;
            _oauthServer.Token = ReplyWithAccessToken;
            var custodian = new Custodian(new OAuthGrantCodeFlow(_api, _loginPage));
            return custodian;
        }

        private Custodian IdTokenCustodian()
        {
            _oauthServer.Authorize = AuthorizeIdToken;
            return new Custodian(new OAuthClientSideFlow(_api, _loginPage));
        }

        private Task AuthorizeIdToken(HttpContext context)
        {
            _tokensRequested++;
            Trace.WriteLine("ReplyWithIdToken");
            var redirect = new Uri(new Uri(_api.Config.ReturnUrl), "?id_token=test_id_token");
            context.Response.Redirect(redirect.AbsoluteUri);
            return context.Response.WriteAsync("");
        }

        private Task AuthorizeGrantCode(HttpContext context)
        {
            Trace.WriteLine("ReplyWithGrantCode");
            var redirect = new Uri(new Uri(_api.Config.ReturnUrl), "?code=test_grant_code");
            context.Response.Redirect(redirect.AbsoluteUri);
            return context.Response.WriteAsync("");
        }

        private Task ReplyWithAccessToken(HttpContext context)
        {
            _tokensRequested++;
            var request = context.Request;
            Trace.WriteLine("ReplyWithAccessToken");
            var response = new Dictionary<string, string>()
            {
                {OAuthApi.AccessToken, "access token " + _tokensRequested},
                {OAuthApi.RefreshToken, "refresh token " + _tokensRequested},
                {OAuthApi.ExpiresIn, "3600"}
            };
            foreach (var kv in request.Form)
            {
                response["request_" + kv.Key] = kv.Value;
            }            
            return context.Response.WriteJson(response);
        }

        private Task InternalError(HttpContext context)
        {
            _tokensRequested++;
            Trace.WriteLine("InternalError");
            context.Response.StatusCode = 500;
            return context.Response.WriteAsync("");
        }

        public class TestPlatform : AbstractPlatform
        {
            public override ISecureStore CreateAccountStore()
            {
                return new FakeSecureSore();
            }
        }
    }
}
