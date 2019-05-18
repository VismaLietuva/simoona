using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using MoreLinq;
using PagedList;
using Shrooms.API.Filters;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.WebApi;
using Shrooms.Domain.Services.Permissions;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.DAL;
using Shrooms.WebViewModels.Models;

namespace Shrooms.API.Controllers.WebApi
{
    [Authorize]
    public class CertificateController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionService _permissionService;
        private readonly IRepository<Exam> _examRepository;
        private readonly IRepository<ApplicationUser> _userRepository;
        private readonly IRepository<Certificate> _certificateRepository;

        public CertificateController(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IPermissionService permissionService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _permissionService = permissionService;

            _examRepository = _unitOfWork.GetRepository<Exam>();
            _userRepository = _unitOfWork.GetRepository<ApplicationUser>();
            _certificateRepository = _unitOfWork.GetRepository<Certificate>();
            _permissionService = permissionService;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Certificate)]
        public IEnumerable<CertificateAutoCompleteViewModel> GetForAutocomplete(string s, int pageSize = ConstWebApi.DefaultAutocompleteListSize)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return new List<CertificateAutoCompleteViewModel>();
            }

            var start = s.ToLower();
            var certificates = _certificateRepository.Get(c => c.Name.ToLower().Contains(start), includeProperties: "Exams")
                .OrderBy(c => c.Name)
                .DistinctBy(c => c.Name)
                .ToPagedList(1, pageSize);
            return _mapper.Map<IEnumerable<CertificateAutoCompleteViewModel>>(certificates);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Certificate)]
        public IHttpActionResult Post(CertificatePostViewModel crudViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetUserAndOrganization().UserId;
            var isAdministrator = _permissionService.UserHasPermission(GetUserAndOrganization(), AdministrationPermissions.Certificate);
            if (!isAdministrator && crudViewModel.ApplicationUserId != currentUserId)
            {
                return BadRequest();
            }

            var certificate = _certificateRepository.Get(c => c.Name == crudViewModel.Name && c.ApplicationUserId == crudViewModel.ApplicationUserId,
                includeProperties: "Exams").FirstOrDefault();
            if (certificate == null)
            {
                certificate = _mapper.Map<Certificate>(crudViewModel);
                certificate.Exams = MapExams(crudViewModel);
                _certificateRepository.Insert(certificate);
            }
            else
            {
                certificate.InProgress = crudViewModel.InProgress;
                var examsToAdd = MapExams(crudViewModel);
                examsToAdd.ForEach(e => certificate.Exams.Add(e));
                _certificateRepository.Update(certificate);
            }

            _unitOfWork.Save();
            return Ok(_mapper.Map<CertificateMiniViewModel>(certificate));
        }

        private ICollection<Exam> MapExams(CertificatePostViewModel crudViewModel)
        {
            var examIds = _mapper.Map<int[]>(crudViewModel.Exams);
            return _examRepository.Get(e => examIds.Contains(e.Id)).ToList();
        }

        [HttpDelete]
        [PermissionAuthorize(Permission = BasicPermissions.Certificate)]
        public IHttpActionResult Delete(int id)
        {
            var currentUserId = GetUserAndOrganization().UserId;
            var isAdmin = _permissionService.UserHasPermission(GetUserAndOrganization(), AdministrationPermissions.Certificate);
            if (id <= 0)
            {
                return BadRequest();
            }

            var model = _certificateRepository.GetByID(id);
            if (model == null)
            {
                return NotFound();
            }

            if (model.ApplicationUserId != currentUserId && !isAdmin)
            {
                return BadRequest();
            }

            _certificateRepository.Delete(model);
            _unitOfWork.Save();

            return Ok();
        }

        [PermissionAuthorize(Permission = BasicPermissions.Certificate)]
        public IHttpActionResult Put(CertificatePostViewModel crudViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetUserAndOrganization().UserId;
            var isAdministrator = _permissionService.UserHasPermission(GetUserAndOrganization(), AdministrationPermissions.Certificate);
            if (!isAdministrator && crudViewModel.ApplicationUserId != currentUserId)
            {
                return BadRequest();
            }

            var certificate = _certificateRepository.Get(c => c.Id == crudViewModel.Id, includeProperties: "Exams").FirstOrDefault();
            if (certificate == null)
            {
                return NotFound();
            }

            certificate.Exams = MapExams(crudViewModel);
            certificate.InProgress = crudViewModel.InProgress;
            _certificateRepository.Update(certificate);
            _unitOfWork.Save();

            return Ok(_mapper.Map<CertificateMiniViewModel>(certificate));
        }
    }
}