using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Shrooms.Presentation.Api.Helpers
{
    public interface IReturnUrlValidator
    {
        /// <summary>True when the URL's origin is a configured client origin; the access token is redirected there.</summary>
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

            if (!string.IsNullOrEmpty(uri.Fragment) || !string.IsNullOrEmpty(uri.UserInfo))
            {
                return false;
            }

            return _allowedOrigins.Contains(uri.GetLeftPart(UriPartial.Authority));
        }

        // ClientUrl, CorsOrigins and AllowedReturnUrlOrigins; "*" contributes nothing.
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
