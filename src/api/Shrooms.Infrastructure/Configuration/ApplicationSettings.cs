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

        public string ClientUrlWithOrg(string tenant) => GetClientPath(tenant);

        public string PictureUrl(string tenantPicturesContainer, string pictureName) => GetClientPath($"api/storage/{tenantPicturesContainer.ToLowerInvariant()}/{pictureName}");

        public string WallPostUrl(string organization, int postId) => GetClientPath($"{organization}/Wall/feed?post={postId}");

        public string UserNotificationSettingsUrl(string tenant) => GetClientPath($"{tenant}/Settings/Notifications");

        public string UserProfileUrl(string tenant, string userId) => GetClientPath($"{tenant}/profiles/{userId}");

        public string GroupUrl(string tenant, int groupId) => GetClientPath($"{tenant}/Groups/{groupId}");

        public string BookUrl(string tenant, int bookOfficeId, int officeId) => GetClientPath($"{tenant}/Books/Edit/{bookOfficeId}/{officeId}");

        public string KudosProfileUrl(string tenant, string userId) => GetClientPath($"{tenant}/Kudos/KudosUserInformation/{userId}");

        public string EventUrl(string tenant, string eventId) => GetClientPath($"{tenant}/Events/EventContent/{eventId}");

        public string EventListByTypeUrl(string tenant, string eventTypeId) => GetClientPath($"{tenant}/Events/List/{eventTypeId}/office/all");

        public string ProjectUrl(string tenant, string projectId) => GetClientPath($"{tenant}/Projects/Details/{projectId}");

        public string CommitteeSugestionUrl(string tenant) => GetClientPath($"{tenant}/Committees/List");

        public string ServiceRequestUrl(string tenant, int id) => GetClientPath($"{tenant}/ServiceRequests/List?Id={id}");

        public string ResetPasswordUrl(string organization, string userName, string token) => GetClientPath($"{organization}/Reset?UserName={WebUtility.UrlEncode(userName)}&Token={WebUtility.UrlEncode(token)}");

        public string VerifyEmailUrl(string organization, string userName, string token) => GetClientPath($"{organization}/Verify?UserName={WebUtility.UrlEncode(userName)}&Token={WebUtility.UrlEncode(token)}");

        public string FeedUrl(string tenant) => GetClientPath($"{tenant}/Wall/Feed");

        private string GetClientPath(string relativePath)
        {
            var baseUrl = ClientUrl.TrimEnd('/');
            return $"{baseUrl}/{relativePath}";
        }
    }
}