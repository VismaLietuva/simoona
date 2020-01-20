using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Shrooms.Authentification;
using Shrooms.Infrastructure.Configuration;

namespace Shrooms.API.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider, IOAuthAuthorizationServerProvider
    {
        private readonly string _jsAppClientId;
        private readonly string _mobileAppClientId;

        private readonly IApplicationSettings _appSettings;
        private readonly IContainer _ioc;

        public ApplicationOAuthProvider(IContainer ioc, IApplicationSettings appSettings, string jsAppClientId, string mobileAppClientId)
        {
            if (jsAppClientId == null || mobileAppClientId == null)
            {
                throw new ArgumentNullException("no client id provided");
            }

            _ioc = ioc;
            _appSettings = appSettings;
            _jsAppClientId = jsAppClientId;
            _mobileAppClientId = mobileAppClientId;
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            using (var requestScope = _ioc.BeginLifetimeScope("AutofacWebRequest"))
            {
                if (context.ClientId != _mobileAppClientId && context.ClientId != _jsAppClientId)
                {
                    context.SetError("unsupported_client");
                    return;
                }

                if (requestScope.Resolve(typeof(ShroomsUserManager)) is ShroomsUserManager userManager)
                {
                    var user = await userManager.FindAsync(context.UserName, context.Password);

                    if (user == null)
                    {
                        context.SetError("invalid_grant", "The user name or password is incorrect");

                        return;
                    }

                    if (!user.EmailConfirmed)
                    {
                        context.SetError("not_verified", "E-mail address is not verified");

                        return;
                    }

                    var identity = await userManager.CreateIdentityAsync(user, context.Options.AuthenticationType);
                    var properties = CreateProperties(user.Id, context.ClientId);
                    var ticket = new AuthenticationTicket(identity, properties);

                    context.Validated(ticket);
                }
            }
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId != _jsAppClientId)
            {
                return Task.FromResult<object>(null);
            }

            var redirectUriAuthority = new Uri(context.RedirectUri).GetLeftPart(UriPartial.Authority);
            var tenant = context.Request.Get<string>("tenantName");
            var authorizedUris = _appSettings.OAuthRedirectUris;

            var isValid = authorizedUris.Any(uri => new Uri(uri).GetLeftPart(UriPartial.Authority) == redirectUriAuthority);
            if (isValid || !_appSettings.IsProductionBuild)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task AuthorizationEndpointResponse(OAuthAuthorizationEndpointResponseContext context)
        {
            context.OwinContext.Authentication.AuthenticationResponseGrant.Properties.Dictionary.TryGetValue("refresh_token", out var refreshToken);

            if (!string.IsNullOrEmpty(refreshToken))
            {
                context.AdditionalResponseParameters.Add("refresh_token", refreshToken);
            }

            return base.AuthorizationEndpointResponse(context);
        }

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            var originalClient = context.Ticket.Properties.Dictionary["client_id"];
            var currentClient = context.ClientId;

            if (originalClient != currentClient)
            {
                context.SetError("invalid_clientId");
                return Task.FromResult<object>(null);
            }

            var newIdentity = new ClaimsIdentity(context.Ticket.Identity);
            var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
            context.Validated(newTicket);

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            if (!context.TryGetBasicCredentials(out var clientId, out _))
            {
                context.TryGetFormCredentials(out clientId, out _);
            }

            if (clientId != _jsAppClientId && clientId != _mobileAppClientId)
            {
                context.SetError("invalid_clientId");

                return Task.FromResult<object>(null);
            }

            context.OwinContext.Set("as:clientAllowedOrigin", "*");
            context.Validated(clientId);

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(string userId, string clientId)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "userIdentifier", userId },
                { "client_id", clientId }
            };
            return new AuthenticationProperties(data)
            {
                IsPersistent = true
            };
        }
    }
}