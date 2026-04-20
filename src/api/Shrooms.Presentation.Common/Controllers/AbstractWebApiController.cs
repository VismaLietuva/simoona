using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.Common.Filters;
using X.PagedList;

namespace Shrooms.Presentation.Common.Controllers
{
    public abstract class AbstractWebApiController<TModel, TViewModel, TPostViewModel> : BaseController
        where TModel : BaseModel
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
        public virtual async Task<IActionResult> Get(int id, string includeProperties = "")
        {
            var model = await _repository.Get(f => f.Id == id, includeProperties: includeProperties).FirstOrDefaultAsync();
            if (model == null)
            {
                return BadRequest(Resources.Common.NotFound);
            }

            return Ok(_mapper.Map<TModel, TViewModel>(model));
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

            var query = _repository.Get(includeProperties: includeProperties, filter: filter, orderBy: sortQuery ?? _defaultOrderByProperty);
            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var models = new StaticPagedList<TModel>(items, page, pageSize, totalCount);

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
        public virtual async Task<IActionResult> Post([FromBody] TPostViewModel crudViewModel)
        {
            if (crudViewModel == null)
            {
                return BadRequest();
            }

            // can not create new item with same id
            if (await _repository.GetByIdAsync(crudViewModel.Id) != null)
            {
                return Conflict();
            }

            var model = _mapper.Map<TPostViewModel, TModel>(crudViewModel);
            _repository.Insert(model);
            await _unitOfWork.SaveAsync();
            crudViewModel.Id = model.Id;

            return StatusCode(201);
        }

        [HttpPut]
        [ValidationFilter]
        public virtual async Task<IActionResult> Put([FromBody] TPostViewModel crudViewModel)
        {
            if (crudViewModel == null)
            {
                return BadRequest();
            }

            var model = await _repository.GetByIdAsync(crudViewModel.Id);

            if (model == null)
            {
                return NotFound();
            }

            _mapper.Map(crudViewModel, model);
            _repository.Update(model);
            await _unitOfWork.SaveAsync();

            return StatusCode(201);
        }

        [HttpDelete]
        public virtual async Task<IActionResult> Delete(int id)
        {
            var model = await _repository.GetByIdAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            _repository.Delete(model);
            await _unitOfWork.SaveAsync();
            return Ok();
        }
    }
}
