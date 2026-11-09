using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class OvertimeEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public OvertimeEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            string employeeName,
            string period,
            string hoursPerDay,
            string reason,
            string requestUrl)
            : base(userNotificationSettingsUrl)
        {
            EmployeeName = employeeName;
            Period = period;
            HoursPerDay = hoursPerDay;
            Reason = reason;
            RequestUrl = requestUrl;
        }

        public string EmployeeName { get; set; }

        public string Period { get; set; }

        public string HoursPerDay { get; set; }

        public string Reason { get; set; }

        public string RequestUrl { get; set; }

        public string Note { get; set; }

        public string Outcome { get; set; }

        public string ActorName { get; set; }
    }

    public class OvertimeDecidableEmailTemplateViewModel : OvertimeEmailTemplateViewModel
    {
        public OvertimeDecidableEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            string employeeName,
            string period,
            string hoursPerDay,
            string reason,
            string requestUrl,
            string approveUrl,
            string rejectUrl)
            : base(userNotificationSettingsUrl, employeeName, period, hoursPerDay, reason, requestUrl)
        {
            ApproveUrl = approveUrl;
            RejectUrl = rejectUrl;
        }

        public string ApproveUrl { get; set; }

        public string RejectUrl { get; set; }
    }
}
