namespace Shrooms.Premium.Constants
{
    public static class BusinessLayerConstants
    {
        public const string DeletedUserName = "Deleted Account";
        public const string DeletedUserFirstName = "Deleted";
        public const string DeletedUserLastName = "Account";

        #region Events
        public const string FoodEventTypeName = "foodEventType";
        public const int EventOptionsMinimumCount = 2;
        public const string EventParticipantsExcelTableName = "Event Participants";
        public const string EventOptionsExcelTableName = "Event Options";

        public enum MyEventsOptions
        {
            Host,
            Participant
        }
        #endregion

        #region ServiceRequests

        public const string ServiceRequestsExcelSheetName = "Service requests";

        #endregion
    }
}