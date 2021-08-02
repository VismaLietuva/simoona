using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.DataTransferObjects.Models.ServiceRequest;
using ServiceRequestModel = Shrooms.DataLayer.EntityModels.Models.ServiceRequest;

namespace Shrooms.Premium.Domain.Services.Email.ServiceRequest
{
    public class ServiceRequestNotificationService : IServiceRequestNotificationService
    {
        private readonly IDbSet<Organization> _organizationsDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<ApplicationRole> _rolesDbSet;
        private readonly IDbSet<ServiceRequestStatus> _serviceRequestStatusDbSet;
        private readonly IDbSet<ServiceRequestModel> _serviceRequestDbSet;
        private readonly IApplicationSettings _appSettings;
        private readonly IMailingService _mailingService;
        private readonly IMailTemplate _mailTemplate;

        public ServiceRequestNotificationService(IUnitOfWork2 uow,
            IMailingService mailingService,
            IMailTemplate mailTemplate,
            IApplicationSettings appSettings)
        {
            _organizationsDbSet = uow.GetDbSet<Organization>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _rolesDbSet = uow.GetDbSet<ApplicationRole>();
            _serviceRequestStatusDbSet = uow.GetDbSet<ServiceRequestStatus>();
            _serviceRequestDbSet = uow.GetDbSet<ServiceRequestModel>();
            _mailingService = mailingService;
            _mailTemplate = mailTemplate;
            _appSettings = appSettings;
        }

        public async Task NotifyAboutNewServiceRequestAsync(CreatedServiceRequestDTO createdServiceRequest)
        {
            var newServiceRequest = await _serviceRequestDbSet.SingleAsync(s => s.Id == createdServiceRequest.ServiceRequestId);
            var organizationName = await GetOrganizationNameAsync(newServiceRequest.OrganizationId);

            var emails = await _usersDbSet
                .Where(x => x.ServiceRequestCategoriesAssigned.Any(y => y.Name == newServiceRequest.CategoryName))
                .Where(x => x.Id != newServiceRequest.EmployeeId)
                .Select(x => x.Email)
                .ToListAsync();

            var subject = Resources.Models.ServiceRequest.ServiceRequest.EmailMessageSubject;
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organizationName);
            var serviceRequestUrl = _appSettings.ServiceRequestUrl(organizationName, newServiceRequest.Id);

            var emailTemplateViewModel = new ServiceRequestEmailTemplateViewModel(userNotificationSettingsUrl,
                newServiceRequest.Title,
                GetUserFullName(newServiceRequest.EmployeeId),
                serviceRequestUrl);

            var body = _mailTemplate.Generate(emailTemplateViewModel, EmailPremiumTemplateCacheKeys.ServiceRequest);

            await _mailingService.SendEmailAsync(new EmailDto(emails, subject, body));
        }

        public async Task NotifyAboutNewCommentAsync(ServiceRequestCreatedCommentDTO createdComment)
        {
            var serviceRequest = await _serviceRequestDbSet.SingleAsync(s => s.Id == createdComment.ServiceRequestId);
            var organizationName = await GetOrganizationNameAsync(serviceRequest.OrganizationId);

            var serviceRequestNotificationRoleId = await GetUserNotificationRoleIdAsync();

            var emails = await _usersDbSet
                .Where(x => x.Roles.Any(y => y.RoleId == serviceRequestNotificationRoleId) ||
                            x.Id == serviceRequest.EmployeeId)
                .Where(x => x.Id != createdComment.CommentedEmployeeId)
                .Select(x => x.Email)
                .ToListAsync();

            var subject = Resources.Common.ServiceRequestAdminCommentedSubject;
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organizationName);
            var serviceRequestUrl = _appSettings.ServiceRequestUrl(organizationName, serviceRequest.Id);

            var emailTemplateViewModel = new ServiceRequestCommentEmailTemplateViewModel(
                userNotificationSettingsUrl,
                serviceRequest.Title,
                GetUserFullName(createdComment.CommentedEmployeeId),
                createdComment.CommentContent,
                serviceRequestUrl);

            var body = _mailTemplate.Generate(emailTemplateViewModel, EmailPremiumTemplateCacheKeys.ServiceRequestComment);

            await _mailingService.SendEmailAsync(new EmailDto(emails, subject, body));
        }

        public async Task NotifyAboutServiceRequestStatusUpdateAsync(UpdatedServiceRequestDTO updatedRequest, UserAndOrganizationDTO userAndOrganizationDTO)
        {
            var serviceRequest = _serviceRequestDbSet.Single(s => s.Id == updatedRequest.ServiceRequestId);
            var newStatusName = _serviceRequestStatusDbSet.Where(x => x.Id == serviceRequest.StatusId).Select(x => x.Title).First();

            var organizationName = await GetOrganizationNameAsync(serviceRequest.OrganizationId);

            var email = await _usersDbSet
                .Where(x => x.Id == serviceRequest.EmployeeId)
                .Where(x => x.Id != userAndOrganizationDTO.UserId)
                .Select(x => x.Email)
                .FirstOrDefaultAsync();

            if (email == null)
            {
                return;
            }

            var subject = Resources.Common.ServiceRequestAdminChangedStatusSubject;
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organizationName);
            var serviceRequestUrl = _appSettings.ServiceRequestUrl(organizationName, serviceRequest.Id);

            var emailTemplateViewModel = new ServiceRequestUpdateEmailTemplateViewModel(userNotificationSettingsUrl,
                serviceRequest.Title,
                GetUserFullName(userAndOrganizationDTO.UserId),
                newStatusName,
                serviceRequestUrl);

            var body = _mailTemplate.Generate(emailTemplateViewModel, EmailPremiumTemplateCacheKeys.ServiceRequestUpdate);

            await _mailingService.SendEmailAsync(new EmailDto(email, subject, body));
        }

        private string GetUserFullName(string userId)
        {
            return _usersDbSet
                .Where(u => u.Id == userId)
                .Select(u => u.FirstName + " " + u.LastName)
                .First();
        }

        private async Task<string> GetOrganizationNameAsync(int? organizationId)
        {
            return await _organizationsDbSet
                .Where(organization => organization.Id == organizationId)
                .Select(organization => organization.ShortName)
                .FirstOrDefaultAsync();
        }

        private async Task<string> GetUserNotificationRoleIdAsync()
        {
            return await _rolesDbSet
                .Where(x => x.Name == Roles.ServiceRequestNotification)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();
        }
    }
}
