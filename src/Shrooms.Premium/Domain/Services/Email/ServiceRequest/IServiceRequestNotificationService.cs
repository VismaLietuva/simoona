using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.ServiceRequest;

namespace Shrooms.Premium.Domain.Services.Email.ServiceRequest
{
    public interface IServiceRequestNotificationService
    {
        Task NotifyAboutNewCommentAsync(ServiceRequestCreatedCommentDTO comment);
        Task NotifyAboutNewServiceRequestAsync(CreatedServiceRequestDTO createdServiceReques);
        Task NotifyAboutServiceRequestStatusUpdateAsync(UpdatedServiceRequestDTO updatedRequest, UserAndOrganizationDTO userAndOrganizationDTO);
    }
}
