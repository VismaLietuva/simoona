using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
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

        public OrganizationController(IMapper mapper, IUnitOfWork unitOfWork, ShroomsUserManager userManager, ShroomsRoleManager roleManager, IOrganizationService organizationService)
            : base(mapper, unitOfWork, userManager, roleManager)
        {
            _organizationService = organizationService;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Organization)]
        public override HttpResponseMessage Get(int id, string includeProperties = "")
        {
            return base.Get(id, includeProperties);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Organization)]
        public override IEnumerable<OrganizationViewModel> GetAll(int maxResults = 0, string orderBy = null, string includeProperties = null)
        {
            return base.GetAll(maxResults, orderBy, includeProperties);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Organization)]
        public override PagedViewModel<OrganizationViewModel> GetPaged(string includeProperties = null, int page = 1,
            int pageSize = WebApiConstants.DefaultPageSize, string sort = null, string dir = "", string s = "")
        {
            return base.GetPaged(includeProperties, page, pageSize, sort, dir, s);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Organization)]
        protected override PagedViewModel<OrganizationViewModel> GetFilteredPaged(
            string includeProperties = null, int page = 1, int pageSize = WebApiConstants.DefaultPageSize,
            string sort = null, string dir = "", Expression<Func<Organization, bool>> filter = null)
        {
            return base.GetFilteredPaged(includeProperties, page, pageSize, sort, dir, filter);
        }

        [HttpPost]
        [ValidationFilter]
        [PermissionAuthorize(Permission = AdministrationPermissions.Organization)]
        public override HttpResponseMessage Post([FromBody] OrganizationPostViewModel crudViewModel)
        {
            return base.Post(crudViewModel);
        }

        [HttpPut]
        [ValidationFilter]
        [PermissionAuthorize(Permission = AdministrationPermissions.Organization)]
        public override HttpResponseMessage Put([FromBody] OrganizationPostViewModel crudViewModel)
        {
            return base.Put(crudViewModel);
        }

        [HttpDelete]
        [PermissionAuthorize(Permission = AdministrationPermissions.Organization)]
        public override HttpResponseMessage Delete(int id)
        {
            return base.Delete(id);
        }

        [HttpGet]
        [PermissionAuthorize(AdministrationPermissions.Organization)]
        public IHttpActionResult GetManagingDirector()
        {
            var currentManagingDirector = _organizationService.GetManagingDirector(GetUserAndOrganization().OrganizationId);
            return Ok(currentManagingDirector);
        }

        [HttpPost]
        [PermissionAuthorize(AdministrationPermissions.Organization)]
        public IHttpActionResult SetManagingDirector(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("userId is required");
            }

            try
            {
                _organizationService.SetManagingDirector(userId, GetUserAndOrganization());
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }
    }
}
