using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.ServiceRequest;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.ServiceRequest
{
    public interface IServiceRequestNotificationService
    {
        void NotifyAboutNewComment(ServiceRequestCreatedCommentDTO comment);
        void NotifyAboutNewServiceRequest(CreatedServiceRequestDTO createdServiceReques);
        void NotifyAboutServiceRequestStatusUpdate(UpdatedServiceRequestDTO updatedRequest, UserAndOrganizationDTO userAndOrganizationDTO);
    }
}
