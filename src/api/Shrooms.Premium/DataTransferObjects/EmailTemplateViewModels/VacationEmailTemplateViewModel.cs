using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    /// <summary>
    /// What every leave email says: whose leave it is, when, and what kind. The
    /// dates and the leave type arrive already written out, because the sender
    /// knows the recipient's language and the template does not.
    /// </summary>
    public class VacationEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public VacationEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            string employeeName,
            string period,
            string leaveType,
            string workingDays,
            string requestUrl)
            : base(userNotificationSettingsUrl)
        {
            EmployeeName = employeeName;
            Period = period;
            LeaveType = leaveType;
            WorkingDays = workingDays;
            RequestUrl = requestUrl;
        }

        public string EmployeeName { get; set; }

        public string Period { get; set; }

        public string LeaveType { get; set; }

        public string WorkingDays { get; set; }

        public string RequestUrl { get; set; }

        /// <summary>The employee's note, or the reason a request was turned down. Optional.</summary>
        public string Note { get; set; }

        /// <summary>Approved, rejected or cancelled, already translated. Set on a decision.</summary>
        public string Outcome { get; set; }

        /// <summary>Who decided or changed it, when that is not the recipient.</summary>
        public string ActorName { get; set; }
    }

    /// <summary>The manager's copy, which can be answered without opening the app.</summary>
    public class VacationSubmittedEmailTemplateViewModel : VacationEmailTemplateViewModel
    {
        public VacationSubmittedEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            string employeeName,
            string period,
            string leaveType,
            string workingDays,
            string requestUrl,
            string approveUrl,
            string rejectUrl)
            : base(userNotificationSettingsUrl, employeeName, period, leaveType, workingDays, requestUrl)
        {
            ApproveUrl = approveUrl;
            RejectUrl = rejectUrl;
        }

        public string ApproveUrl { get; set; }

        public string RejectUrl { get; set; }
    }
}
