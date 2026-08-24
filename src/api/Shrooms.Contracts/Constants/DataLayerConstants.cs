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
        /// Width of AspNetUsers.Id / AspNetRoles.Id in the deployed databases.
        /// Predates EF Core Identity's 450-character default. Applied centrally in
        /// ShroomsDbContext.OnModelCreating to the Identity keys and every foreign
        /// key referencing them, so individual entity configurations must not
        /// declare it themselves — SQL Server refuses a foreign key whose column
        /// is not the same length as the one it references.
        /// </summary>
        public const int IdentityKeyLength = 128;
    }
}