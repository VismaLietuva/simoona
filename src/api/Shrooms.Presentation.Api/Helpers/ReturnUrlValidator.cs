using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Shrooms.Presentation.Api.Helpers
{
    public interface IReturnUrlValidator
    {
        /// <summary>
        /// True when <paramref name="returnUrl"/> is an absolute http(s) URL whose origin (scheme, host, port)
        /// is one of the configured client origins. Used before redirecting a freshly issued access token
        /// to it, so an attacker-supplied returnUrl cannot receive the token.
        /// </summary>
        bool IsAllowed(string returnUrl);
    }

    public class ReturnUrlValidator : IReturnUrlValidator
    {
        private readonly HashSet<string> _allowedOrigins;

        public ReturnUrlValidator(IConfiguration configuration)
            : this(CollectOrigins(configuration["ClientUrl"], configuration["CorsOrigins"], configuration["AllowedReturnUrlOrigins"]))
        {
        }

        public ReturnUrlValidator(IEnumerable<string> allowedOrigins)
        {
            _allowedOrigins = new HashSet<string>(
                allowedOrigins.Select(NormalizeOrigin).Where(o => o != null),
                StringComparer.OrdinalIgnoreCase);
        }

        public bool IsAllowed(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl) || returnUrl.Length > 2048)
            {
                return false;
            }

            if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
            {
                return false;
            }

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                return false;
            }

            // The token is appended as a fragment; a returnUrl that already carries one, or that embeds
            // credentials, is never something the client sends.
            if (!string.IsNullOrEmpty(uri.Fragment) || !string.IsNullOrEmpty(uri.UserInfo))
            {
                return false;
            }

            return _allowedOrigins.Contains(uri.GetLeftPart(UriPartial.Authority));
        }

        // ClientUrl is the primary client; CorsOrigins already lists every origin the API is meant to talk
        // to; AllowedReturnUrlOrigins is an optional extra list. "*" contributes nothing on purpose.
        private static IEnumerable<string> CollectOrigins(string clientUrl, string corsOrigins, string extraOrigins)
        {
            yield return clientUrl;

            foreach (var list in new[] { corsOrigins, extraOrigins })
            {
                if (string.IsNullOrWhiteSpace(list) || list.Trim() == "*")
                {
                    continue;
                }

                foreach (var origin in list.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    yield return origin;
                }
            }
        }

        private static string NormalizeOrigin(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri))
            {
                return null;
            }

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                return null;
            }

            return uri.GetLeftPart(UriPartial.Authority);
        }
    }
}
