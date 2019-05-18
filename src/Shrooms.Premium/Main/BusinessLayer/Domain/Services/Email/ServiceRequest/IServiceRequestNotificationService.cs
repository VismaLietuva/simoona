using Shrooms.DataTransferObjects.Models;
using Shrooms.EntityModels.Models;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.ServiceRequest
{
    public interface IServiceRequestNotificationService
    {
        void NotifyAboutNewComment(EntityModels.Models.ServiceRequest serviceRequest, ServiceRequestComment serviceRequestComment);
        void NotifyAboutNewServiceRequest(EntityModels.Models.ServiceRequest newServiceRequest, UserAndOrganizationDTO userAndOrg);
        void NotifyAboutServiceRequestStatusUpdate(EntityModels.Models.ServiceRequest serviceRequest, UserAndOrganizationDTO userAndOrganizationDTO, string statusName);
    }
}
