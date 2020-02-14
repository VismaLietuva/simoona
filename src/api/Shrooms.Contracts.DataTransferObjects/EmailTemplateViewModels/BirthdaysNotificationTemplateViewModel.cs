using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels
{
    public class BirthdaysNotificationTemplateViewModel : BaseEmailTemplateViewModel
    {
        public IList<BirthdaysNotificationEmployeeViewModel> Employees { get; set; }

        public BirthdaysNotificationTemplateViewModel(IList<BirthdaysNotificationEmployeeViewModel> employees, string userNotificationSettingsUrl)
            : base(userNotificationSettingsUrl)
        {
            Employees = employees;
        }
    }
}