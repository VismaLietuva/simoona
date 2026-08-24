namespace Shrooms.Contracts.Constants
{
    public static class ErrorCodes
    {
        // General
        public const int DuplicatesIntolerable = 600;
        public const int UserNotFound = 601;
        public const int ContentDoesNotExist = 602;
        public const int ConcurrencyError = 603;
        public const int InvalidOrganization = 604;

        // Books, 1**
        public const string BookNotFoundByExternalProviderCode = "100";
        public const string BookAlreadyExistsCode = "101";
        public const string BoolAllQuantitiesAreZeroCode = "102";

        // Kudos, 3**
        public const int CanNotSendKudosToSelf = 302;
        public const int KudosTypeNotFound = 303;
        public const int InsufficientKudos = 304;
        public const int SenderReceiverCannotAcceptRejectKudos = 305;
        public const int KudosAlreadyApproved = 306;
        public const int KudosTypeAlreadyDisabled = 307;
        public const int KudosTypeNameAlreadyExists = 308;

        // Wall, 4**
        public const int WallPostNotFoundCode = 402;
        public const int WallNotFound = 404;
        public const int WallNameAlreadyExists = 405;
        public const int WallModeratorCanNotLeave = 406;
        public const int WallCannotLeaveMain = 407;

        // Post, 5**
        public const int UserIsNotAMemberOfWall = 501;

        // Know Your Speaker, 8**
        public const int GameAlreadyFinished = 801;

        // User settings. Culture, 9**
        public const int CultureUnsupported = 900;
        public const int TimezoneUnsupported = 901;

        // Project, 11**
        public const int CantRemoveProjectOwner = 1100;

        // Organization, 12**
        public const int UserIsNotAManager = 1200;

        // Badges, 13**
        public const int BadgeTypeNotFound = 1300;
        public const int BadgeCategoryNotFound = 1301;
        public const int BadgeToKudosRelationshipNotFound = 1302;

        // Filter presets, 14**
        public const int InvalidType = 1400;
        public const int FilterNotFound = 1403;
        public const int FilterPresetContainsMoreThanOneDefaultPreset = 1404;

        // Blacklist, 15**
        public const int BlacklistEntryNotFound = 1500;
        public const int InvalidPermissionForBlacklistHistory = 1501;

        // Service requests, 16**
        public const int InvalidCategoryChange = 1600;

        // Groups, 17**
        public const int GroupNotFound = 1700;
        public const int GroupTypeNotFound = 1701;
        public const int GroupNameAlreadyExists = 1702;
        public const int GroupEditNotAllowed = 1703;
        public const int GroupTypeNameAlreadyExists = 1704;
        public const int GroupTypeHasGroups = 1706;
        public const int GroupFieldNotAllowedByType = 1707;
        public const int GroupEndDateBeforeStartDate = 1709;
        public const int GroupCreationNotAllowed = 1712;
        public const int GroupApprovalAnswersRequired = 1713;
        public const int GroupDeleteNotAllowed = 1714;
        public const int GroupMemberCannotBeRemoved = 1715;
        public const int GroupInvalidKudosPeriod = 1716;

        // Custom emoji, 18**
        public const int InvalidCustomEmojiName = 1800;
        public const int InvalidCustomEmojiImage = 1801;
        public const int CustomEmojiImageTooLarge = 1802;

        // Vacations, 19**
        public const int VacationTypeRequired = 1900;
        public const int VacationDatesRequired = 1901;
        public const int VacationWrongPeriod = 1902;
        public const int VacationStartInPast = 1903;
        public const int VacationTooFarAhead = 1904;
        public const int VacationNoWorkingDays = 1905;
        public const int VacationOverlap = 1906;
        public const int VacationNoteTooLong = 1907;
        public const int VacationNotFound = 1908;
        public const int VacationNotEditable = 1909;
        public const int VacationNotCancellable = 1910;
        public const int VacationNotReviewable = 1911;
        public const int VacationReasonTooShort = 1912;
        public const int VacationNotAuthorized = 1913;
        public const int VacationOrderEmpty = 1914;
        public const int VacationOrderNotFound = 1915;
        public const int VacationImportDateRequired = 1916;
        public const int VacationImportUnreadable = 1917;
        public const int VacationOrderRaceLost = 1918;
        public const int VacationArchiveTooLarge = 1919;
    }
}
