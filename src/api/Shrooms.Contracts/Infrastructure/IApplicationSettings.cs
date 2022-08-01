using System.Collections.Generic;

namespace Shrooms.Contracts.Infrastructure
{
    public interface IApplicationSettings
    {
        string StorageConnectionString { get; }

        bool IsEmailEnabled { get; }

        int DefaultOrganizationId { get; }

        bool IsProductionBuild { get; }

        string DemoAccountDefaultPictureId { get; }

        string BasicUsername { get; }

        string BasicPassword { get; }

        string CorsOriginsSetting { get; }

        string SupportEmail { get; }

        string VacationsBotAuthToken { get; }

        string VacationsBotHistoryUrl { get; }

        string ClientUrl { get; }

        IEnumerable<string> OAuthRedirectUris { get; }

        string ApiUrl { get; }

        string PictureUrl(string tenantPicturesContainer, string pictureName);

        string WallPostUrl(string organization, int postId);

        string UserNotificationSettingsUrl(string tenant);

        string UserProfileUrl(string tenant, string id);

        string BookUrl(string tenant, int bookOfficeId, int officeId);

        string KudosProfileUrl(string tenant, string userId);

        string EventUrl(string tenant, string eventId);

        string EventListByTypeUrl(string tenant, string eventId);

        string ProjectUrl(string tenant, string projectId);

        string CommitteeSugestionUrl(string tenant);

        string ServiceRequestUrl(string tenant, int id);

        string ResetPasswordUrl(string organization, string userName, string token);

        string VerifyEmailUrl(string organization, string userName, string token);

        string FeedUrl(string tenant);
    }
}