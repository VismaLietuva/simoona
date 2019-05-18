using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Shrooms.Authentification;
using Shrooms.Constants.WebApi;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.DAL;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.PostModels;

namespace Shrooms.API.Controllers.WebApi
{
    public class AbstractClassifierController : AbstractWebApiController<AbstractClassifier, AbstractClassifierViewModel, AbstractClassifierPostViewModel>
    {
        private AbstractClassifier _classifierModel;
        private readonly IRepository<AbstractClassifier> _classifierRepository;

        public AbstractClassifierController(IMapper mapper, IUnitOfWork unitOfWork, ShroomsUserManager userManager, ShroomsRoleManager roleManager)
            : base(mapper, unitOfWork, userManager, roleManager, null)
        {
            _classifierRepository = unitOfWork.GetRepository<AbstractClassifier>();
        }

        [AllowAnonymous]
        public override HttpResponseMessage Post(AbstractClassifierPostViewModel crudViewModel)
        {
            if (Repository.GetByID(crudViewModel.Id) != null)
            {
                return Request.CreateResponse(HttpStatusCode.Conflict);
            }

            var specificType = Type.GetType("DataLayer.Models." + crudViewModel.AbstractClassifierType + ",DataLayer");
            _classifierModel = _mapper.Map(crudViewModel, _classifierModel, crudViewModel.GetType(), specificType) as AbstractClassifier;

            //TODO Need to fix Child saving issue when saving abstract classifier after removing childs
            RemoveAllChilds(_classifierModel);
            UpdateChilds(crudViewModel, _classifierModel, specificType);

            Repository.Insert(_classifierModel);
            UnitOfWork.Save();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        public override HttpResponseMessage Put(AbstractClassifierPostViewModel crudViewModel)
        {
            var model = Repository.GetByID(crudViewModel.Id);

            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var specificType = Type.GetType("DataLayer.Models." + crudViewModel.AbstractClassifierType + ",DataLayer");
            model = _mapper.Map(crudViewModel, model, crudViewModel.GetType(), specificType) as AbstractClassifier;

            //TODO Need to fix Child saving issue when saving abstract classifier after removing childs
            RemoveAllChilds(model);
            UpdateChilds(crudViewModel, model, specificType);

            Repository.Update(model);
            UnitOfWork.Save();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        public override PagedViewModel<AbstractClassifierViewModel> GetPaged(string includeProperties = null, int page = 1, int pageSize = ConstWebApi.DefaultPageSize, string sort = "Name Asc, OrganizationId Asc", string dir = "", string s = "")
        {
            return GetFilteredPaged(includeProperties, page, pageSize, sort, dir, f => f.Name.Contains(s));
        }

        public IEnumerable<AbstractClassifierTypeViewModel> GetAbstractClassifierTypes()
        {
            List<AbstractClassifierTypeViewModel> abstractClassifierTypes = new List<AbstractClassifierTypeViewModel>();
            var values = ConstWebApi.AbstractClassifierTypes;

            foreach (var value in values)
            {
                AbstractClassifierTypeViewModel abstractClassifierTypeModel = new AbstractClassifierTypeViewModel();

                abstractClassifierTypeModel.AbstractClassifierType = value;

                abstractClassifierTypes.Add(abstractClassifierTypeModel);
            }

            return abstractClassifierTypes;
        }

        [HttpGet]
        public IEnumerable<AbstractClassifierViewModel> GetChildsForAutoComplete(string search, int id = 0)
        {
            IEnumerable<AbstractClassifierViewModel> childsViewModel;

            if (!string.IsNullOrEmpty(search))
            {
                var childs = _classifierRepository.Get(o => o.Name.Contains(search) && o.Id != id);
                childsViewModel = _mapper.Map<IEnumerable<AbstractClassifier>, IEnumerable<AbstractClassifierViewModel>>(childs);
            }
            else
            {
                return null;
            }

            return childsViewModel;
        }

        public IEnumerable<AbstractClassifierViewModel> GetClassifiersWithoutMe()
        {
            IEnumerable<AbstractClassifierViewModel> childsViewModel;
            var childs = _classifierRepository.Get();
            childsViewModel = _mapper.Map<IEnumerable<AbstractClassifier>, IEnumerable<AbstractClassifierViewModel>>(childs);
            return childsViewModel;
        }

        private void UpdateChilds(AbstractClassifierAbstractViewModel crudViewModel, AbstractClassifier classifierModel, Type specificType)
        {
            Repository.Update(classifierModel);

            if (classifierModel == null || crudViewModel == null)
            {
                return;
            }

            foreach (AbstractClassifierAbstractViewModel childViewModel in crudViewModel.Childs)
            {
                AbstractClassifier model = _mapper.Map(childViewModel, childViewModel.GetType(), specificType) as AbstractClassifier;
                model.ParentId = classifierModel.Id;
                Repository.Update(model);
            }
        }

        private void RemoveAllChilds(AbstractClassifier classifierModel)
        {
            classifierModel = Repository.Get(f => f.Id == classifierModel.Id, includeProperties: "Childs").FirstOrDefault();
            classifierModel.Childs = null;
            Repository.Update(classifierModel);
        }
    }
}