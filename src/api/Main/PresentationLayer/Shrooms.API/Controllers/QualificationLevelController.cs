using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using PagedList;
using Shrooms.API.Filters;
using Shrooms.Constants.WebApi;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.DAL;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.PostModels;

namespace Shrooms.API.Controllers
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
        public IEnumerable<QualificationLevelAutoCompleteViewModel> GetForAutoComplete(string s, int pageSize = WebApiConstants.DefaultAutocompleteListSize)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return new List<QualificationLevelAutoCompleteViewModel>();
            }

            s = s.ToLowerInvariant();
            var users = _repository
                    .Get(q => q.Name.ToLower().StartsWith(s))
                    .OrderBy(q => q.Id)
                    .ToPagedList(1, pageSize);

            return _mapper.Map<IEnumerable<QualificationLevelAutoCompleteViewModel>>(users);
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.QualificationLevel)]
        public override HttpResponseMessage Get(int id, string includeProperties = "")
        {
            return base.Get(id, includeProperties);
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.QualificationLevel)]
        public override IEnumerable<QualificationLevelViewModel> GetAll(int maxResults = 0, string orderBy = null, string includeProperties = null)
        {
            return base.GetAll(maxResults, orderBy, includeProperties);
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.QualificationLevel)]
        public override PagedViewModel<QualificationLevelViewModel> GetPaged(string includeProperties = null, int page = 1,
            int pageSize = WebApiConstants.DefaultPageSize, string sort = null, string dir = "", string s = "")
        {
            return base.GetPaged(includeProperties, page, pageSize, sort, dir, s);
        }

        [PermissionAuthorize(BasicPermissions.QualificationLevel)]
        protected override PagedViewModel<QualificationLevelViewModel> GetFilteredPaged(
            string includeProperties = null, int page = 1, int pageSize = WebApiConstants.DefaultPageSize,
            string sort = null, string dir = "", Expression<Func<QualificationLevel, bool>> filter = null)
        {
            return base.GetFilteredPaged(includeProperties, page, pageSize, sort, dir, filter);
        }

        [HttpPost]
        [ValidationFilter]
        [PermissionAuthorize(Permission = AdministrationPermissions.QualificationLevel)]
        public override HttpResponseMessage Post([FromBody] QualificationLevelPostViewModel crudViewModel)
        {
            return base.Post(crudViewModel);
        }

        [HttpPut]
        [ValidationFilter]
        [PermissionAuthorize(Permission = AdministrationPermissions.QualificationLevel)]
        public override HttpResponseMessage Put([FromBody] QualificationLevelPostViewModel crudViewModel)
        {
            return base.Put(crudViewModel);
        }

        [HttpDelete]
        [PermissionAuthorize(Permission = AdministrationPermissions.QualificationLevel)]
        public override HttpResponseMessage Delete(int id)
        {
            return base.Delete(id);
        }
    }
}