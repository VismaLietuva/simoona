namespace Shrooms.Host.Contracts.Constants
{
    public static class DataLayerConstants
    {
        public const string ClaimUserImpersonation = "UserImpersonation";
        public const string ClaimOriginalUsername = "OriginalUsername";
        public const string OrganizationManagerUsername = "Admin";
        public static readonly char PermissionSplitter = '_';

        public const string ConnectionStringNameBackgroundJobs = "BackgroundJobs";
    }
}