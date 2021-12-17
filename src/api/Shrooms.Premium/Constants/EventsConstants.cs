namespace Shrooms.Premium.Constants
{
    public static class EventsConstants
    {
        public const int EventNameMaxLength = 35;
        public const int EventTypeNameMaxLength = 30;
        public const int EventMinimumParticipants = 0;
        public const int EventMaxParticipants = 1000;
        public const int EventLocationMaxLength = 50;
        public const int EventDescriptionMaxLength = 5000;
        public const int EventMinimumOptions = 0;
        public const int EventsDefaultPageSize = 10;
        public const int EventsMaxDateFilterRangeInDays = 100;

        public const int EventOptionsMinimumCount = 2;
        public const string EventParticipantsExcelTableName = "Event Participants";
        public const string EventOptionsExcelTableName = "Event Options";
    }

    public enum AttendingStatus
    {
        NotAttending = 0,
        Attending,
        MaybeAttending,
        Idle
    }

    public enum MyEventsOptions
    {
        Host,
        Participant
    }
}