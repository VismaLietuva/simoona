using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Configuration;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;

namespace Shrooms.Infrastructure.Configuration
{
    public class ApplicationSettings : IApplicationSettings
    {
        private const string ClientLocale = "en";

        private readonly IConfiguration _configuration;

        public ApplicationSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string StorageConnectionString => _configuration.GetConnectionString("StorageConnectionString") ?? string.Empty;

        public bool IsEmailEnabled => bool.TryParse(_configuration["EmailEnabled"], out var val) && val;

        public EmailBuildingStrategy EmailBuildingStrategy =>
            Enum.TryParse(_configuration["EmailBuildingStrategy"], out EmailBuildingStrategy strat) ? strat : EmailBuildingStrategy.AllTo;

        public int DefaultOrganizationId => int.TryParse(_configuration["DefaultOrganizationId"], out var id) ? id : 1;

        public int AccessTokenLifeTimeInHours => int.TryParse(_configuration["AccessTokenLifeTimeInHours"], out var h) ? h : 24;

        public int? KudosAvailableToSendPerMonth => int.TryParse(_configuration["KudosAvailableToSendPerMonth"], out var k) ? k : null;

        public bool IsProductionBuild => bool.TryParse(_configuration["IsProductionBuild"], out var result) && result;

        public IEnumerable<string> OAuthRedirectUris => (_configuration["OAuthRedirectUri"] ?? string.Empty).Split(',');

        public string DemoAccountDefaultPictureId => _configuration["DemoAccountDefaultPictureID"] ?? string.Empty;

        public string ClientUrl => _configuration["ClientUrl"] ?? string.Empty;

        public string BasicUsername => _configuration["BasicUsername"] ?? string.Empty;

        public string BasicPassword => _configuration["BasicPassword"] ?? string.Empty;

        public string CorsOriginsSetting => _configuration["CorsOriginsSettingKey"] ?? string.Empty;

        public string SupportEmail => _configuration["SupportEmail"] ?? string.Empty;

        public string VacationsBotAuthToken => _configuration["VacationsBotAuthToken"] ?? string.Empty;

        public string VacationsBotHistoryUrl => _configuration["VacationsBotHistoryUrl"] ?? string.Empty;

        public string ApiUrl => _configuration["ApiUrl"] ?? string.Empty;

        public string PictureUrl(string tenantPicturesContainer, string pictureName) =>
            $"{ApiUrl.TrimEnd('/')}/storage/{tenantPicturesContainer.ToLowerInvariant()}/{pictureName}";

        public string WallPostUrl(string organization, int postId) => GetClientPath($"posts/{postId}");

        public string UserNotificationSettingsUrl(string tenant) => GetClientPath("settings/notifications");

        public string UserProfileUrl(string tenant, string userId) => GetClientPath($"profile/{userId}");

        public string GroupUrl(string tenant, int groupId) => GetClientPath($"groups/{groupId}");

        // The client resolves the office from the book itself, so officeId stays out of the path.
        public string BookUrl(string tenant, int bookOfficeId, int officeId) => GetClientPath($"books/{bookOfficeId}");

        public string KudosProfileUrl(string tenant, string userId) => GetClientPath($"kudos?userId={WebUtility.UrlEncode(userId)}");

        public string EventUrl(string tenant, string eventId) => GetClientPath($"events/{eventId}");

        public string EventListByTypeUrl(string tenant, string eventTypeId) => GetClientPath($"events?typeId={WebUtility.UrlEncode(eventTypeId)}");

        public string ProjectUrl(string tenant, string projectId) => GetClientPath($"projects/{projectId}");

        // Committees are a group type on the client, so the suggestion lands on the group list.
        public string CommitteeSugestionUrl(string tenant) => GetClientPath("groups");

        public string ServiceRequestUrl(string tenant, int id) => GetClientPath($"service-requests/{id}/edit");

        public string ResetPasswordUrl(string organization, string userName, string token) =>
            GetClientPath($"reset-password?UserName={WebUtility.UrlEncode(userName)}&Token={WebUtility.UrlEncode(token)}&org={WebUtility.UrlEncode(organization)}");

        public string VerifyEmailUrl(string organization, string userName, string token) =>
            GetClientPath($"verify-email?UserName={WebUtility.UrlEncode(userName)}&Token={WebUtility.UrlEncode(token)}&org={WebUtility.UrlEncode(organization)}");

        public string FeedUrl(string tenant) => GetClientPath(string.Empty);

        // Emails are English-only, so every client link is pinned to the en locale
        // rather than threading a recipient locale through the notification services.
        private string GetClientPath(string relativePath)
        {
            var baseUrl = ClientUrl.TrimEnd('/');
            return $"{baseUrl}/{ClientLocale}/{relativePath}";
        }
    }
}