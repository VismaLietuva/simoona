using Autofac;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.Infrastructure;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.RefreshTokens;
using Shrooms.Domain.Services.RefreshTokens;
using Shrooms.Presentation.Api.Helpers;
using Shrooms.Presentation.Common.Helpers;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Shrooms.Presentation.Api.Providers
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
                var token = new RefreshTokenDto
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
                var userOrg = new UserAndOrganizationDto
                {
                    OrganizationId = context.Ticket.Identity.GetOrganizationId(),
                    UserId = context.Ticket.Identity.GetUserId()
                };

                if (tokenService != null)
                {
                    await tokenService.RemoveTokenBySubjectAsync(userOrg);
                    await tokenService.AddNewTokenAsync(token);
                }

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

                if (tokenService != null)
                {
                    var refreshToken = await tokenService.GetTokenTicketByIdAsync(hashedTokenId);

                    if (refreshToken != null)
                    {
                        context.DeserializeTicket(refreshToken.ProtectedTicket);
                        await tokenService.RemoveTokenByIdAsync(hashedTokenId);
                    }
                }
            }

            await Task.CompletedTask;
        }
    }
}
