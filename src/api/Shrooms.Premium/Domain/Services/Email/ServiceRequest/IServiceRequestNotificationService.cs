using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.ServiceRequest;

namespace Shrooms.Premium.Domain.Services.Email.ServiceRequest
{
    public interface IServiceRequestNotificationService
    {
        Task NotifyAboutNewCommentAsync(ServiceRequestCreatedCommentDto comment);
        Task NotifyAboutNewServiceRequestAsync(CreatedServiceRequestDto createdServiceRequest);
        Task NotifyAboutServiceRequestStatusUpdateAsync(UpdatedServiceRequestDto updatedRequest, UserAndOrganizationDto userAndOrganizationDto);
    }
}
