using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Roles;

namespace Shrooms.Domain.Services.WebHookCallbacks.BirthdayNotification
{
    public class BirthdaysNotificationWebHookService : IBirthdaysNotificationWebHookService
    {
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<Organization> _organizationsDbSet;
        private readonly DateTime _date;
        private readonly IMailingService _mailingService;
        private readonly IRoleService _roleService;
        private readonly IMailTemplate _mailTemplate;
        private readonly IApplicationSettings _appSettings;

        public BirthdaysNotificationWebHookService(
            IUnitOfWork2 uow,
            IMailingService mailingService,
            IRoleService roleService,
            IMailTemplate mailTemplate,
            IApplicationSettings appSettings)
        {
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationsDbSet = uow.GetDbSet<Organization>();

            _date = DateTime.UtcNow;
            _mailingService = mailingService;
            _roleService = roleService;
            _mailTemplate = mailTemplate;
            _appSettings = appSettings;
        }

        public async Task SendNotificationsAsync(string organizationName)
        {
            var todaysBirthdayUsers = await _usersDbSet
                .Where(w =>
                    w.BirthDay.HasValue &&
                    w.BirthDay.Value.Month == _date.Month &&
                    w.BirthDay.Value.Day == _date.Day)
                .ToListAsync();

            if (!todaysBirthdayUsers.Any())
            {
                return;
            }

            await SendBirthdayReminderAsync(todaysBirthdayUsers, organizationName);
        }

        private async Task SendBirthdayReminderAsync(IEnumerable<ApplicationUser> employees, string organizationName)
        {
            var currentOrganization = await _organizationsDbSet.FirstAsync(name => name.ShortName == organizationName);

            var receivers = await _roleService.GetAdministrationRoleEmailsAsync(currentOrganization.Id);
            var model = new BirthdaysNotificationTemplateViewModel(GetFormattedEmployeesList(employees, organizationName, currentOrganization.ShortName), _appSettings.UserNotificationSettingsUrl(organizationName));
            var content = _mailTemplate.Generate(model, EmailTemplateCacheKeys.BirthdaysNotification);
            var emailData = new EmailDto(receivers, Resources.Emails.Templates.BirthdaysNotificationEmailSubject, content);

            await _mailingService.SendEmailAsync(emailData);
        }

        private IList<BirthdaysNotificationEmployeeViewModel> GetFormattedEmployeesList(IEnumerable<ApplicationUser> employees, string organizationName, string tenantPicturesContainer)
        {
            var formattedEmployeeList = new List<BirthdaysNotificationEmployeeViewModel>();

            foreach (var employee in employees)
            {
                formattedEmployeeList.Add(new BirthdaysNotificationEmployeeViewModel
                {
                    FullName = $"{employee.FirstName} {employee.LastName}",
                    PictureUrl = _appSettings.PictureUrl(tenantPicturesContainer, employee.PictureId),
                    ProfileUrl = _appSettings.UserProfileUrl(organizationName, employee.Id)
                });
            }

            return formattedEmployeeList;
        }
    }
}