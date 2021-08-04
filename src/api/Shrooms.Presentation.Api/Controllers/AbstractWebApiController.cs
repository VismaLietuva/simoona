using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.Api.Filters;
using X.PagedList;

namespace Shrooms.Presentation.Api.Controllers
{
    public abstract class AbstractWebApiController<TModel, TViewModel, TPostViewModel> : BaseController
        where TModel : BaseModel, ISoftDelete
        where TViewModel : AbstractViewModel
        where TPostViewModel : AbstractViewModel
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IRepository<TModel> _repository;

        protected readonly ShroomsUserManager _userManager;

        private readonly string _defaultOrderByProperty;
        protected readonly IMapper _mapper;

        protected AbstractWebApiController(IMapper mapper, IUnitOfWork unitOfWork, ShroomsUserManager userManager, string defaultOrderByProperty = null)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.GetRepository<TModel>();
            _userManager = userManager;
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
        public virtual async Task<HttpResponseMessage> Get(int id, string includeProperties = "")
        {
            var model = await _repository.Get(f => f.Id == id, includeProperties: includeProperties).FirstOrDefaultAsync();
            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, Resources.Common.NotFound);
            }

            return Request.CreateResponse(HttpStatusCode.OK, _mapper.Map<TModel, TViewModel>(model));
        }

        [HttpGet]
        public virtual async Task<IEnumerable<TViewModel>> GetAll(int maxResults = 0, string orderBy = null, string includeProperties = null)
        {
            var model = await _repository.Get(maxResults: maxResults, orderBy: orderBy ?? _defaultOrderByProperty, includeProperties: includeProperties).ToListAsync();
            return _mapper.Map<IEnumerable<TModel>, IEnumerable<TViewModel>>(model);
        }

        [HttpGet]
        public virtual async Task<PagedViewModel<TViewModel>> GetPaged(string includeProperties = null,
            int page = 1,
            int pageSize = WebApiConstants.DefaultPageSize,
            string sort = null,
            string dir = "",
            string s = "")
        {
            return await GetFilteredPaged(includeProperties, page, pageSize, sort, dir);
        }

        protected virtual async Task<PagedViewModel<TViewModel>> GetFilteredPaged(string includeProperties = null,
            int page = 1,
            int pageSize = WebApiConstants.DefaultPageSize,
            string sort = null,
            string dir = "",
            Expression<Func<TModel, bool>> filter = null)
        {
            var sortQuery = string.IsNullOrEmpty(sort) ? null : $"{sort} {dir}";

            var models = await _repository
                .Get(includeProperties: includeProperties, filter: filter, orderBy: sortQuery ?? _defaultOrderByProperty)
                .ToPagedListAsync(page, pageSize);

            var abstractViewModels = _mapper.Map<IEnumerable<TModel>, IEnumerable<TViewModel>>(models);
            var pagedVm = new StaticPagedList<TViewModel>(abstractViewModels, models.PageNumber, models.PageSize, models.TotalItemCount);

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
        public virtual async Task<HttpResponseMessage> Post([FromBody] TPostViewModel crudViewModel)
        {
            if (crudViewModel == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            // can not create new item with same id
            if (await _repository.GetByIdAsync(crudViewModel.Id) != null)
            {
                return Request.CreateResponse(HttpStatusCode.Conflict);
            }

            var model = _mapper.Map<TPostViewModel, TModel>(crudViewModel);
            _repository.Insert(model);
            await _unitOfWork.SaveAsync();
            crudViewModel.Id = model.Id;

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpPut]
        [ValidationFilter]
        public virtual async Task<HttpResponseMessage> Put([FromBody] TPostViewModel crudViewModel)
        {
            if (crudViewModel == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var model = await _repository.GetByIdAsync(crudViewModel.Id);

            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            _mapper.Map(crudViewModel, model);
            _repository.Update(model);
            await _unitOfWork.SaveAsync();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpDelete]
        public virtual async Task<HttpResponseMessage> Delete(int id)
        {
            var model = await _repository.GetByIdAsync(id);

            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            _repository.Delete(model);
            await _unitOfWork.SaveAsync();
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
