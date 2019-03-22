using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.ServiceRequest;

namespace Shrooms.Domain.Services.ServiceRequests
{
    public interface IServiceRequestService
    {
        void CreateComment(ServiceRequestCommentDTO comment, UserAndOrganizationDTO userAndOrganizationDTO);

        void CreateNewServiceRequest(ServiceRequestDTO newServiceRequestDTO, UserAndOrganizationDTO userAndOrganizationDTO);

        void UpdateServiceRequest(ServiceRequestDTO serviceRequestDTO, UserAndOrganizationDTO userAndOrganizationDTO);

        IEnumerable<ServiceRequestCategoryDTO> GetCategories();

        void CreateCategory(ServiceRequestCategoryDTO category, string userId);
        
        ServiceRequestCategoryDTO GetCategory(int categoryId);

        void EditCategory(ServiceRequestCategoryDTO modelDto, string userId);

        void DeleteCategory(int categoryId, string userId);

        void MoveRequestToDone(int requestId, UserAndOrganizationDTO userAndOrganizationDTO);
    }
}
