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
        bool IsEmailVerified(string provider, ClaimsPrincipal principal);
    }

    /// <summary>Facebook: always; Google: email_verified; Microsoft: only tenants in MicrosoftTrustedTenantIds, because
    /// any Entra tenant's administrator can give a user any address.</summary>
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

        /// <summary>The id_token "tid" claim, or null. Back-channel response: same trust as the access token, no re-validation.</summary>
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
