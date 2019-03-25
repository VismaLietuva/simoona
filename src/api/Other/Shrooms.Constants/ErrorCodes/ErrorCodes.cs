namespace Shrooms.Constants.ErrorCodes
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
    }
}
