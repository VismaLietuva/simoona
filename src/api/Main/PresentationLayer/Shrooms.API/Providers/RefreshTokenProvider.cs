using System;
using System.Configuration;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.Infrastructure;
using Shrooms.API.Helpers;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.RefreshTokens;
using Shrooms.Domain.Services.RefreshTokens;

namespace Shrooms.API.Providers
{
    public class RefreshTokenProvider : IAuthenticationTokenProvider
    {
        private readonly IContainer _ioc;

        public RefreshTokenProvider(IContainer ioc)
        {
            _ioc = ioc;
        }

        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            using (var requestScope = _ioc.BeginLifetimeScope("AutofacWebRequest"))
            {
                var tokenService = requestScope.Resolve(typeof(IRefreshTokenService)) as IRefreshTokenService;
                var clientId = context.Ticket.Properties.Dictionary["client_id"];

                if (string.IsNullOrEmpty(clientId))
                {
                    await Task.CompletedTask;
                }

                var refreshTokenId = Guid.NewGuid().ToString("n");

                var refreshTokenLifeTimeInDays = Convert.ToInt16(ConfigurationManager.AppSettings["RefreshTokenLifeTimeInDays"]);
                var token = new RefreshTokenDTO
                {
                    Id = CryptoHelper.GetHash(refreshTokenId),
                    Subject = context.Ticket.Identity.GetUserId(),
                    IssuedUtc = DateTime.UtcNow,
                    ExpiresUtc = DateTime.UtcNow.AddDays(refreshTokenLifeTimeInDays),
                    OrganizationId = context.Ticket.Identity.GetOrganizationId()
                };

                context.Ticket.Properties.IssuedUtc = token.IssuedUtc;
                context.Ticket.Properties.ExpiresUtc = token.ExpiresUtc;

                token.ProtectedTicket = context.SerializeTicket();
                var userOrg = new UserAndOrganizationDTO
                {
                    OrganizationId = context.Ticket.Identity.GetOrganizationId(),
                    UserId = context.Ticket.Identity.GetUserId()
                };

                tokenService?.RemoveTokenBySubject(userOrg);
                tokenService?.AddNewToken(token);

                context.SetToken(refreshTokenId);
            }

            await Task.CompletedTask;
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            using (var requestScope = _ioc.BeginLifetimeScope("AutofacWebRequest"))
            {
                var tokenService = requestScope.Resolve(typeof(IRefreshTokenService)) as IRefreshTokenService;
                const string corsHeaderName = "Access-Control-Allow-Origin";
                var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");
                if (!context.OwinContext.Response.Headers.ContainsKey(corsHeaderName))
                {
                    context.OwinContext.Response.Headers.Add(corsHeaderName, new[] { allowedOrigin });
                }

                var hashedTokenId = CryptoHelper.GetHash(context.Token);
                var refreshToken = tokenService?.GetTokenTicketById(hashedTokenId);

                if (refreshToken != null)
                {
                    context.DeserializeTicket(refreshToken.ProtectedTicket);
                    tokenService.RemoveTokenById(hashedTokenId);
                }
            }

            await Task.CompletedTask;
        }
    }
}