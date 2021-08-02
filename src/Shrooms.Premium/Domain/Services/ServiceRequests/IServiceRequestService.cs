using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.ServiceRequest;

namespace Shrooms.Premium.Domain.Services.ServiceRequests
{
    public interface IServiceRequestService
    {
        Task CreateCommentAsync(ServiceRequestCommentDTO comment, UserAndOrganizationDTO userAndOrganizationDTO);

        Task CreateNewServiceRequestAsync(ServiceRequestDTO newServiceRequestDTO, UserAndOrganizationDTO userAndOrganizationDTO);

        Task UpdateServiceRequestAsync(ServiceRequestDTO serviceRequestDTO, UserAndOrganizationDTO userAndOrganizationDTO);

        Task<IEnumerable<ServiceRequestCategoryDTO>> GetCategoriesAsync();

        Task CreateCategoryAsync(ServiceRequestCategoryDTO category, string userId);

        ServiceRequestCategoryDTO GetCategory(int categoryId);

        Task EditCategoryAsync(ServiceRequestCategoryDTO modelDto, string userId);

        void DeleteCategory(int categoryId, string userId);

        Task MoveRequestToDoneAsync(int requestId, UserAndOrganizationDTO userAndOrganizationDTO);
    }
}
