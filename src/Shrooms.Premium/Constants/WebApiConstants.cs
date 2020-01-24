namespace Shrooms.Premium.Constants
{
    public static class WebApiConstants
    {
        public const int DefaultPageSize = 10;
        public const int OneHour = 3600;
        public const int MaximumPictureSizeInBytes = 12000000;

        public const int EventNameMaxLength = 35;
        public const int EventTypeNameMaxLength = 30;
        public const int EventMinimumParticipants = 0;
        public const int EventMaxParticipants = 1000;
        public const int EventLocationMaxLength = 50;
        public const int EventDescriptionMaxLength = 5000;
        public const int EventMinimumOptions = 0;

        public const int ServiceRequestCategoryNameMaxLength = 30;

        public const int KudosTypeNameMaxLength = 30;

        #region Claims
        public const string ClaimOrganizationId = "OrganizationId";
        #endregion
    }
}
