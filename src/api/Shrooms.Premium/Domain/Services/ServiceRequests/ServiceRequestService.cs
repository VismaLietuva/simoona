using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.ServiceRequest;
using Shrooms.Premium.Domain.Services.Email.ServiceRequest;

namespace Shrooms.Premium.Domain.Services.ServiceRequests
{
    public class ServiceRequestService : IServiceRequestService
    {
        private const string ServiceRequestStatusDone = "Done";
        private const string ServiceRequestCategoryKudos = "Kudos";

        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<ServiceRequest> _serviceRequestsDbSet;
        private readonly IDbSet<ServiceRequestComment> _serviceRequestCommentsDbSet;
        private readonly IDbSet<ServiceRequestCategory> _serviceRequestCategoryDbSet;
        private readonly IDbSet<ServiceRequestPriority> _serviceRequestPriorityDbSet;
        private readonly IDbSet<ServiceRequestStatus> _serviceRequestStatusDbSet;
        private readonly IDbSet<ApplicationUser> _userDbSet;
        private readonly IPermissionService _permissionService;
        private readonly IAsyncRunner _asyncRunner;

        public ServiceRequestService(IUnitOfWork2 uow, IPermissionService permissionService, IAsyncRunner asyncRunner)
        {
            _uow = uow;
            _serviceRequestsDbSet = _uow.GetDbSet<ServiceRequest>();
            _serviceRequestCommentsDbSet = _uow.GetDbSet<ServiceRequestComment>();
            _serviceRequestCategoryDbSet = _uow.GetDbSet<ServiceRequestCategory>();
            _serviceRequestPriorityDbSet = _uow.GetDbSet<ServiceRequestPriority>();
            _serviceRequestStatusDbSet = _uow.GetDbSet<ServiceRequestStatus>();
            _userDbSet = _uow.GetDbSet<ApplicationUser>();
            _permissionService = permissionService;
            _asyncRunner = asyncRunner;
        }

        public async Task CreateNewServiceRequestAsync(ServiceRequestDto newServiceRequestDto, UserAndOrganizationDto userAndOrganizationDto)
        {
            await ValidateServiceRequestForCreateAsync(newServiceRequestDto);

            var serviceRequestStatusId = await _serviceRequestStatusDbSet
                    .Where(x => x.Title.Equals("Open"))
                    .Select(x => x.Id)
                    .FirstAsync();

            var serviceRequestCategory = await _serviceRequestCategoryDbSet
                .FirstOrDefaultAsync(x => x.Id == newServiceRequestDto.ServiceRequestCategoryId);

            if (serviceRequestCategory == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request category does not exist");
            }

            var timestamp = DateTime.UtcNow;

            var serviceRequest = new ServiceRequest
            {
                Description = newServiceRequestDto.Description,
                Title = newServiceRequestDto.Title,
                CreatedBy = userAndOrganizationDto.UserId,
                ModifiedBy = userAndOrganizationDto.UserId,
                EmployeeId = userAndOrganizationDto.UserId,
                KudosAmmount = newServiceRequestDto.KudosAmmount,
                KudosShopItemId = newServiceRequestDto.KudosShopItemId,
                OrganizationId = userAndOrganizationDto.OrganizationId,
                CategoryName = serviceRequestCategory.Name,
                StatusId = serviceRequestStatusId,
                PriorityId = newServiceRequestDto.PriorityId,
                Created = timestamp,
                Modified = timestamp,
                PictureId = newServiceRequestDto.PictureId
            };

            _serviceRequestsDbSet.Add(serviceRequest);

            await _uow.SaveChangesAsync(false);

            var srqDto = new CreatedServiceRequestDto { ServiceRequestId = serviceRequest.Id };

            _asyncRunner.Run<IServiceRequestNotificationService>(async notifier => await notifier.NotifyAboutNewServiceRequestAsync(srqDto), _uow.ConnectionName);
        }

        public async Task MoveRequestToDoneAsync(int requestId, UserAndOrganizationDto userAndOrganizationDto)
        {
            var serviceRequest = await _serviceRequestsDbSet
                .Include(x => x.Status)
                .FirstOrDefaultAsync(x => x.Id == requestId && x.OrganizationId == userAndOrganizationDto.OrganizationId);

            var doneStatus = await _serviceRequestStatusDbSet.SingleAsync(s => s.Title == "Done");

            if (serviceRequest == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request does not exist");
            }

            var isServiceRequestAdmin = await _permissionService.UserHasPermissionAsync(userAndOrganizationDto, AdministrationPermissions.ServiceRequest);
            var isServiceRequestCategoryAssignee = (await GetCategoryAssigneesAsync(serviceRequest.CategoryName)).Contains(userAndOrganizationDto.UserId);

            if ((!isServiceRequestAdmin && !isServiceRequestCategoryAssignee) || serviceRequest.StatusId == doneStatus.Id)
            {
                throw new UnauthorizedAccessException();
            }

            serviceRequest.Status = doneStatus;

            await _uow.SaveChangesAsync(false);

            var statusDto = new UpdatedServiceRequestDto { ServiceRequestId = requestId, NewStatusId = serviceRequest.StatusId };
            _asyncRunner.Run<IServiceRequestNotificationService>(async notifier => await notifier.NotifyAboutServiceRequestStatusUpdateAsync(statusDto, userAndOrganizationDto), _uow.ConnectionName);
        }

