using System.Data.Entity;
using System.Linq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
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

        public void NotifyAboutNewServiceRequest(CreatedServiceRequestDTO createdServiceRequest)
        {
            var newServiceRequest = _serviceRequestDbSet.Single(s => s.Id == createdServiceRequest.ServiceRequestId);
            var organizationName = GetOrganizationName(newServiceRequest.OrganizationId);

            var emails = _usersDbSet
                .Where(x => x.ServiceRequestCategoriesAssigned.Any(y => y.Name == newServiceRequest.CategoryName))
                .Where(x => x.Id != newServiceRequest.EmployeeId)
                .Select(x => x.Email)
                .ToList();

            var subject = Resources.Models.ServiceRequest.ServiceRequest.EmailMessageSubject;
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organizationName);
            var serviceRequestUrl = _appSettings.ServiceRequestUrl(organizationName, newServiceRequest.Id);

            var emailTemplateViewModel = new ServiceRequestEmailTemplateViewModel(
                userNotificationSettingsUrl,
                newServiceRequest.Title,
                GetUserFullName(newServiceRequest.EmployeeId),
                serviceRequestUrl);

            var body = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.ServiceRequest);

            _mailingService.SendEmail(new EmailDto(emails, subject, body));
        }

        public void NotifyAboutNewComment(ServiceRequestCreatedCommentDTO createdComment)
        {
            var serviceRequest = _serviceRequestDbSet.Single(s => s.Id == createdComment.ServiceRequestId);
            var organizationName = GetOrganizationName(serviceRequest.OrganizationId);

            var serviceRequestNotificationRoleId = GetUserNotificationRoleId();

            var emails = _usersDbSet
                .Where(x => x.Roles.Any(y => y.RoleId == serviceRequestNotificationRoleId) ||
                    x.Id == serviceRequest.EmployeeId)
                .Where(x => x.Id != createdComment.CommentedEmployeeId)
                .Select(x => x.Email)
                .ToList();

            var subject = Resources.Common.ServiceRequestAdminCommentedSubject;
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organizationName);
            var serviceRequestUrl = _appSettings.ServiceRequestUrl(organizationName, serviceRequest.Id);

            var emailTemplateViewModel = new ServiceRequestCommentEmailTemplateViewModel(
                userNotificationSettingsUrl,
                serviceRequest.Title,
                GetUserFullName(createdComment.CommentedEmployeeId),
                createdComment.CommentContent,
                serviceRequestUrl);

            var body = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.ServiceRequestComment);

            _mailingService.SendEmail(new EmailDto(emails, subject, body));
        }

        public void NotifyAboutServiceRequestStatusUpdate(
            UpdatedServiceRequestDTO updatedRequest,
            UserAndOrganizationDTO userAndOrganizationDTO)
        {
            var serviceRequest = _serviceRequestDbSet.Single(s => s.Id == updatedRequest.ServiceRequestId);
            var newStatusName = _serviceRequestStatusDbSet.Where(x => x.Id == serviceRequest.StatusId).Select(x => x.Title).First();

            var organizationName = GetOrganizationName(serviceRequest.OrganizationId);

            var email = _usersDbSet
                .Where(x => x.Id == serviceRequest.EmployeeId)
                .Where(x => x.Id != userAndOrganizationDTO.UserId)
                .Select(x => x.Email)
                .FirstOrDefault();

            if (email == null)
            {
                return;
            }

            var subject = Resources.Common.ServiceRequestAdminChangedStatusSubject;
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organizationName);
            var serviceRequestUrl = _appSettings.ServiceRequestUrl(organizationName, serviceRequest.Id);

            var emailTemplateViewModel = new ServiceRequestUpdateEmailTemplateViewModel(
                userNotificationSettingsUrl,
                serviceRequest.Title,
                GetUserFullName(userAndOrganizationDTO.UserId),
                newStatusName,
                serviceRequestUrl);

            var body = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.ServiceRequestUpdate);

            _mailingService.SendEmail(new EmailDto(email, subject, body));
        }

        private string GetUserFullName(string userId) => _usersDbSet
                 .Where(u => u.Id == userId)
                 .Select(u => u.FirstName + " " + u.LastName)
                 .First();

        private string GetOrganizationName(int? organizationId) => _organizationsDbSet
                .Where(organization => organization.Id == organizationId)
                .Select(organization => organization.ShortName)
                .FirstOrDefault();

        private string GetUserNotificationRoleId() => _rolesDbSet
                .Where(x => x.Name == Roles.ServiceRequestNotification)
                .Select(x => x.Id)
                .FirstOrDefault();
    }
}
