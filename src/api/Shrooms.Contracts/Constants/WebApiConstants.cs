namespace Shrooms.Contracts.Constants
{
    public static class WebApiConstants
    {
        public const int DefaultPageSize = 10;
        public const int OneHour = 3600;
        public const int OneDay = 3600 * 24;
        public const int FiveMinutes = 60 * 5;
        public const string PropertiesForUserDetails = "Manager,Room,Room.RoomType,Room.Floor,Room.Floor.Office,Projects,Certificates,WorkingHours,Skills,QualificationLevel,Exams,JobPosition,BlacklistEntries,BlacklistEntries.ModifiedByUser";
        public const string PropertiesForUserJobInfo = "Manager,Projects,Certificates,Certificates.Exams,WorkingHours,Skills,QualificationLevel,Exams";
        public const string PropertiesForUserOfficeInfo = "Room,Room.Floor";
        public const int DefaultAutocompleteListSize = 20;
        public static readonly string[] AbstractClassifierTypes = { "Project", "Language" };
        public static readonly char[] SearchSplitter = { ' ', ',', ';', '|' };
        public const int MonitorNameMaxLength = 50;
        public const int ProjectNameMaxLength = 100;

        public const int JobTitleMaxLength = 50;

        #region Language
        public const string DefaultLanguage = "en-US";
        public static readonly string[] SupportedLanguages = { "lt-LT", "en-US" };
        public const string LanguageCookieName = "UserLanguage";
        #endregion

        #region Pictures
        public const int MaximumPictureSizeInBytes = 12000000;
        #endregion

#if DEBUG
        public const string OrganizationManagerUsername = "Admin";
#else
        public const string OrganizationManagerUsername = "urbonman";
#endif

        #region Birthday
        public const int LowestBirthdayYear = 1900;
        #endregion

        #region Claims
        public const string ClaimOrganizationName = "OrganizationName";
        public const string ClaimOrganizationId = "OrganizationId";
        public const string ClaimUserImpersonation = "UserImpersonation";
        public const string ClaimOriginalUsername = "OriginalUsername";
        #endregion

        #region ApplicationUser
        public const int MaxPhoneNumberLength = 15;
        #endregion

        #region BlacklistState
        public const int DefaultBlacklistYearDuration = 2; 
        #endregion

        public const int WallNameMaxLength = 35;
        public const int WallDescMaxLength = 128;

        public const int KudosTypeNameMaxLength = 30;
    }
}