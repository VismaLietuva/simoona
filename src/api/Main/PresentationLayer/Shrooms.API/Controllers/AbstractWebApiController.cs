using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using PagedList;
using Shrooms.API.Filters;
using Shrooms.Authentification;
using Shrooms.Constants.WebApi;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.DAL;
using Shrooms.WebViewModels.Models;

namespace Shrooms.API.Controllers
{
    public abstract class AbstractWebApiController<TModel, TViewModel, TPostViewModel> : BaseController
        where TModel : BaseModel, ISoftDelete
        where TViewModel : AbstractViewModel
        where TPostViewModel : AbstractViewModel
    {
        protected readonly IUnitOfWork UnitOfWork;
        protected readonly IRepository<TModel> Repository;

        protected ShroomsUserManager UserManager { get; set; }
        protected ShroomsRoleManager RoleManager { get; set; }

        protected readonly string DefaultOrderByProperty;
        protected readonly IMapper _mapper;

        protected AbstractWebApiController(IMapper mapper, IUnitOfWork unitOfWork, ShroomsUserManager userManager, ShroomsRoleManager roleManager, string defaultOrderByProperty = null)
        {
            _mapper = mapper;
            UnitOfWork = unitOfWork;
            Repository = UnitOfWork.GetRepository<TModel>();
            UserManager = userManager;
            RoleManager = roleManager;
            DefaultOrderByProperty = defaultOrderByProperty;
        }

        protected AbstractWebApiController(IMapper mapper, IUnitOfWork unitOfWork, string defaultOrderByProperty = null)
        {
            _mapper = mapper;
            UnitOfWork = unitOfWork;
            Repository = UnitOfWork.GetRepository<TModel>();
            DefaultOrderByProperty = defaultOrderByProperty;
        }

        [HttpGet]
        public virtual HttpResponseMessage Get(int id, string includeProperties = "")
        {
            var model = Repository.Get(f => f.Id == id, includeProperties: includeProperties).FirstOrDefault();
            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, Resources.Common.NotFound);
            }

            return Request.CreateResponse(HttpStatusCode.OK, _mapper.Map<TModel, TViewModel>(model));
        }

        [HttpGet]
        public virtual IEnumerable<TViewModel> GetAll(int maxResults = 0, string orderBy = null, string includeProperties = null)
        {
            var model = Repository.Get(maxResults: maxResults, orderBy: orderBy ?? DefaultOrderByProperty, includeProperties: includeProperties);
            return _mapper.Map<IEnumerable<TModel>, IEnumerable<TViewModel>>(model);
        }

        [HttpGet]
        public virtual PagedViewModel<TViewModel> GetPaged(string includeProperties = null, int page = 1,
            int pageSize = ConstWebApi.DefaultPageSize, string sort = null, string dir = "", string s = "")
        {
            return GetFilteredPaged(includeProperties, page, pageSize, sort, dir);
        }

        protected virtual PagedViewModel<TViewModel> GetFilteredPaged(
            string includeProperties = null, int page = 1, int pageSize = ConstWebApi.DefaultPageSize,
            string sort = null, string dir = "", Expression<Func<TModel, bool>> filter = null)
        {
            var sortQuery = string.IsNullOrEmpty(sort) ? null : $"{sort} {dir}";

            IPagedList<TModel> models = Repository.Get(
                includeProperties: includeProperties, filter: filter, orderBy: sortQuery ?? DefaultOrderByProperty)
                .ToPagedList(page, pageSize);

            var pagedVM = new StaticPagedList<TViewModel>(_mapper.Map<IEnumerable<TModel>, IEnumerable<TViewModel>>(models), models.PageNumber, models.PageSize, models.TotalItemCount);

            var result = new PagedViewModel<TViewModel>
            {
                PagedList = pagedVM,
                PageCount = pagedVM.PageCount,
                ItemCount = pagedVM.TotalItemCount,
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
            if (Repository.GetByID(crudViewModel.Id) != null)
            {
                return Request.CreateResponse(HttpStatusCode.Conflict);
            }

            var model = _mapper.Map<TPostViewModel, TModel>(crudViewModel);
            Repository.Insert(model);
            UnitOfWork.Save();
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

            var model = Repository.GetByID(crudViewModel.Id);

            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            _mapper.Map(crudViewModel, model);
            Repository.Update(model);
            UnitOfWork.Save();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpDelete]
        public virtual HttpResponseMessage Delete(int id)
        {
            var model = Repository.GetByID(id);

            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            Repository.Delete(model);
            UnitOfWork.Save();
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}