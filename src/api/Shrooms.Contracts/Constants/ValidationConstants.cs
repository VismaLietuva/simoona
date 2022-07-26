namespace Shrooms.Contracts.Constants
{
    public static class ValidationConstants
    {
        public const int MaxPostMessageBodyLength = 5000;
        public const int MaxCommentMessageBodyLength = 5000;
        public const int SupportMessageBodyMaxLength = 5000;
        public const int SupportSubjectMaxLength = 300;

        public const int KudosMultiplyByMaxValue = int.MaxValue;
        public const int KudosMultiplyByMinValue = 1;
        public const int KudosCommentMaxLength = 500;

        public const int ExamMaxTitleLength = 255;
        public const int ExamMaxNumberLength = 255;

        public const int FilterPresetMaxNameLength = 30;

        public const int BlacklistReasonMaxLength = 2000;
    }
}