        public async Task UpdateServiceRequestAsync(ServiceRequestDto serviceRequestDto, UserAndOrganizationDto userAndOrganizationDto)
        {
            var serviceRequest = await _serviceRequestsDbSet
                .Include(x => x.Status)
                .FirstOrDefaultAsync(x => x.Id == serviceRequestDto.Id && x.OrganizationId == userAndOrganizationDto.OrganizationId);

            if (serviceRequest == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request does not exist");
            }

            if (serviceRequest.Status.Title == ServiceRequestStatusDone && serviceRequest.CategoryName == ServiceRequestCategoryKudos)
            {
                throw new ValidationException(PremiumErrorCodes.ServiceRequestIsClosed, "Kudos request status is done");
            }

            await ValidateServiceRequestForCreateAsync(serviceRequestDto);
            await ValidateServiceRequestForUpdateAsync(serviceRequestDto);

            var serviceRequestCategory = await _serviceRequestCategoryDbSet.FirstOrDefaultAsync(x => x.Id == serviceRequestDto.ServiceRequestCategoryId);

            if (serviceRequestCategory == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request category does not exist");
            }

            var isServiceRequestAdmin = await _permissionService.UserHasPermissionAsync(userAndOrganizationDto, AdministrationPermissions.ServiceRequest);
            var isServiceRequestCreator = serviceRequest.EmployeeId == userAndOrganizationDto.UserId;
            var isServiceRequestCategoryAssignee = (await GetCategoryAssigneesAsync(serviceRequest.CategoryName)).Contains(userAndOrganizationDto.UserId);

            var statusHasBeenChanged = serviceRequest.StatusId != serviceRequestDto.StatusId && isServiceRequestAdmin;

            if (!isServiceRequestAdmin && !isServiceRequestCreator && !isServiceRequestCategoryAssignee)
            {
                throw new UnauthorizedAccessException();
            }

            if (serviceRequest.CategoryName == ServiceRequestCategoryKudos && serviceRequestCategory.Name != ServiceRequestCategoryKudos)
            {
                throw new ValidationException(ErrorCodes.InvalidCategoryChange, $"Cannot change from {ServiceRequestCategoryKudos} category to {serviceRequestCategory.Name}");
            }

            if (serviceRequest.CategoryName != ServiceRequestCategoryKudos && serviceRequestCategory.Name == ServiceRequestCategoryKudos)
            {
                throw new ValidationException(ErrorCodes.InvalidCategoryChange, $"Cannot change from {serviceRequest.CategoryName} category to {ServiceRequestCategoryKudos}");
            }

            if (isServiceRequestAdmin || isServiceRequestCategoryAssignee)
            {
                serviceRequest.Title = serviceRequestDto.Title;
                serviceRequest.StatusId = serviceRequestDto.StatusId;
                serviceRequest.CategoryName = serviceRequestCategory.Name;
                serviceRequest.KudosAmmount = serviceRequest.KudosShopItemId == null ? serviceRequestDto.KudosAmmount : serviceRequest.KudosAmmount;
            }

            serviceRequest.PriorityId = serviceRequestDto.PriorityId;
            serviceRequest.Description = serviceRequestDto.Description;
            serviceRequest.PictureId = serviceRequestDto.PictureId;
            serviceRequest.UpdateMetadata(userAndOrganizationDto.UserId);

            await _uow.SaveChangesAsync(false);

            if (!statusHasBeenChanged)
            {
                return;
            }

            var statusDto = new UpdatedServiceRequestDto
            {
                ServiceRequestId = serviceRequestDto.Id,
                NewStatusId = serviceRequest.StatusId
            };

            _asyncRunner.Run<IServiceRequestNotificationService>(async notifier => await notifier.NotifyAboutServiceRequestStatusUpdateAsync(statusDto, userAndOrganizationDto), _uow.ConnectionName);
        }

        public async Task CreateCommentAsync(ServiceRequestCommentDto comment, UserAndOrganizationDto userAndOrganizationDto)
        {
            var serviceRequest = await _serviceRequestsDbSet
                    .SingleOrDefaultAsync(x => x.Id == comment.ServiceRequestId && x.OrganizationId == userAndOrganizationDto.OrganizationId);

            if (serviceRequest == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request does not exist");
            }

            var timestamp = DateTime.UtcNow;

            var serviceRequestComment = new ServiceRequestComment
            {
                Content = comment.Content,
                EmployeeId = userAndOrganizationDto.UserId,
                OrganizationId = userAndOrganizationDto.OrganizationId,
                ServiceRequest = serviceRequest,
                CreatedBy = userAndOrganizationDto.UserId,
                ModifiedBy = userAndOrganizationDto.UserId,
                Modified = timestamp,
                Created = timestamp
            };

            _serviceRequestCommentsDbSet.Add(serviceRequestComment);
            await _uow.SaveChangesAsync(false);

            var createdComment = new ServiceRequestCreatedCommentDto
            {
                ServiceRequestId = comment.ServiceRequestId,
                CommentedEmployeeId = serviceRequestComment.EmployeeId,
                CommentContent = serviceRequestComment.Content
            };

            _asyncRunner.Run<IServiceRequestNotificationService>(async notifier => await notifier.NotifyAboutNewCommentAsync(createdComment), _uow.ConnectionName);
        }

