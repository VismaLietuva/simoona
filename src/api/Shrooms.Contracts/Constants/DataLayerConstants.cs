namespace Shrooms.Contracts.Constants
{
    public static class DataLayerConstants
    {
        public const string ClaimUserImpersonation = "UserImpersonation";
        public const string ClaimOriginalUsername = "OriginalUsername";
        public const string OrganizationManagerUsername = "Admin";
        public static readonly char PermissionSplitter = '_';

        public const string ConnectionStringNameBackgroundJobs = "BackgroundJobs";

        public const string DefaultTimeZone = "FLE Standard Time";

        /// <summary>
        /// Width of AspNetUsers.Id in the deployed databases. Predates EF Core's
        /// 450-character default, so any new string foreign key pointing at a user
        /// must declare it — SQL Server refuses a foreign key whose column is not
        /// the same length as the one it references.
        /// </summary>
        public const int IdentityKeyLength = 128;
    }
}