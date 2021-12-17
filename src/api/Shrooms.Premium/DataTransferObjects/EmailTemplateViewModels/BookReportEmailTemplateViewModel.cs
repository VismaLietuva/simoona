using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class BookReportEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Report { get; set; }
        public string Comment { get; set; }
        public string BookUrl { get; set; }
        public string FullName { get; set; }

        public BookReportEmailTemplateViewModel(string title, string author, string report, string comment, string bookUrl, string fullName, string userNotificationSettingsUrl)
            : base(userNotificationSettingsUrl)
        {
            Title = title;
            Author = author;
            Report = report;
            Comment = comment;
            BookUrl = bookUrl;
            FullName = fullName;
        }
    }
}
