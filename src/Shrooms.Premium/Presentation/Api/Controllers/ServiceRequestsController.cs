using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;
using Shrooms.Premium.DataTransferObjects.Models.ServiceRequest;
using Shrooms.Premium.Domain.Services.KudosShop;
using Shrooms.Premium.Domain.Services.ServiceRequests;
using Shrooms.Premium.Presentation.WebViewModels.KudosShop;
using Shrooms.Premium.Presentation.WebViewModels.ServiceRequests;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.Api.Filters;
using X.PagedList;

namespace Shrooms.Premium.Presentation.Api.Controllers
{
    [Authorize]
    public class ServiceRequestsController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<ServiceRequest> _serviceRequestRepository;
        private readonly IRepository<ServiceRequestCategory> _categoryRepository;
        private readonly IRepository<ServiceRequestPriority> _priorityRepository;
        private readonly IRepository<ServiceRequestStatus> _statusRepository;
        private readonly IRepository<ServiceRequestComment> _commentRepository;
        private readonly IKudosShopService _kudosShopService;
        private readonly IServiceRequestService _serviceRequestService;
        private readonly IServiceRequestExportService _serviceRequestExportService;
        private readonly IPermissionService _permissionService;
        private readonly IMapper _mapper;

        public ServiceRequestsController(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IKudosShopService kudosShopService,
            IPermissionService permissionService,
            IServiceRequestService serviceRequestService,
            IServiceRequestExportService serviceRequestExportService)
        {
            _categoryRepository = unitOfWork.GetRepository<ServiceRequestCategory>();
            _priorityRepository = unitOfWork.GetRepository<ServiceRequestPriority>();
            _statusRepository = unitOfWork.GetRepository<ServiceRequestStatus>();
            _commentRepository = unitOfWork.GetRepository<ServiceRequestComment>();
            _serviceRequestRepository = unitOfWork.GetRepository<ServiceRequest>();
            _kudosShopService = kudosShopService;
            _permissionService = permissionService;
            _serviceRequestService = serviceRequestService;
            _serviceRequestExportService = serviceRequestExportService;
            _uow = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.ServiceRequest)]
        public HttpResponseMessage Get(int id, string includeProperties = "")
        {
            var model = _serviceRequestRepository.Get(f => f.Id == id, includeProperties: includeProperties).FirstOrDefault();

            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, Resources.Common.NotFound);
            }

            return Request.CreateResponse(HttpStatusCode.OK, _mapper.Map<ServiceRequest, ServiceRequestViewModel>(model));
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.ServiceRequest)]
        public async Task<PagedViewModel<ServiceRequestViewModel>> GetPagedFiltered(string includeProperties = null, int page = 1, int pageSize = WebApiConstants.DefaultPageSize, string sortBy = null, string sortOrder = "", string search = "", string priority = "", string status = "", string serviceRequestCategory = "")
        {
            search ??= string.Empty;
            priority ??= string.Empty;
            status ??= string.Empty;
            serviceRequestCategory ??= string.Empty;

            if (await _permissionService.UserHasPermissionAsync(GetUserAndOrganization(), AdministrationPermissions.ServiceRequest))
            {
                Expression<Func<ServiceRequest, bool>> filter = u =>
                    (u.Title.Contains(search) ||
                    u.Employee.FirstName.Contains(search) ||
                    u.Employee.LastName.Contains(search)) &&
                    u.Priority.Title.Contains(priority) &&
                    u.Status.Title.Contains(status) &&
                    (string.IsNullOrEmpty(serviceRequestCategory) || u.CategoryName == serviceRequestCategory);

                return await GetFilteredPagedAsync(includeProperties, page, pageSize, sortBy, sortOrder, filter);
            }

            var id = User.Identity.GetUserId();
            var assigneeCategoriesNames = (await _serviceRequestService.GetCategoriesAsync())
                    .Where(x => x.Assignees.Select(y => y.Id).Contains(id))
                    .Select(x => x.Name)
                    .ToList();

            Expression<Func<ServiceRequest, bool>> filterForCurrentUser = u =>
                (u.EmployeeId == id &&
                u.Title.Contains(search) &&
                u.Priority.Title.Contains(priority) &&
                u.Status.Title.Contains(status) &&
                (string.IsNullOrEmpty(serviceRequestCategory) || u.CategoryName == serviceRequestCategory))
                ||
                    ((u.Title.Contains(search) ||
                    u.Employee.FirstName.Contains(search) ||
                    u.Employee.LastName.Contains(search)) &&
                    u.Priority.Title.Contains(priority) &&
                    u.Status.Title.Contains(status) &&
                    (string.IsNullOrEmpty(serviceRequestCategory) || u.CategoryName == serviceRequestCategory) &&
                    assigneeCategoriesNames.Contains(u.CategoryName));

            var serviceRequestPage = await GetFilteredPagedAsync(includeProperties, page, pageSize, sortBy, sortOrder, filterForCurrentUser);

            foreach (var serviceRequest in serviceRequestPage.PagedList)
            {
                if (assigneeCategoriesNames.Contains(serviceRequest.CategoryName))
                {
                    serviceRequest.IsCloseable = true;
                }
            }

            return serviceRequestPage;
        }

        private async Task<PagedViewModel<ServiceRequestViewModel>> GetFilteredPagedAsync(string includeProperties = null, int page = 1, int pageSize = WebApiConstants.DefaultPageSize, string sort = null, string dir = "", Expression<Func<ServiceRequest, bool>> filter = null)
        {
            var sortQuery = string.IsNullOrEmpty(sort) ? null : $"{sort} {dir}";

            var models = await _serviceRequestRepository
                .Get(includeProperties: includeProperties, filter: filter, orderBy: sortQuery ?? "Created")
                .ToPagedListAsync(page, pageSize);

            var pagedModel = new StaticPagedList<ServiceRequestViewModel>(_mapper.Map<IEnumerable<ServiceRequest>, IEnumerable<ServiceRequestViewModel>>(models), models.PageNumber, models.PageSize, models.TotalItemCount);

            var result = new PagedViewModel<ServiceRequestViewModel>
            {
                PagedList = pagedModel,
                PageCount = pagedModel.PageCount,
                ItemCount = pagedModel.TotalItemCount,
                PageSize = pageSize
            };

            return result;
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.ServiceRequest)]
        public List<ServiceRequestCategory> GetCategories()
        {
            return _categoryRepository.Get().OrderBy(cat => cat.Name).ToList();
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.ServiceRequest)]
        public List<ServiceRequestPriority> GetPriorities()
        {
            return _priorityRepository.Get().ToList();
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.ServiceRequest)]
        public async Task<IHttpActionResult> KudosShopItemsExist()
        {
            return Ok(await _kudosShopService.ItemsExistAsync(GetUserAndOrganization()));
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.ServiceRequest)]
        public List<KudosShopItemViewModel> GetKudosShopItems()
        {
            var userOrganization = GetUserAndOrganization();
            var kudosShopItemsTask = Task.Run(async () => await _kudosShopService.GetAllItemsAsync(userOrganization));
            var kudosShopItems = _mapper.Map<IEnumerable<KudosShopItemDto>, IEnumerable<KudosShopItemViewModel>>(kudosShopItemsTask.Result);
            return kudosShopItems.ToList();
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.ServiceRequest)]
        public List<ServiceRequestStatus> GetStatuses()
        {
            return _statusRepository.Get().ToList();
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.ServiceRequest)]
        public IEnumerable<ServiceRequestCommentViewModel> GetComments(int requestId)
        {
            var model = _commentRepository.Get(includeProperties: "Employee", filter: c => c.ServiceRequestId == requestId);
            return _mapper.Map<IEnumerable<ServiceRequestComment>, IEnumerable<ServiceRequestCommentViewModel>>(model);
        }

        [HttpPost]
        [PermissionAuthorize(BasicPermissions.ServiceRequest)]
        public async Task<IHttpActionResult> PostComment([FromBody] ServiceRequestCommentPostViewModel postModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var comment = _mapper.Map<ServiceRequestCommentPostViewModel, ServiceRequestCommentDto>(postModel);

            try
            {
                await _serviceRequestService.CreateCommentAsync(comment, GetUserAndOrganization());
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }

        [HttpPost]
        [PermissionAuthorize(BasicPermissions.ServiceRequest)]
        public async Task<IHttpActionResult> Create(ServiceRequestCreateViewModel newServiceRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newServiceRequestDto = _mapper.Map<ServiceRequestCreateViewModel, ServiceRequestDto>(newServiceRequest);

            try
            {
                await _serviceRequestService.CreateNewServiceRequestAsync(newServiceRequestDto, GetUserAndOrganization());
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }

        [HttpPut]
        [PermissionAuthorize(BasicPermissions.ServiceRequest)]
        public async Task<IHttpActionResult> Update(ServiceRequestUpdateViewModel serviceRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var serviceRequestDto = _mapper.Map<ServiceRequestUpdateViewModel, ServiceRequestDto>(serviceRequest);

            try
            {
                await _serviceRequestService.UpdateServiceRequestAsync(serviceRequestDto, GetUserAndOrganization());
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbidden();
            }

            return Ok();
        }

        [HttpPut]
        [PermissionAuthorize(BasicPermissions.ServiceRequest)]
        public async Task<IHttpActionResult> MarkAsDone(int id)
        {
            try
            {
                await _serviceRequestService.MoveRequestToDoneAsync(id, GetUserAndOrganization());
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbidden();
            }

            return Ok();
        }

        [HttpDelete]
        [PermissionAuthorize(Permission = AdministrationPermissions.ServiceRequest)]
        public async Task<HttpResponseMessage> Delete(int id)
        {
            var model = await _serviceRequestRepository.GetByIdAsync(id);

            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            _serviceRequestRepository.Delete(model);
            await _uow.SaveAsync();
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.ServiceRequest)]
        public async Task<IHttpActionResult> GetServiceRequestCategories()
        {
            var serviceRequestCategoriesDto = await _serviceRequestService.GetCategoriesAsync();

            return Ok(serviceRequestCategoriesDto);
        }

        [HttpPost]
        [PermissionAuthorize(Permission = AdministrationPermissions.ServiceRequest)]
        public async Task<IHttpActionResult> CreateCategory(ServiceRequestCategoryCreateViewModel category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newCategory = _mapper.Map<ServiceRequestCategoryCreateViewModel, ServiceRequestCategoryDto>(category);
                await _serviceRequestService.CreateCategoryAsync(newCategory, GetUserAndOrganization().UserId);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.ServiceRequest)]
        public async Task<IHttpActionResult> EditCategory(int categoryId)
        {
            try
            {
                var categoryDto = await _serviceRequestService.GetCategoryAsync(categoryId);

                return Ok(categoryDto);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [PermissionAuthorize(Permission = AdministrationPermissions.ServiceRequest)]
        public async Task<IHttpActionResult> EditCategory(ServiceRequestCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var modelDto = _mapper.Map<ServiceRequestCategoryViewModel, ServiceRequestCategoryDto>(model);
                await _serviceRequestService.EditCategoryAsync(modelDto, GetUserAndOrganization().UserId);

                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpDelete]
        [PermissionAuthorize(Permission = AdministrationPermissions.ServiceRequest)]
        public async Task<IHttpActionResult> RemoveCategory(int categoryId)
        {
            try
            {
                await _serviceRequestService.DeleteCategoryAsync(categoryId, GetUserAndOrganization().UserId);

                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.ServiceRequest)]
        public async Task<IHttpActionResult> GetServiceRequestsAsExcel()
        {
            var userId = GetUserAndOrganization().UserId;
            Expression<Func<ServiceRequest, bool>> filter = null;
            var isAdmin = await _permissionService.UserHasPermissionAsync(GetUserAndOrganization(), AdministrationPermissions.ServiceRequest);

            if (!isAdmin)
            {
                var assigneeCategoriesNames = (await _serviceRequestService.GetCategoriesAsync())
                        .Where(x => x.Assignees.Select(y => y.Id).Contains(userId))
                        .Select(x => x.Name);

                filter = u =>
                    u.EmployeeId == userId || assigneeCategoriesNames.Contains(u.CategoryName);
            }

            try
            {
                var stream = new ByteArrayContent(await _serviceRequestExportService.ExportToExcelAsync(GetUserAndOrganization(), filter));
                var result = new HttpResponseMessage(HttpStatusCode.OK) { Content = stream };
                return ResponseMessage(result);
            }
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}