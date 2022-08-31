using System;

namespace Shrooms.Contracts.DataTransferObjects.EmailTemplateDtos
{
    public class LotteryStartedEmailDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int EntryFee { get; set; }

        public DateTime EndDate { get; set; }
    }
}