        public async Task<IEnumerable<ServiceRequestCategoryDto>> GetCategoriesAsync()
        {
            var categories = await _serviceRequestCategoryDbSet
                .Include(x => x.Assignees)
                .Select(x => new ServiceRequestCategoryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsNecessary = x.Name == ServiceRequestCategoryKudos,
                    Assignees = x.Assignees.Select(u => new ApplicationUserMinimalDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        PictureId = u.PictureId
                    })
                })
                .ToListAsync();

            return categories;
        }

        public async Task CreateCategoryAsync(ServiceRequestCategoryDto category, string userId)
        {
            await ValidateCategoryNameAsync(category.Name);
            var assignees = category.Assignees.Select(x => x.Id).ToList();

            var serviceCategory = new ServiceRequestCategory
            {
                Name = category.Name,
                Assignees = _userDbSet.Where(u => assignees.Contains(u.Id)).ToList()
            };

            _serviceRequestCategoryDbSet.Add(serviceCategory);

            await _uow.SaveChangesAsync(userId);
        }

        public async Task<ServiceRequestCategoryDto> GetCategoryAsync(int categoryId)
        {
            var category = await _serviceRequestCategoryDbSet
                .Where(x => x.Id == categoryId)
                .Include(x => x.Assignees)
                .Select(x => new ServiceRequestCategoryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsNecessary = x.Name == ServiceRequestCategoryKudos,
                    Assignees = x.Assignees.Select(u => new ApplicationUserMinimalDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        PictureId = u.PictureId
                    })
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request category does not exist");
            }

            return category;
        }

        public async Task EditCategoryAsync(ServiceRequestCategoryDto modelDto, string userId)
        {
            await ValidateCategoryNameAsync(modelDto.Name, modelDto.Id);
            var category = await _serviceRequestCategoryDbSet
                .Where(c => c.Id == modelDto.Id)
                .Include(x => x.Assignees)
                .FirstOrDefaultAsync();

            if (category == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request category does not exist");
            }

            if (category.Name != ServiceRequestCategoryKudos)
            {
                category.Name = modelDto.Name;
            }

            var assigneeIds = modelDto.Assignees.Select(y => y.Id).ToList();
            category.Assignees = await _userDbSet.Where(x => assigneeIds.Contains(x.Id)).ToListAsync();

            await _uow.SaveChangesAsync(userId);
        }

        public async Task DeleteCategoryAsync(int categoryId, string userId)
        {
            var category = await _serviceRequestCategoryDbSet
                .Where(c => c.Id == categoryId && c.Name != ServiceRequestCategoryKudos)
                .Include(x => x.Assignees)
                .FirstOrDefaultAsync();

            if (category == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request category does not exist");
            }

            _serviceRequestCategoryDbSet.Remove(category);
            await _uow.SaveChangesAsync(userId);
        }

        private async Task<List<string>> GetCategoryAssigneesAsync(string categoryName)
        {
            var assignees = await _serviceRequestCategoryDbSet
                .Where(x => x.Name == categoryName)
                .Include(x => x.Assignees)
                .Select(x => x.Assignees)
                .FirstOrDefaultAsync();

            if (assignees == null)
            {
                return new List<string>();
            }

            return assignees.Select(x => x.Id).ToList();
        }

        private async Task ValidateServiceRequestForUpdateAsync(ServiceRequestDto serviceRequest)
        {
            var isServiceRequestStatusIdCorrect = await _serviceRequestStatusDbSet.FirstOrDefaultAsync(x => x.Id == serviceRequest.StatusId);

            if (isServiceRequestStatusIdCorrect == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request status does not exist");
            }
        }

        private async Task ValidateServiceRequestForCreateAsync(ServiceRequestDto newServiceRequest)
        {
            var isServiceRequestPriorityIdCorrect = await _serviceRequestPriorityDbSet.AnyAsync(x => x.Id == newServiceRequest.PriorityId);

            if (!isServiceRequestPriorityIdCorrect)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request priority does not exist");
            }
        }

        private async Task ValidateCategoryNameAsync(string categoryName, int id = 0)
        {
            var isNameAlreadyUsed = await _serviceRequestCategoryDbSet.AnyAsync(x => x.Name == categoryName && x.Id != id);

            if (isNameAlreadyUsed)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "category name already exists");
            }
        }
    }
}