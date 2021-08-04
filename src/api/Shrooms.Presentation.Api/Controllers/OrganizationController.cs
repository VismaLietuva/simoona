using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.PostModels;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    public class OrganizationController : AbstractWebApiController<Organization, OrganizationViewModel, OrganizationPostViewModel>
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(IMapper mapper, IUnitOfWork unitOfWork, ShroomsUserManager userManager, IOrganizationService organizationService)
            : base(mapper, unitOfWork, userManager)
        {
            _organizationService = organizationService;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Organization)]
        public override async Task<HttpResponseMessage> Get(int id, string includeProperties = "")
        {
            return await base.Get(id, includeProperties);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Organization)]
        public override async Task<IEnumerable<OrganizationViewModel>> GetAll(int maxResults = 0, string orderBy = null, string includeProperties = null)
        {
            return await base.GetAll(maxResults, orderBy, includeProperties);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Organization)]
        public override async Task<PagedViewModel<OrganizationViewModel>> GetPaged(string includeProperties = null,
            int page = 1,
            int pageSize = WebApiConstants.DefaultPageSize,
            string sort = null,
            string dir = "",
            string s = "")
        {
            return await base.GetPaged(includeProperties, page, pageSize, sort, dir, s);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Organization)]
        protected override async Task<PagedViewModel<OrganizationViewModel>> GetFilteredPaged(string includeProperties = null,
            int page = 1,
            int pageSize = WebApiConstants.DefaultPageSize,
            string sort = null,
            string dir = "",
            Expression<Func<Organization, bool>> filter = null)
        {
            return await base.GetFilteredPaged(includeProperties, page, pageSize, sort, dir, filter);
        }

        [HttpPost]
        [ValidationFilter]
        [PermissionAuthorize(Permission = AdministrationPermissions.Organization)]
        public override async Task<HttpResponseMessage> Post([FromBody] OrganizationPostViewModel crudViewModel)
        {
            return await base.Post(crudViewModel);
        }

        [HttpPut]
        [ValidationFilter]
        [PermissionAuthorize(Permission = AdministrationPermissions.Organization)]
        public override async Task<HttpResponseMessage> Put([FromBody] OrganizationPostViewModel crudViewModel)
        {
            return await base.Put(crudViewModel);
        }

        [HttpDelete]
        [PermissionAuthorize(Permission = AdministrationPermissions.Organization)]
        public override async Task<HttpResponseMessage> Delete(int id)
        {
            return await base.Delete(id);
        }

        [HttpGet]
        [PermissionAuthorize(AdministrationPermissions.Organization)]
        public async Task<IHttpActionResult> GetManagingDirector()
        {
            var currentManagingDirector = await _organizationService.GetManagingDirectorAsync(GetUserAndOrganization().OrganizationId);
            return Ok(currentManagingDirector);
        }

        [HttpPost]
        [PermissionAuthorize(AdministrationPermissions.Organization)]
        public async Task<IHttpActionResult> SetManagingDirector(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("userId is required");
            }

            try
            {
                await _organizationService.SetManagingDirectorAsync(userId, GetUserAndOrganization());
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }
    }
}
