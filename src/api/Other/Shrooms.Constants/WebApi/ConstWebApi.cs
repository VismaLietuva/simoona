namespace Shrooms.Constants.WebApi
{
    public static class ConstWebApi
    {
        public const int DefaultPageSize = 10;
        public const int OneHour = 3600;
        public const string PropertiesForUserDetails = "Manager,Room,Room.RoomType,Room.Floor,Room.Floor.Office,Projects,Certificates,WorkingHours,Skills,QualificationLevel,Exams,JobPosition";
        public const string PropertiesForUserJobInfo = "Manager,Projects,Certificates,Certificates.Exams,WorkingHours,Skills,QualificationLevel,Exams";
        public const string PropertiesForUserOfficeInfo = "Room,Room.Floor";
        public const string ServiceRequestsUrl = "ServiceRequests/List";
        public const int DefaultAutocompleteListSize = 20;
        public static string[] AbstractClassifierTypes = new string[] { "Project", "Language" };
        public static readonly char[] SearchSplitter = new char[] { ' ', ',', ';', '|' };
        public const int MonitorNameMaxLength = 50;
        public const int ProjectNameMaxLength = 100;

        public const int EventTypeNameMaxLength = 30;
        public const int JobTitleMaxLength = 50;

        #region language
        public const string DefaultLanguage = "en-US";
        public static string[] SupportedLanguages = { "lt-LT", "en-US" };
        public const string LanguageCookieName = "UserLanguage";
        #endregion

        #region pictures
        public const int MaximumPictureSizeInBytes = 12000000;
        #endregion

#if DEBUG
        public const string OrganizationManagerUsername = "Admin";
#else
        public const string OrganizationManagerUsername = "urbonman";
#endif

        #region birthday
        public const int LowestBirthdayYear = 1900;
        #endregion

        #region claims
        public const string ClaimOrganizationName = "OrganizationName";
        public const string ClaimOrganizationId = "OrganizationId";
        public const string ClaimUserImpersonation = "UserImpersonation";
        public const string ClaimOriginalUsername = "OriginalUsername";
        #endregion

        #region Events
        public const int EventNameMaxLength = 35;
        public const int EventMinimumParticipants = 0;
        public const int EventMaxParticipants = 1000;
        public const int EventLocationMaxLength = 50;
        public const int EventDescriptionMaxLength = 5000;
        public const int EventMinimumOptions = 0;
        #endregion

        #region ApplicationUser
        public const int MaxPhoneNumberLength = 15;
        #endregion

        public const int WallNameMaxLength = 35;
        public const int WallDescMaxLength = 128;

        public const int ServiceRequestCategoryNameMaxLength = 30;
        public const int KudosTypeNameMaxLength = 30;
    }
}