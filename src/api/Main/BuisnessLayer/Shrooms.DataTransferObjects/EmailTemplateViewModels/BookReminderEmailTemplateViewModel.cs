using System;

namespace Shrooms.DataTransferObjects.EmailTemplateViewModels
{
    public class BookReminderEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public BookReminderEmailTemplateViewModel(string title, string author, string takenFrom, string bookUrl, string fullName, string userNotificationSettingsUrl)
            : base(userNotificationSettingsUrl)
        {
            Title = title;
            Author = author;
            TakenFrom = takenFrom;
            BookUrl = bookUrl;
            FullName = fullName;
        }

        public string Title { get; set; }
        public string Author { get; set; }
        public string TakenFrom { get; set; }
        public string BookUrl { get; set; }
        public string FullName { get; set; }
    }
}