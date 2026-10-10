using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Shrooms.Contracts.Constants;

namespace Shrooms.Presentation.Api.Helpers
{
    public interface IExternalEmailTrust
    {
        /// <summary>
        /// True when the identity provider vouches for the principal's email address, so the address may be
        /// used to match an existing account or to register a new one.
        /// </summary>
        bool IsEmailVerified(string provider, ClaimsPrincipal principal);
    }

    /// <summary>
    /// Facebook only returns confirmed addresses. Google states verification explicitly (email_verified).
    /// Microsoft's email comes from the Graph profile (mail / userPrincipalName), which the administrator of
    /// whichever Entra tenant the account lives in sets freely, so it is trusted only for tenants this
    /// deployment lists in MicrosoftTrustedTenantIds (the tenant id travels in the id_token "tid" claim, copied
    /// onto the principal at sign-in). Anyone can create an Entra tenant and give a user any address.
    /// </summary>
    public class ExternalEmailTrust : IExternalEmailTrust
    {
        public const string MicrosoftTenantClaim = "tid";
        public const string TrustedTenantsSetting = "MicrosoftTrustedTenantIds";

        private readonly HashSet<string> _trustedMicrosoftTenants;

        public ExternalEmailTrust(IEnumerable<string> trustedMicrosoftTenantIds)
        {
            _trustedMicrosoftTenants = new HashSet<string>(
                (trustedMicrosoftTenantIds ?? Enumerable.Empty<string>()).Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()),
                StringComparer.OrdinalIgnoreCase);
        }

        public static ExternalEmailTrust FromConfiguration(IConfiguration configuration)
        {
            return new ExternalEmailTrust(SplitList(configuration[TrustedTenantsSetting]));
        }

        public bool HasTrustedMicrosoftTenants => _trustedMicrosoftTenants.Count > 0;

        public bool IsEmailVerified(string provider, ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                return false;
            }

            if (provider == AuthenticationConstants.FacebookLoginProvider)
            {
                return true;
            }

            if (provider == AuthenticationConstants.GoogleLoginProvider)
            {
                return string.Equals(principal.FindFirstValue("email_verified"), "true", StringComparison.OrdinalIgnoreCase);
            }

            if (provider == AuthenticationConstants.MicrosoftLoginProvider)
            {
                var tenantId = principal.FindFirstValue(MicrosoftTenantClaim);
                return !string.IsNullOrEmpty(tenantId) && _trustedMicrosoftTenants.Contains(tenantId);
            }

            return false;
        }

        /// <summary>
        /// The "tid" claim of the id_token in an OAuth token response, or null. The response arrived over the
        /// back channel in exchange for the client secret, the same trust the access token already gets, so the
        /// signature is not re-validated here.
        /// </summary>
        public static string ReadMicrosoftTenantId(JsonElement tokenResponse)
        {
            if (tokenResponse.ValueKind != JsonValueKind.Object
                || !tokenResponse.TryGetProperty("id_token", out var idToken)
                || idToken.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            var parts = idToken.GetString()?.Split('.');
            if (parts == null || parts.Length < 2)
            {
                return null;
            }

            try
            {
                var payload = parts[1].Replace('-', '+').Replace('_', '/');
                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                using var document = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
                return document.RootElement.TryGetProperty("tid", out var tid) && tid.ValueKind == JsonValueKind.String ? tid.GetString() : null;
            }
            catch (Exception ex) when (ex is FormatException || ex is JsonException)
            {
                return null;
            }
        }

        private static IEnumerable<string> SplitList(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? Enumerable.Empty<string>()
                : value.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
    }
}
