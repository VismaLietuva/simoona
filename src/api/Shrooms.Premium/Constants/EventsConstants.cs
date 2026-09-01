namespace Shrooms.Premium.Constants
{
    public static class EventsConstants
    {
        public const string EventMainType = "main";
        public const string EventAllType = "all";

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

        public const int EventQuestionsMaxCount = 20;
        public const int EventQuestionOptionsMaxCount = 30;
        public const int EventQuestionOptionsMinCount = 1;
        public const int EventQuestionTitleMaxLength = 100;
        public const int EventQuestionOptionNameMaxLength = 100;
        public const int EventQuestionMaxConditionalDepth = 5;

        public const string EventParticipantsExcelTableName = "Event Participants";
        public const string EventOptionsExcelTableName = "Event Options";

        public const int EventReportVisitedEventPreviewCount = 3;
    }

    public enum AttendingStatus
    {
        NotAttending = 0,
        Attending,
        MaybeAttending,
        Idle,
        AttendingVirtually
    }

    public enum MyEventsOptions
    {
        Host,
        Participant
    }

    public enum EventTimeFrame
    {
        Upcoming = 0,
        Past = 1,
        All = 2
    }
}
