using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.ServiceRequest;
using Shrooms.EntityModels.Models;

namespace Shrooms.Domain.Services.Email.ServiceRequest
{
    public interface IServiceRequestNotificationService
    {
        void NotifyAboutNewComment(ServiceRequestCreatedCommentDTO comment);
        void NotifyAboutNewServiceRequest(CreatedServiceRequestDTO createdServiceReques);
        void NotifyAboutServiceRequestStatusUpdate(UpdatedServiceRequestDTO updatedRequest, UserAndOrganizationDTO userAndOrganizationDTO);
    }
}
