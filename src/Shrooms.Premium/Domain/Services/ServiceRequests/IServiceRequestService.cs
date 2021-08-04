using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.ServiceRequest;

namespace Shrooms.Premium.Domain.Services.ServiceRequests
{
    public interface IServiceRequestService
    {
        Task CreateCommentAsync(ServiceRequestCommentDto comment, UserAndOrganizationDto userAndOrganizationDto);

        Task CreateNewServiceRequestAsync(ServiceRequestDto newServiceRequestDto, UserAndOrganizationDto userAndOrganizationDto);

        Task UpdateServiceRequestAsync(ServiceRequestDto serviceRequestDto, UserAndOrganizationDto userAndOrganizationDto);

        Task<IEnumerable<ServiceRequestCategoryDto>> GetCategoriesAsync();

        Task CreateCategoryAsync(ServiceRequestCategoryDto category, string userId);

        Task<ServiceRequestCategoryDto> GetCategoryAsync(int categoryId);

        Task EditCategoryAsync(ServiceRequestCategoryDto modelDto, string userId);

        Task DeleteCategoryAsync(int categoryId, string userId);

        Task MoveRequestToDoneAsync(int requestId, UserAndOrganizationDto userAndOrganizationDto);
    }
}
