using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Shrooms.Infrastructure.Configuration
{
    public class ApplicationSettings : IApplicationSettings
    {
        public string StorageConnectionString => ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;

        public bool IsEmailEnabled => bool.Parse(ConfigurationManager.AppSettings["EmailEnabled"]);

        public bool IsProductionBuild
        {
            get
            {
                bool result;
                bool.TryParse(ConfigurationManager.AppSettings["IsProductionBuild"], out result);

                return result;
            }
        }

        public IEnumerable<string> OAuthRedirectUris => ConfigurationManager.AppSettings["OAuthRedirectUri"].Split(',');

        public string DemoAccountDefaultPictureId => ConfigurationManager.AppSettings["DemoAccountDefaultPictureID"];

        public string ClientUrl => string.Format(ConfigurationManager.AppSettings["ClientUrl"]);

        public string BasicUsername => ConfigurationManager.AppSettings["BasicUsername"];

        public string BasicPassword => ConfigurationManager.AppSettings["BasicPassword"];

        public string GoogleCalendarServiceId => ConfigurationManager.AppSettings["GoogleCalendarServiceId"];

        public string GoogleCalendarServiceCertThumb => ConfigurationManager.AppSettings["GoogleCalendarServiceCertThumb"];

        public StoreLocation GoogleCalendarCertStore => (StoreLocation)Enum.Parse(typeof(StoreLocation), ConfigurationManager.AppSettings["GoogleCalendarCertStore"]);

        public string GoogleCalendarServiceCertPassword => ConfigurationManager.AppSettings["GoogleCalendarServiceCertPassword"];

        public string GoogleCalendarServiceCertRelativePath => ConfigurationManager.AppSettings["GoogleCalendarServiceCertRelativePath"];

        public string CorsOriginsSetting => ConfigurationManager.AppSettings["CorsOriginsSettingKey"];

        public string SupportEmail => ConfigurationManager.AppSettings["SupportEmail"];

        public string VacationsBotAuthToken => ConfigurationManager.AppSettings["VacationsBotAuthToken"];

        public string VacationsBotHistoryUrl => ConfigurationManager.AppSettings["VacationsBotHistoryUrl"];

        public string ClientUrlWithOrg(string tenant) => $"{ClientUrl}/{tenant}";

        public string PictureUrl(string tenantPicturesContainer, string pictureName) => $"{ClientUrl}/api/storage/{tenantPicturesContainer.ToLowerInvariant()}/{pictureName}";

        public string WallPostUrl(string organization, int postId) => $"{ClientUrl}/{organization}/Wall/feed?post={postId}";

        public string UserNotificationSettingsUrl(string tenant) => $"{ClientUrl}/{tenant}/Settings/Notifications";

        public string UserProfileUrl(string tenant, string userId) => $"{ClientUrl}/{tenant}/profiles/{userId}";

        public string BookUrl(string tenant, int bookOfficeId, int officeId) => $"{ClientUrl}/{tenant}/Books/Edit/{bookOfficeId}/{officeId}";

        public string KudosProfileUrl(string tenant, string userId) => $"{ClientUrl}/{tenant}/Kudos/KudosUserInformation/{userId}";

        public string EventUrl(string tenant, string eventId) => $"{ClientUrl}/{tenant}/Events/EventContent/{eventId}";

        public string ProjectUrl(string tenant, string projectId) => $"{ClientUrl}/{tenant}/Projects/Details/{projectId}";

        public string CommitteeSugestionUrl(string tenant) => $"{ClientUrl}/{tenant}/Committees/List";

        public string ServiceRequestUrl(string tenant, int id) => $"{ClientUrl}/{tenant}/ServiceRequests/List?Id={id}";

        public string ResetPasswordUrl(string organization, string userName, string token) => $"{ClientUrl}/{organization}/Reset/{userName}/Token/{token}";

        public string VerifyEmailUrl(string organization, string userName, string token) => $"{ClientUrl}/{organization}/Verify/{userName}/Token/{token}";

        public string ApiUrl => string.Format(ConfigurationManager.AppSettings["ApiUrl"]);
    }
}