using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using PagedList;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models.Skill;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [RoutePrefix("Skill")]
    public class SkillController : ApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Skill> _skillRepository;

        public SkillController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _skillRepository = unitOfWork.GetRepository<Skill>();
        }

        [ValidationFilter]
        [PermissionAuthorize(Permission = BasicPermissions.Skill)]
        public HttpResponseMessage Post(SkillPostViewModel model)
        {
            var skill = _skillRepository.Get(s => s.Title == model.Title).FirstOrDefault();
            if (skill == null)
            {
                skill = _mapper.Map<Skill>(model);
                _skillRepository.Insert(skill);
                _unitOfWork.Save();
            }

            return Request.CreateResponse(HttpStatusCode.Created, _mapper.Map<SkillMiniViewModel>(skill));
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Skill)]
        public IEnumerable<SkillAutoCompleteViewModel> GetForAutoComplete(string s, int pageSize = WebApiConstants.DefaultAutocompleteListSize)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return new List<SkillAutoCompleteViewModel>();
            }

            s = s.ToLowerInvariant();
            var skills = _skillRepository.Get(sk => sk.ShowInAutoComplete && sk.Title.ToLower().StartsWith(s))
                            .OrderBy(sk => sk.Title)
                            .ToPagedList(1, pageSize);

            return _mapper.Map<IEnumerable<SkillAutoCompleteViewModel>>(skills);
        }
    }
}
