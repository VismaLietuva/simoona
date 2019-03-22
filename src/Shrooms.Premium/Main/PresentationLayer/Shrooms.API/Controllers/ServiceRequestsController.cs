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
using PagedList;
using Shrooms.API.Filters;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.WebApi;
using Shrooms.DataLayer;
using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.DataTransferObjects.Models.ServiceRequest;
using Shrooms.Domain.Services.KudosShop;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.ServiceRequests;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.KudosShop;
using Shrooms.WebViewModels.Models.ServiceRequests;
using Shrooms.Domain.Services.ServiceRequests.Export;

namespace Shrooms.API.Controllers
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
        public PagedViewModel<ServiceRequestViewModel> GetPagedFiltered(string includeProperties = null, int page = 1, int pageSize = ConstWebApi.DefaultPageSize, string sortBy = null, string sortOrder = "", string search = "", string priority = "", string status = "", string serviceRequestCategory = "")
        {
            if (search == null)
            {
                search = string.Empty;
            }

            if (priority == null)
            {
                priority = string.Empty;
            }

            if (status == null)
            {
                status = string.Empty;
            }

            if (serviceRequestCategory == null)
            {
                serviceRequestCategory = string.Empty;
            }

            if (_permissionService.UserHasPermission(GetUserAndOrganization(), AdministrationPermissions.ServiceRequest))
            {
                Expression<Func<ServiceRequest, bool>> filter = u =>
                    (u.Title.Contains(search) ||
                    u.Employee.FirstName.Contains(search) ||
                    u.Employee.LastName.Contains(search)) &&
                    u.Priority.Title.Contains(priority) &&
                    u.Status.Title.Contains(status) &&
                    (string.IsNullOrEmpty(serviceRequestCategory) || u.CategoryName == serviceRequestCategory);

                return GetFilteredPaged(includeProperties, page, pageSize, sortBy, sortOrder, filter);
            }

            var id = User.Identity.GetUserId();
            var assigneeCategoriesNames = _serviceRequestService
                    .GetCategories()
                    .Where(x => x.Assignees.Select(y => y.Id).Contains(id))
                    .Select(x => x.Name);
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

            var serviceRequestPage = GetFilteredPaged(includeProperties, page, pageSize, sortBy, sortOrder, filterForCurrentUser);
            foreach (var serviceRequest in serviceRequestPage.PagedList)
            {
                if (assigneeCategoriesNames.Contains(serviceRequest.CategoryName))
                {
                    serviceRequest.IsCloseable = true;
                }
            }

            return serviceRequestPage;
        }

        private PagedViewModel<ServiceRequestViewModel> GetFilteredPaged(string includeProperties = null, int page = 1, int pageSize = ConstWebApi.DefaultPageSize, string sort = null, string dir = "", Expression<Func<ServiceRequest, bool>> filter = null)
        {
            string sortQuery = string.IsNullOrEmpty(sort) ? null : string.Format("{0} {1}", sort, dir);

            IPagedList<ServiceRequest> models = _serviceRequestRepository.Get(
                includeProperties: includeProperties, filter: filter, orderBy: sortQuery ?? "Created")
                .ToPagedList(page, pageSize);

            var pagedVM = new StaticPagedList<ServiceRequestViewModel>(_mapper.Map<IEnumerable<ServiceRequest>, IEnumerable<ServiceRequestViewModel>>(models), models.PageNumber, models.PageSize, models.TotalItemCount);

            var result = new PagedViewModel<ServiceRequestViewModel>
            {
                PagedList = pagedVM,
                PageCount = pagedVM.PageCount,
                ItemCount = pagedVM.TotalItemCount,
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
        public IHttpActionResult KudosShopItemsExist()
        {
            return Ok(_kudosShopService.ItemsExist(GetUserAndOrganization()));
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.ServiceRequest)]
        public List<KudosShopItemViewModel> GetKudosShopItems()
        {
            var userOrganization = GetUserAndOrganization();
            var kudosShopItemsTask = Task.Run(async () => await _kudosShopService.GetAllItems(userOrganization));
            var kudosShopItems = _mapper.Map<IEnumerable<KudosShopItemDTO>, IEnumerable<KudosShopItemViewModel>>(kudosShopItemsTask.Result);
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
        public IHttpActionResult PostComment([FromBody] ServiceRequestCommentPostViewModel postModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var comment = _mapper.Map<ServiceRequestCommentPostViewModel, ServiceRequestCommentDTO>(postModel);

            try
            {
                _serviceRequestService.CreateComment(comment, GetUserAndOrganization());
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }

        [HttpPost]
        [PermissionAuthorize(BasicPermissions.ServiceRequest)]
        public IHttpActionResult Create(ServiceRequestCreateViewModel newServiceRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newServiceRequestDTO = _mapper.Map<ServiceRequestCreateViewModel, ServiceRequestDTO>(newServiceRequest);

            try
            {
                _serviceRequestService.CreateNewServiceRequest(newServiceRequestDTO, GetUserAndOrganization());
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }

        [HttpPut]
        [PermissionAuthorize(BasicPermissions.ServiceRequest)]
        public IHttpActionResult Update(ServiceRequestUpdateViewModel serviceRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var serviceRequestDTO = _mapper.Map<ServiceRequestUpdateViewModel, ServiceRequestDTO>(serviceRequest);

            try
            {
                _serviceRequestService.UpdateServiceRequest(serviceRequestDTO, GetUserAndOrganization());
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
        public IHttpActionResult MarkAsDone(int id)
        {
            try
            {
                _serviceRequestService.MoveRequestToDone(id, GetUserAndOrganization());
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
        public HttpResponseMessage Delete(int id)
        {
            var model = _serviceRequestRepository.GetByID(id);

            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            _serviceRequestRepository.Delete(model);
            _uow.Save();
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.ServiceRequest)]
        public IHttpActionResult GetServiceRequestCategories()
        {
            var serviceRequestCategoriesDto = _serviceRequestService.GetCategories();

            return Ok(serviceRequestCategoriesDto);
        }

        [HttpPost]
        [PermissionAuthorize(Permission = AdministrationPermissions.ServiceRequest)]
        public IHttpActionResult CreateCategory(ServiceRequestCategoryCreateViewModel category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newCategory = _mapper.Map<ServiceRequestCategoryCreateViewModel, ServiceRequestCategoryDTO>(category);
                _serviceRequestService.CreateCategory(newCategory, GetUserAndOrganization().UserId);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.ServiceRequest)]
        public IHttpActionResult EditCategory(int categoryId)
        {
            try
            {
                var categoryDto = _serviceRequestService.GetCategory(categoryId);

                return Ok(categoryDto);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [PermissionAuthorize(Permission = AdministrationPermissions.ServiceRequest)]
        public IHttpActionResult EditCategory(ServiceRequestCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var modelDto = _mapper.Map<ServiceRequestCategoryViewModel, ServiceRequestCategoryDTO>(model);
                _serviceRequestService.EditCategory(modelDto, GetUserAndOrganization().UserId);

                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpDelete]
        [PermissionAuthorize(Permission = AdministrationPermissions.ServiceRequest)]
        public IHttpActionResult RemoveCategory(int categoryId)
        {
            try
            {
                _serviceRequestService.DeleteCategory(categoryId, GetUserAndOrganization().UserId);

                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.ServiceRequest)]
        public IHttpActionResult GetServiceRequestsAsExcel()
        {
            var userId = GetUserAndOrganization().UserId;
            Expression<Func<ServiceRequest, bool>> filter = null;
            var isAdmin = _permissionService.UserHasPermission(GetUserAndOrganization(), AdministrationPermissions.ServiceRequest);
            if (!isAdmin)
            {
                var assigneeCategoriesNames = _serviceRequestService
                        .GetCategories()
                        .Where(x => x.Assignees.Select(y => y.Id).Contains(userId))
                        .Select(x => x.Name);
                filter = u =>
                    u.EmployeeId == userId || assigneeCategoriesNames.Contains(u.CategoryName);
            }

            try
            {
                var stream = new ByteArrayContent(_serviceRequestExportService.ExportToExcel(GetUserAndOrganization(), filter));
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