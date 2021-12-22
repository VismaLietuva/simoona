using System;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class BookTakenEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string BookTitle { get; set; }
        public string BookAuthor { get; set; }
        public DateTime TakenTill { get; set; }
        public string BookUrl { get; set; }

        public BookTakenEmailTemplateViewModel(string userNotificationSettingsUrl, string bookTitle, string bookAuthor, string bookUrl)
            : base(userNotificationSettingsUrl)
        {
            BookTitle = bookTitle;
            BookAuthor = bookAuthor;
            BookUrl = bookUrl;
        }
    }
}
