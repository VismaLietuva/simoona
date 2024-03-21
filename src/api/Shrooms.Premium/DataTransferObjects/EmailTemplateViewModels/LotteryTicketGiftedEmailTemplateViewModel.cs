using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class LotteryTicketGiftedEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string LotteryTitle { get; set; }
        public string LotteryUrl { get; set; }
        public string BuyerFullName { get; set; }
        public int GiftedTicketCount { get; set; }

        public LotteryTicketGiftedEmailTemplateViewModel(string lotteryTitle, string lotteryUrl, string buyerFullName, int giftedTicketCount, string userNotificationSettingsUrl)
            : base(userNotificationSettingsUrl)
        {
            LotteryTitle = lotteryTitle;
            LotteryUrl = lotteryUrl;
            BuyerFullName = buyerFullName;
            GiftedTicketCount = giftedTicketCount;
        }
    }
}
