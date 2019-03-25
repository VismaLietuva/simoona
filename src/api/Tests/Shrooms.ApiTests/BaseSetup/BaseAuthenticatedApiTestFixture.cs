using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler;
using Shrooms.Constants.WebApi;
using System;
using System.Net.Http;
using System.Security.Claims;

namespace Shrooms.ApiTest.BaseSetup
{
    /// <summary>
    /// Base class for integration tests that require authentication.
    /// </summary>
    public abstract class BaseAuthenticatedApiTestFixture : BaseApiTestFixture
    {
        private string _token;

        /// <summary>
        /// Token for authenticated requests.
        /// </summary>
        protected virtual string Token
        {
            get { return _token ?? (_token = GenerateToken()); }
        }

        protected override HttpRequestMessage CreateRequest(HttpMethod method, object data)
        {
            var request = base.CreateRequest(method, data);
            if (!String.IsNullOrEmpty(this.Token))
            {
                request.Headers.Add("Authorization", "Bearer " + this.Token);
            }
            return request;
        }

        private string GenerateToken()
        {
            // Generate an OAuth bearer token for ASP.NET/Owin Web Api service that uses the default OAuthBearer token middleware.
            var claims = new[]
            {
                new Claim(ConstWebApi.ClaimOrganizationId, "2"),
                new Claim(ClaimTypes.NameIdentifier, "2d2a0c79-987f-42ea-821b-402dd345f3e6"),
                new Claim(ClaimTypes.Role, "User"),
            };
            var identity = new ClaimsIdentity(claims, "Test");

            // Use the same token generation logic as the OAuthBearer Owin middleware. 
            var tdf = new TicketDataFormat(this.DataProtector);
            var ticket = new AuthenticationTicket(identity, new AuthenticationProperties { ExpiresUtc = DateTime.UtcNow.AddHours(1) });
            var accessToken = tdf.Protect(ticket);

            return accessToken;
        }
    }
}