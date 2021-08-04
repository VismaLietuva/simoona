using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.PostModels;
using X.PagedList;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    public class QualificationLevelController : AbstractWebApiController<QualificationLevel, QualificationLevelViewModel, QualificationLevelPostViewModel>
    {
        public QualificationLevelController(IMapper mapper, IUnitOfWork unitOfWork)
            : base(mapper, unitOfWork)
        {
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.QualificationLevel)]
        public async Task<IEnumerable<QualificationLevelAutoCompleteViewModel>> GetForAutoComplete(string s, int pageSize = WebApiConstants.DefaultAutocompleteListSize)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return new List<QualificationLevelAutoCompleteViewModel>();
            }

            s = s.ToLowerInvariant();
            var users = await _repository
                    .Get(q => q.Name.ToLower().StartsWith(s))
                    .OrderBy(q => q.Id)
                    .ToPagedListAsync(1, pageSize);

            return _mapper.Map<IEnumerable<QualificationLevelAutoCompleteViewModel>>(users);
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.QualificationLevel)]
        public override async Task<HttpResponseMessage> Get(int id, string includeProperties = "")
        {
            return await base.Get(id, includeProperties);
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.QualificationLevel)]
        public override async Task<IEnumerable<QualificationLevelViewModel>> GetAll(int maxResults = 0, string orderBy = null, string includeProperties = null)
        {
            return await base.GetAll(maxResults, orderBy, includeProperties);
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.QualificationLevel)]
        public override async Task<PagedViewModel<QualificationLevelViewModel>> GetPaged(string includeProperties = null,
            int page = 1,
            int pageSize = WebApiConstants.DefaultPageSize,
            string sort = null,
            string dir = "",
            string s = "")
        {
            return await base.GetPaged(includeProperties, page, pageSize, sort, dir, s);
        }

        [PermissionAuthorize(BasicPermissions.QualificationLevel)]
        protected override async Task<PagedViewModel<QualificationLevelViewModel>> GetFilteredPaged(string includeProperties = null,
            int page = 1,
            int pageSize = WebApiConstants.DefaultPageSize,
            string sort = null,
            string dir = "",
            Expression<Func<QualificationLevel, bool>> filter = null)
        {
            return await base.GetFilteredPaged(includeProperties, page, pageSize, sort, dir, filter);
        }

        [HttpPost]
        [ValidationFilter]
        [PermissionAuthorize(Permission = AdministrationPermissions.QualificationLevel)]
        public override async Task<HttpResponseMessage> Post([FromBody] QualificationLevelPostViewModel crudViewModel)
        {
            return await base.Post(crudViewModel);
        }

        [HttpPut]
        [ValidationFilter]
        [PermissionAuthorize(Permission = AdministrationPermissions.QualificationLevel)]
        public override async Task<HttpResponseMessage> Put([FromBody] QualificationLevelPostViewModel crudViewModel)
        {
            return await base.Put(crudViewModel);
        }

        [HttpDelete]
        [PermissionAuthorize(Permission = AdministrationPermissions.QualificationLevel)]
        public override async Task<HttpResponseMessage> Delete(int id)
        {
            return await base.Delete(id);
        }
    }
}