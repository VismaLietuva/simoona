using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Users;
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

        public void CreateNewServiceRequest(ServiceRequestDTO newServiceRequestDTO, UserAndOrganizationDTO userAndOrganizationDTO)
        {
            ValidateServiceRequestForCreate(newServiceRequestDTO);

            var serviceRequestStatusId = _serviceRequestStatusDbSet
                    .Where(x => x.Title.Equals("Open"))
                    .Select(x => x.Id)
                    .First();

            var serviceRequestCategory = _serviceRequestCategoryDbSet
                .FirstOrDefault(x => x.Id == newServiceRequestDTO.ServiceRequestCategoryId);

            if (serviceRequestCategory == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request category does not exist");
            }

            var timestamp = DateTime.UtcNow;

            var serviceRequest = new ServiceRequest
            {
                Description = newServiceRequestDTO.Description,
                Title = newServiceRequestDTO.Title,
                CreatedBy = userAndOrganizationDTO.UserId,
                ModifiedBy = userAndOrganizationDTO.UserId,
                EmployeeId = userAndOrganizationDTO.UserId,
                KudosAmmount = newServiceRequestDTO.KudosAmmount,
                OrganizationId = userAndOrganizationDTO.OrganizationId,
                CategoryName = serviceRequestCategory.Name,
                StatusId = serviceRequestStatusId,
                PriorityId = newServiceRequestDTO.PriorityId,
                Created = timestamp,
                Modified = timestamp,
                PictureId = newServiceRequestDTO.PictureId
            };
            if (newServiceRequestDTO.KudosShopItemId != null)
            {
                serviceRequest.KudosShopItemId = newServiceRequestDTO.KudosShopItemId;
            }

            _serviceRequestsDbSet.Add(serviceRequest);
            _uow.SaveChanges(false);

            var srqDto = new CreatedServiceRequestDTO { ServiceRequestId = serviceRequest.Id };
            _asyncRunner.Run<IServiceRequestNotificationService>(n => n.NotifyAboutNewServiceRequest(srqDto), _uow.ConnectionName);
        }

        public void MoveRequestToDone(int requestId, UserAndOrganizationDTO userAndOrganizationDTO)
        {
            var serviceRequest = _serviceRequestsDbSet
                .Include(x => x.Status)
                .FirstOrDefault(x => x.Id == requestId &&
                        x.OrganizationId == userAndOrganizationDTO.OrganizationId);

            var doneStatus = _serviceRequestStatusDbSet.Single(s => s.Title == "Done");

            if (serviceRequest == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request does not exist");
            }

            var isServiceRequestAdmin = _permissionService.UserHasPermission(userAndOrganizationDTO, AdministrationPermissions.ServiceRequest);
            var isServiceRequestCategoryAssignee = GetCategoryAssignees(serviceRequest.CategoryName).Contains(userAndOrganizationDTO.UserId);

            if ((!isServiceRequestAdmin && !isServiceRequestCategoryAssignee) || serviceRequest.StatusId == doneStatus.Id)
            {
                throw new UnauthorizedAccessException();
            }

            serviceRequest.Status = doneStatus;

            _uow.SaveChanges(false);

            var statusDto = new UpdatedServiceRequestDTO { ServiceRequestId = requestId, NewStatusId = serviceRequest.StatusId };
            _asyncRunner.Run<IServiceRequestNotificationService>(n => n.NotifyAboutServiceRequestStatusUpdate(statusDto, userAndOrganizationDTO), _uow.ConnectionName);
        }

        public void UpdateServiceRequest(ServiceRequestDTO serviceRequestDTO, UserAndOrganizationDTO userAndOrganizationDTO)
        {
            var serviceRequest = _serviceRequestsDbSet
                .Include(x => x.Status)
                .FirstOrDefault(x => x.Id == serviceRequestDTO.Id &&
                        x.OrganizationId == userAndOrganizationDTO.OrganizationId);

            ValidateServiceRequestForUpdate(serviceRequest, serviceRequestDTO);

            var serviceRequestCategory = _serviceRequestCategoryDbSet
                    .FirstOrDefault(x => x.Id == serviceRequestDTO.ServiceRequestCategoryId);

            if (serviceRequestCategory == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request category does not exist");
            }

            var isServiceRequestAdmin = _permissionService.UserHasPermission(userAndOrganizationDTO, AdministrationPermissions.ServiceRequest);
            var isServiceRequestCreator = serviceRequest.EmployeeId == userAndOrganizationDTO.UserId;
            var isServiceRequestCategoryAssignee = GetCategoryAssignees(serviceRequest.CategoryName).Contains(userAndOrganizationDTO.UserId);

            var statusHasBeenChanged = serviceRequest.StatusId != serviceRequestDTO.StatusId && isServiceRequestAdmin;

            if (!isServiceRequestAdmin && !isServiceRequestCreator && !isServiceRequestCategoryAssignee)
            {
                throw new UnauthorizedAccessException();
            }

            if (isServiceRequestAdmin || isServiceRequestCategoryAssignee)
            {
                serviceRequest.Title = serviceRequestDTO.Title;
                serviceRequest.StatusId = serviceRequestDTO.StatusId;
                serviceRequest.CategoryName = serviceRequestCategory.Name;
                serviceRequest.KudosAmmount = serviceRequestDTO.KudosAmmount;
            }

            serviceRequest.PriorityId = serviceRequestDTO.PriorityId;
            serviceRequest.Description = serviceRequestDTO.Description;
            serviceRequest.PictureId = serviceRequestDTO.PictureId;
            serviceRequest.UpdateMetadata(userAndOrganizationDTO.UserId);

            _uow.SaveChanges(false);

            if (statusHasBeenChanged)
            {
                var statusDto = new UpdatedServiceRequestDTO
                {
                    ServiceRequestId = serviceRequestDTO.Id,
                    NewStatusId = serviceRequest.StatusId
                };
                _asyncRunner.Run<IServiceRequestNotificationService>(n => n.NotifyAboutServiceRequestStatusUpdate(statusDto, userAndOrganizationDTO), _uow.ConnectionName);
            }
        }

        public void CreateComment(ServiceRequestCommentDTO comment, UserAndOrganizationDTO userAndOrganizationDTO)
        {
            var serviceRequest = _serviceRequestsDbSet
                    .SingleOrDefault(x => x.Id == comment.ServiceRequestId &&
                            x.OrganizationId == userAndOrganizationDTO.OrganizationId);

            if (serviceRequest == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request does not exist");
            }

            var timestamp = DateTime.UtcNow;

            var serviceRequestComment = new ServiceRequestComment
            {
                Content = comment.Content,
                EmployeeId = userAndOrganizationDTO.UserId,
                OrganizationId = userAndOrganizationDTO.OrganizationId,
                ServiceRequest = serviceRequest,
                CreatedBy = userAndOrganizationDTO.UserId,
                ModifiedBy = userAndOrganizationDTO.UserId,
                Modified = timestamp,
                Created = timestamp,
            };

            _serviceRequestCommentsDbSet.Add(serviceRequestComment);
            _uow.SaveChanges(false);

            var createdComment = new ServiceRequestCreatedCommentDTO
            {
                ServiceRequestId = comment.ServiceRequestId,
                CommentedEmployeeId = serviceRequestComment.EmployeeId,
                CommentContent = serviceRequestComment.Content
            };
            _asyncRunner.Run<IServiceRequestNotificationService>(n => n.NotifyAboutNewComment(createdComment), _uow.ConnectionName);
        }

        public IEnumerable<ServiceRequestCategoryDTO> GetCategories()
        {
            var categories = _serviceRequestCategoryDbSet
                .Include(x => x.Assignees)
                .Select(x => new ServiceRequestCategoryDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsNecessary = x.Name == ServiceRequestCategoryKudos,
                    Assignees = x.Assignees.Select(u => new UserDto
                    {
                        UserId = u.Id,
                        FullName = u.FirstName + " " + u.LastName,
                        PictureId = u.PictureId
                    })
                })
                .ToList();

            return categories;
        }

        public void CreateCategory(ServiceRequestCategoryDTO category, string userId)
        {
            ValidateCategoryName(category.Name);
            var assignees = category.Assignees.Select(x => x.UserId).ToList();
            var serviceCategory = new ServiceRequestCategory
            {
                Name = category.Name,
                Assignees = _userDbSet.Where(u => assignees.Contains(u.Id)).ToList()
            };

            _serviceRequestCategoryDbSet.Add(serviceCategory);

            _uow.SaveChanges(userId);
        }

        public ServiceRequestCategoryDTO GetCategory(int categoryId)
        {
            var category = _serviceRequestCategoryDbSet
                .Where(x => x.Id == categoryId)
                .Include(x => x.Assignees)
                .Select(x => new ServiceRequestCategoryDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsNecessary = x.Name == ServiceRequestCategoryKudos,
                    Assignees = x.Assignees.Select(u => new UserDto
                    {
                        UserId = u.Id,
                        FullName = u.FirstName + " " + u.LastName,
                        PictureId = u.PictureId
                    })
                })
                .FirstOrDefault();

            if (category == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request category does not exist");
            }

            return category;
        }

        public void EditCategory(ServiceRequestCategoryDTO modelDto, string userId)
        {
            ValidateCategoryName(modelDto.Name, modelDto.Id);
            var category = _serviceRequestCategoryDbSet
                .Where(c => c.Id == modelDto.Id)
                .Include(x => x.Assignees)
                .FirstOrDefault();

            if (category == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request category does not exist");
            }

            if (category.Name != ServiceRequestCategoryKudos)
            {
                category.Name = modelDto.Name;
            }
            var assigneeIds = modelDto.Assignees.Select(y => y.UserId).ToList();
            category.Assignees = _userDbSet.Where(x => assigneeIds.Contains(x.Id)).ToList();

            _uow.SaveChanges(userId);
        }

        public void DeleteCategory(int categoryId, string userId)
        {
            var category = _serviceRequestCategoryDbSet
                .Where(c => c.Id == categoryId && c.Name != ServiceRequestCategoryKudos)
                .Include(x => x.Assignees)
                .FirstOrDefault();

            if (category == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request category does not exist");
            }

            _serviceRequestCategoryDbSet.Remove(category);
            _uow.SaveChanges(userId);
        }

        private List<string> GetCategoryAssignees(string categoryName)
        {
            var assignees = _serviceRequestCategoryDbSet
                .Where(x => x.Name == categoryName)
                .Include(x => x.Assignees)
                .Select(x => x.Assignees)
                .FirstOrDefault();

            if (assignees == null)
            {
                return new List<string>();
            }

            return assignees.Select(x => x.Id).ToList();
        }

        private void ValidateServiceRequestForUpdate(ServiceRequest currentServiceRequest, ServiceRequestDTO serviceRequestDTO)
        {
            ValidateServiceRequestForCreate(serviceRequestDTO);

            var isServiceRequestStatusIdCorrect = _serviceRequestStatusDbSet
                                .FirstOrDefault(x => x.Id == serviceRequestDTO.StatusId);

            if (isServiceRequestStatusIdCorrect == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request status does not exist");
            }

            if (currentServiceRequest == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request does not exist");
            }

            if (currentServiceRequest.Status.Title == ServiceRequestStatusDone && currentServiceRequest.CategoryName == ServiceRequestCategoryKudos)
            {
                throw new ValidationException(PremiumErrorCodes.ServiceRequestIsClosed, "Kudos request status is done");
            }
        }

        private void ValidateServiceRequestForCreate(ServiceRequestDTO newServiceRequestDTO)
        {
            var isServiceRequestPriorityIdCorrect = _serviceRequestPriorityDbSet
                    .Any(x => x.Id == newServiceRequestDTO.PriorityId);

            if (!isServiceRequestPriorityIdCorrect)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Service request priority does not exist");
            }
        }

        private void ValidateCategoryName(string categoryName, int id = 0)
        {
            var isNameAlreadyUsed = _serviceRequestCategoryDbSet.Any(x => x.Name == categoryName && x.Id != id);

            if (isNameAlreadyUsed)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "category name already exists");
            }
        }
    }
}