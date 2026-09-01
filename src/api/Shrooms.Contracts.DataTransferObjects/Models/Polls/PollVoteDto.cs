using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Polls
{
    public class PollVoteDto : UserAndOrganizationDto
    {
        public int PollId { get; set; }

        public IList<PollQuestionAnswerDto> Answers { get; set; }
    }

    public class PollQuestionAnswerDto
    {
        public int QuestionId { get; set; }

        public IList<int> OptionIds { get; set; }
    }
}
