using AutoMapper;
using Microsoft.AspNet.SignalR;
using MoreLinq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.PostModels;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using X.PagedList;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    public class FloorController : AbstractWebApiController<Floor, FloorViewModel, FloorPostViewModel>
    {
        private readonly IRepository<Office> _officeRepository;
        private readonly IRepository<ApplicationUser> _applicationUserRepository;
        private readonly IRepository<Organization> _organizationRepository;

        public FloorController(IMapper mapper, IUnitOfWork unitOfWork)
            : base(mapper, unitOfWork)
        {
            _officeRepository = unitOfWork.GetRepository<Office>();
            _applicationUserRepository = unitOfWork.GetRepository<ApplicationUser>();
            _organizationRepository = unitOfWork.GetRepository<Organization>();
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Floor)]
        public override async Task<HttpResponseMessage> Post(FloorPostViewModel crudViewModel)
        {
            return await base.Post(crudViewModel);
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Floor)]
        public override async Task<HttpResponseMessage> Put(FloorPostViewModel crudViewModel)
        {
            return await base.Put(crudViewModel);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Floor)]
        public async Task<FloorViewModel> GetByRoom(int roomId)
        {
            var model = await _repository.Get(f => f.Rooms.Any(r => r.Id == roomId), 1).FirstOrDefaultAsync();
            return _mapper.Map<Floor, FloorViewModel>(model);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Floor)]
        public async Task<IEnumerable<FloorViewModel>> GetByOffice(int officeId)
        {
            var model = await _repository.Get(f => f.OfficeId == officeId, includeProperties: "Picture").ToListAsync();
            return _mapper.Map<IEnumerable<Floor>, IEnumerable<FloorViewModel>>(model);
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Floor)]
        public async Task<FloorViewPagedModel> GetAllFloors(int officeId, int page = 1, int pageSize = WebApiConstants.DefaultPageSize, string s = "", string sort = "Id", string dir = "")
        {
            var sortQuery = string.IsNullOrEmpty(sort) ? null : $"{sort} {dir}";
            s ??= string.Empty;

            var floors = await _repository.GetPagedAsync(f => (officeId == -1 ? f.OfficeId != -1 : f.OfficeId == officeId) && f.Name.Contains(s),
                                                        orderBy: sortQuery, includeProperties: "Rooms,Rooms.ApplicationUsers");

            var floorId = floors.Where(n => n != null).Select(n => n.Id).FirstOrDefault();

            var floorOrganizationId = await _applicationUserRepository
                .Get(n => n.Room != null && n.Room.FloorId == floorId)
                .Select(n => n.OrganizationId)
                .FirstOrDefaultAsync();

            var floorsViewModel = _mapper.Map<IEnumerable<Floor>, IEnumerable<FloorViewModel>>(floors).ToList();

            var organizationName = await _organizationRepository
                .Get(n => n.Id == floorOrganizationId)
                .Select(n => n.ShortName)
                .SingleOrDefaultAsync();

            foreach (var floorViewModel in floorsViewModel)
            {
                floorViewModel.OrganizationName = organizationName;
            }

            var pagedList = await floorsViewModel.ToPagedListAsync(page, pageSize);

            var floorsViewPagedModel = new FloorViewPagedModel
            {
                PagedList = pagedList,
                PageCount = pagedList.PageCount,
                ItemCount = pagedList.TotalItemCount,
                PageSize = pagedList.PageSize
            };

            if (officeId != -1)
            {
                var office = await _officeRepository.GetByIdAsync(officeId);
                floorsViewPagedModel.Office = _mapper.Map<Office, OfficeViewModel>(office);
            }

            floorsViewPagedModel.PagedList.ForEach(f => f.Rooms.ForEach(r =>
            {
                f.ApplicationUsersCount += r.ApplicationUsersCount;
            }));

            return floorsViewPagedModel;
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Floor)]
        public async Task<FloorViewPagedModel> GetPaged(int officeId, int page = 1, int pageSize = WebApiConstants.DefaultPageSize, string s = "", string sort = "Id", string dir = "")
        {
            var sortQuery = string.IsNullOrEmpty(sort) ? null : $"{sort} {dir}";
            s ??= string.Empty;

            var floors = await _repository.GetPagedAsync(f => (officeId == -1 ? f.OfficeId != -1 : f.OfficeId == officeId) && f.Name.Contains(s),
                                                        orderBy: sortQuery, includeProperties: "Picture,Rooms,Rooms.ApplicationUsers");

            var floorId = floors.Where(n => n != null).Select(n => n.Id).FirstOrDefault();

            var floorOrganizationId = await _applicationUserRepository.Get(n => n.Room != null && n.Room.FloorId == floorId).Select(n => n.OrganizationId).FirstOrDefaultAsync();

            var floorsViewModel = _mapper.Map<IEnumerable<Floor>, IEnumerable<FloorViewModel>>(floors).ToList();

            var organizationName = await _organizationRepository.Get(n => n.Id == floorOrganizationId).Select(n => n.ShortName).SingleOrDefaultAsync();

            foreach (var floorViewModel in floorsViewModel)
            {
                floorViewModel.OrganizationName = organizationName;
            }

            var pagedList = await floorsViewModel.ToPagedListAsync(page, pageSize);

            var floorsViewPagedModel = new FloorViewPagedModel
            {
                PagedList = pagedList,
                PageCount = pagedList.PageCount,
                ItemCount = pagedList.TotalItemCount,
                PageSize = pagedList.PageSize
            };

            if (officeId != -1)
            {
                var office = await _officeRepository.GetByIdAsync(officeId);
                floorsViewPagedModel.Office = _mapper.Map<Office, OfficeViewModel>(office);
            }

            floorsViewPagedModel.PagedList.ForEach(f => f.Rooms.ForEach(r =>
            {
                f.ApplicationUsersCount += r.ApplicationUsersCount;
            }));

            return floorsViewPagedModel;
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Floor)]
        public override async Task<HttpResponseMessage> Delete(int id)
        {
            var floor = await _repository.Get(filter: of => of.Id == id, includeProperties: "Rooms,Rooms.ApplicationUsers").FirstOrDefaultAsync();

            if (floor == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            floor.Rooms.ForEach(r =>
            {
                r.ApplicationUsers.ForEach(e => e.RoomId = null);
                r.FloorId = null;
            });

            _repository.Delete(floor);
            await _unitOfWork.SaveAsync();

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
