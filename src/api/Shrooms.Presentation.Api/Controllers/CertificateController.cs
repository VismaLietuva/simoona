using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using MoreLinq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models.Certificate;
using X.PagedList;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    public class CertificateController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionService _permissionService;
        private readonly IRepository<Exam> _examRepository;
        private readonly IRepository<Certificate> _certificateRepository;

        public CertificateController(IMapper mapper, IUnitOfWork unitOfWork, IPermissionService permissionService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _permissionService = permissionService;

            _examRepository = _unitOfWork.GetRepository<Exam>();
            _certificateRepository = _unitOfWork.GetRepository<Certificate>();
            _permissionService = permissionService;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Certificate)]
        public async Task<IEnumerable<CertificateAutoCompleteViewModel>> GetForAutocomplete(string s, int pageSize = WebApiConstants.DefaultAutocompleteListSize)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return new List<CertificateAutoCompleteViewModel>();
            }

            var start = s.ToLower();
            var certificates = await _certificateRepository.Get(c => c.Name.ToLower().Contains(start), includeProperties: "Exams")
                .OrderBy(c => c.Name)
                .DistinctBy(c => c.Name)
                .ToPagedListAsync(1, pageSize);

            return _mapper.Map<IEnumerable<CertificateAutoCompleteViewModel>>(certificates);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Certificate)]
        public async Task<IHttpActionResult> Post(CertificatePostViewModel crudViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetUserAndOrganization().UserId;
            var isAdministrator = await _permissionService.UserHasPermissionAsync(GetUserAndOrganization(), AdministrationPermissions.Certificate);
            if (!isAdministrator && crudViewModel.ApplicationUserId != currentUserId)
            {
                return BadRequest();
            }

            var certificate = await _certificateRepository
                .Get(c => c.Name == crudViewModel.Name && c.ApplicationUserId == crudViewModel.ApplicationUserId, includeProperties: "Exams")
                .FirstOrDefaultAsync();

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

            await _unitOfWork.SaveAsync();
            return Ok(_mapper.Map<CertificateMiniViewModel>(certificate));
        }

        private ICollection<Exam> MapExams(CertificatePostViewModel crudViewModel)
        {
            var examIds = _mapper.Map<int[]>(crudViewModel.Exams);
            return _examRepository.Get(e => examIds.Contains(e.Id)).ToList();
        }

        [HttpDelete]
        [PermissionAuthorize(Permission = BasicPermissions.Certificate)]
        public async Task<IHttpActionResult> Delete(int id)
        {
            var currentUserId = GetUserAndOrganization().UserId;
            var isAdmin = await _permissionService.UserHasPermissionAsync(GetUserAndOrganization(), AdministrationPermissions.Certificate);
            if (id <= 0)
            {
                return BadRequest();
            }

            var model = await _certificateRepository.GetByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            if (model.ApplicationUserId != currentUserId && !isAdmin)
            {
                return BadRequest();
            }

            _certificateRepository.Delete(model);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [PermissionAuthorize(Permission = BasicPermissions.Certificate)]
        public async Task<IHttpActionResult> Put(CertificatePostViewModel crudViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetUserAndOrganization().UserId;
            var isAdministrator = await _permissionService.UserHasPermissionAsync(GetUserAndOrganization(), AdministrationPermissions.Certificate);
            if (!isAdministrator && crudViewModel.ApplicationUserId != currentUserId)
            {
                return BadRequest();
            }

            var certificate = await _certificateRepository.Get(c => c.Id == crudViewModel.Id, includeProperties: "Exams").FirstOrDefaultAsync();
            if (certificate == null)
            {
                return NotFound();
            }

            certificate.Exams = MapExams(crudViewModel);
            certificate.InProgress = crudViewModel.InProgress;
            _certificateRepository.Update(certificate);

            await _unitOfWork.SaveAsync();

            return Ok(_mapper.Map<CertificateMiniViewModel>(certificate));
        }
    }
}
