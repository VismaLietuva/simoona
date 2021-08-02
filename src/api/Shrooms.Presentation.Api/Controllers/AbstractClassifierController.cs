using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.PostModels;

namespace Shrooms.Presentation.Api.Controllers
{
    public class AbstractClassifierController : AbstractWebApiController<AbstractClassifier, AbstractClassifierViewModel, AbstractClassifierPostViewModel>
    {
        private AbstractClassifier _classifierModel;
        private readonly IRepository<AbstractClassifier> _classifierRepository;

        public AbstractClassifierController(IMapper mapper, IUnitOfWork unitOfWork, ShroomsUserManager userManager, ShroomsRoleManager roleManager)
            : base(mapper, unitOfWork, userManager, roleManager)
        {
            _classifierRepository = unitOfWork.GetRepository<AbstractClassifier>();
        }

        [AllowAnonymous]
        public override HttpResponseMessage Post(AbstractClassifierPostViewModel crudViewModel)
        {
            if (_repository.GetByID(crudViewModel.Id) != null)
            {
                return Request.CreateResponse(HttpStatusCode.Conflict);
            }

            var specificType = Type.GetType("DataLayer.Models." + crudViewModel.AbstractClassifierType + ",DataLayer");
            _classifierModel = _mapper.Map(crudViewModel, _classifierModel, crudViewModel.GetType(), specificType) as AbstractClassifier;

            //TODO Need to fix Child saving issue when saving abstract classifier after removing children
            RemoveAllChildren(_classifierModel);
            UpdateChildren(crudViewModel, _classifierModel, specificType);

            _repository.Insert(_classifierModel);
            _unitOfWork.Save();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        public override HttpResponseMessage Put(AbstractClassifierPostViewModel crudViewModel)
        {
            var model = _repository.GetByID(crudViewModel.Id);

            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var specificType = Type.GetType("DataLayer.Models." + crudViewModel.AbstractClassifierType + ",DataLayer");
            model = _mapper.Map(crudViewModel, model, crudViewModel.GetType(), specificType) as AbstractClassifier;

            //TODO Need to fix Child saving issue when saving abstract classifier after removing children
            RemoveAllChildren(model);
            UpdateChildren(crudViewModel, model, specificType);

            _repository.Update(model);
            _unitOfWork.Save();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        public override PagedViewModel<AbstractClassifierViewModel> GetPaged(string includeProperties = null, int page = 1, int pageSize = WebApiConstants.DefaultPageSize, string sort = null, string dir = "", string s = "")
        {
            if (sort == null)
            {
                sort = "Name Asc, OrganizationId Asc";
            }

            return GetFilteredPaged(includeProperties, page, pageSize, sort, dir, f => f.Name.Contains(s));
        }

        public IEnumerable<AbstractClassifierTypeViewModel> GetAbstractClassifierTypes()
        {
            var abstractClassifierTypes = new List<AbstractClassifierTypeViewModel>();
            var values = WebApiConstants.AbstractClassifierTypes;

            foreach (var value in values)
            {
                var abstractClassifierTypeModel = new AbstractClassifierTypeViewModel();

                abstractClassifierTypeModel.AbstractClassifierType = value;

                abstractClassifierTypes.Add(abstractClassifierTypeModel);
            }

            return abstractClassifierTypes;
        }

        [HttpGet]
        public IEnumerable<AbstractClassifierViewModel> GetChildrenForAutoComplete(string search, int id = 0)
        {
            IEnumerable<AbstractClassifierViewModel> childrenViewModel;

            if (!string.IsNullOrEmpty(search))
            {
                var children = _classifierRepository.Get(o => o.Name.Contains(search) && o.Id != id);
                childrenViewModel = _mapper.Map<IEnumerable<AbstractClassifier>, IEnumerable<AbstractClassifierViewModel>>(children);
            }
            else
            {
                return null;
            }

            return childrenViewModel;
        }

        public IEnumerable<AbstractClassifierViewModel> GetClassifiersWithoutMe()
        {
            var children = _classifierRepository.Get();
            var childrenViewModel = _mapper.Map<IEnumerable<AbstractClassifier>, IEnumerable<AbstractClassifierViewModel>>(children);
            return childrenViewModel;
        }

        private void UpdateChildren(AbstractClassifierAbstractViewModel crudViewModel, AbstractClassifier classifierModel, Type specificType)
        {
            _repository.Update(classifierModel);

            if (classifierModel == null || crudViewModel == null)
            {
                return;
            }

            foreach (var childViewModel in crudViewModel.Children)
            {
                var model = _mapper.Map(childViewModel, childViewModel.GetType(), specificType) as AbstractClassifier;

                if (model != null)
                {
                    model.ParentId = classifierModel.Id;
                    _repository.Update(model);
                }
            }
        }

        private void RemoveAllChildren(AbstractClassifier classifierModel)
        {
            var classifierModelId = classifierModel.Id;
            classifierModel = _repository.Get(f => f.Id == classifierModelId, includeProperties: "Children").FirstOrDefault();

            if (classifierModel != null)
            {
                classifierModel.Children = null;
                _repository.Update(classifierModel);
            }
        }
    }
}