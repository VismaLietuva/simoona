using System.Collections.Generic;
using Shrooms.Host.Contracts.DataTransferObjects;

namespace Shrooms.DataTransferObjects.EmailTemplateViewModels
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