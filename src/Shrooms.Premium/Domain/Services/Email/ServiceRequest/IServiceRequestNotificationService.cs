using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.ServiceRequest;

namespace Shrooms.Premium.Domain.Services.Email.ServiceRequest
{
    public interface IServiceRequestNotificationService
    {
        void NotifyAboutNewComment(ServiceRequestCreatedCommentDTO comment);
        void NotifyAboutNewServiceRequest(CreatedServiceRequestDTO createdServiceReques);
        void NotifyAboutServiceRequestStatusUpdate(UpdatedServiceRequestDTO updatedRequest, UserAndOrganizationDTO userAndOrganizationDTO);
    }
}
