using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using PagedList;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models;

namespace Shrooms.Presentation.Api.Controllers
{
    public abstract class AbstractWebApiController<TModel, TViewModel, TPostViewModel> : BaseController
        where TModel : BaseModel, ISoftDelete
        where TViewModel : AbstractViewModel
        where TPostViewModel : AbstractViewModel
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IRepository<TModel> _repository;

        protected ShroomsUserManager UserManager { get; set; }
        protected ShroomsRoleManager RoleManager { get; set; }

        protected readonly string _defaultOrderByProperty;
        protected readonly IMapper _mapper;

        protected AbstractWebApiController(IMapper mapper, IUnitOfWork unitOfWork, ShroomsUserManager userManager, ShroomsRoleManager roleManager, string defaultOrderByProperty = null)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.GetRepository<TModel>();
            UserManager = userManager;
            RoleManager = roleManager;
            _defaultOrderByProperty = defaultOrderByProperty;
        }

        protected AbstractWebApiController(IMapper mapper, IUnitOfWork unitOfWork, string defaultOrderByProperty = null)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.GetRepository<TModel>();
            _defaultOrderByProperty = defaultOrderByProperty;
        }

        [HttpGet]
        public virtual HttpResponseMessage Get(int id, string includeProperties = "")
        {
            var model = _repository.Get(f => f.Id == id, includeProperties: includeProperties).FirstOrDefault();
            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, Resources.Common.NotFound);
            }

            return Request.CreateResponse(HttpStatusCode.OK, _mapper.Map<TModel, TViewModel>(model));
        }

        [HttpGet]
        public virtual IEnumerable<TViewModel> GetAll(int maxResults = 0, string orderBy = null, string includeProperties = null)
        {
            var model = _repository.Get(maxResults: maxResults, orderBy: orderBy ?? _defaultOrderByProperty, includeProperties: includeProperties);
            return _mapper.Map<IEnumerable<TModel>, IEnumerable<TViewModel>>(model);
        }

        [HttpGet]
        public virtual PagedViewModel<TViewModel> GetPaged(string includeProperties = null, int page = 1,
            int pageSize = WebApiConstants.DefaultPageSize, string sort = null, string dir = "", string s = "")
        {
            return GetFilteredPaged(includeProperties, page, pageSize, sort, dir);
        }

        protected virtual PagedViewModel<TViewModel> GetFilteredPaged(
            string includeProperties = null, int page = 1, int pageSize = WebApiConstants.DefaultPageSize,
            string sort = null, string dir = "", Expression<Func<TModel, bool>> filter = null)
        {
            var sortQuery = string.IsNullOrEmpty(sort) ? null : $"{sort} {dir}";

            IPagedList<TModel> models = _repository.Get(
                includeProperties: includeProperties, filter: filter, orderBy: sortQuery ?? _defaultOrderByProperty)
                .ToPagedList(page, pageSize);

            var pagedVm = new StaticPagedList<TViewModel>(_mapper.Map<IEnumerable<TModel>, IEnumerable<TViewModel>>(models), models.PageNumber, models.PageSize, models.TotalItemCount);

            var result = new PagedViewModel<TViewModel>
            {
                PagedList = pagedVm,
                PageCount = pagedVm.PageCount,
                ItemCount = pagedVm.TotalItemCount,
                PageSize = pageSize
            };

            return result;
        }

        [HttpPost]
        [ValidationFilter]
        public virtual HttpResponseMessage Post([FromBody] TPostViewModel crudViewModel)
        {
            if (crudViewModel == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            // can not create new item with same id
            if (_repository.GetByID(crudViewModel.Id) != null)
            {
                return Request.CreateResponse(HttpStatusCode.Conflict);
            }

            var model = _mapper.Map<TPostViewModel, TModel>(crudViewModel);
            _repository.Insert(model);
            _unitOfWork.Save();
            crudViewModel.Id = model.Id;

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpPut]
        [ValidationFilter]
        public virtual HttpResponseMessage Put([FromBody] TPostViewModel crudViewModel)
        {
            if (crudViewModel == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var model = _repository.GetByID(crudViewModel.Id);

            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            _mapper.Map(crudViewModel, model);
            _repository.Update(model);
            _unitOfWork.Save();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpDelete]
        public virtual HttpResponseMessage Delete(int id)
        {
            var model = _repository.GetByID(id);

            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            _repository.Delete(model);
            _unitOfWork.Save();
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}