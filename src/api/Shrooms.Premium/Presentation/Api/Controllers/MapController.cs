using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.DataTransferObjects.Models.OfficeMap;
using Shrooms.Premium.Domain.Services.OfficeMap;
using Shrooms.Premium.Presentation.WebViewModels.Map;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;
using X.PagedList;

namespace Shrooms.Premium.Presentation.Api.Controllers
{
    [Authorize]
    public class MapController : BaseController
    {
        private const string IncludeProperties = "Rooms,Rooms.RoomType,Rooms.ApplicationUsers,Organization";

        private readonly IMapper _mapper;
        private readonly IRepository<Floor> _floorRepository;
        private readonly IRepository<Office> _officeRepository;
        private readonly IRepository<RoomType> _roomTypeRepository;
        private readonly IOfficeMapService _officeMapService;

        public MapController(IMapper mapper, IUnitOfWork unitOfWork, IOfficeMapService officeMapService)
        {
            _mapper = mapper;
            _officeMapService = officeMapService;
            _floorRepository = unitOfWork.GetRepository<Floor>();
            _officeRepository = unitOfWork.GetRepository<Office>();
            _roomTypeRepository = unitOfWork.GetRepository<RoomType>();
        }

        [PermissionAuthorize(Permission = BasicPermissions.Map)]
        public async Task<MapViewModel> GetDefault()
        {
            var userAndOrg = GetUserAndOrganization();
            var userOfficeAndFloor = await _officeMapService.GetUserOfficeAndFloorAsync(userAndOrg.UserId);

            if (userOfficeAndFloor.FloorId.HasValue)
            {
                return await GetByFloor(userOfficeAndFloor.FloorId.Value);
            }

            var office = _officeRepository.Get(o => o.IsDefault).FirstOrDefault();

            if (office == null)
            {
                office = _officeRepository.Get().FirstOrDefault();
            }

            if (office != null)
            {
                return await GetByOffice(office.Id);
            }

            return new MapViewModel();
        }

        [PermissionAuthorize(Permission = BasicPermissions.Map)]
        public async Task<MapViewModel> GetByOffice(int officeId)
        {
            var floor = await _floorRepository.Get(f => f.OfficeId == officeId, includeProperties: IncludeProperties).FirstOrDefaultAsync();

            return await GetModelAsync(floor);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Map)]
        public async Task<MapViewModel> GetByRoom(int roomId)
        {
            var floor = await _floorRepository.Get(f => f.Rooms.Any(r => r.Id == roomId)).FirstOrDefaultAsync();

            return await GetModelAsync(floor);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Map)]
        public async Task<MapViewModel> GetByApplicationUser(string userName)
        {
            var floor = await _floorRepository.Get(f => f.Rooms.Any(r => r.ApplicationUsers.Any(e => e.UserName == userName))).FirstOrDefaultAsync();

            return await GetModelAsync(floor);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Map)]
        public async Task<MapViewModel> GetByFloor(int floorId)
        {
            var floor = await _floorRepository.Get(f => f.Id == floorId, includeProperties: IncludeProperties).FirstOrDefaultAsync();

            return await GetModelAsync(floor, true);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.OfficeUsers)]
        public async Task<IEnumerable<string>> GetUsersEmailsByOffice(int officeId)
        {
            var emails = await _officeMapService.GetEmailsByOfficeAsync(officeId);
            return emails;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.OfficeUsers)]
        public async Task<IEnumerable<string>> GetUsersEmailsByFloor(int floorId)
        {
            var emails = await _officeMapService.GetEmailsByFloorAsync(floorId);
            return emails;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.OfficeUsers)]
        public async Task<IEnumerable<string>> GetUsersEmailsByRoom(int roomId)
        {
            var emails = await _officeMapService.GetEmailsByRoomAsync(roomId);
            return emails;
        }

        [HttpGet, HttpPost]
        [PermissionAuthorize(Permission = BasicPermissions.OfficeUsers)]
        public async Task<PagedViewModel<OfficeUserDto>> GetPagedByFloor(int floorId, int page = 1, int pageSize = WebApiConstants.DefaultPageSize, string includeProperties = "")
        {
            var officeUsersDto = await _officeMapService.GetOfficeUsersAsync(floorId, includeProperties);

            var officeUserPagedViewModel = await officeUsersDto.ToPagedListAsync(page, pageSize);

            var pagedModel = new PagedViewModel<OfficeUserDto>
            {
                PagedList = officeUserPagedViewModel,
                PageCount = officeUserPagedViewModel.PageCount,
                ItemCount = officeUserPagedViewModel.TotalItemCount,
                PageSize = pageSize
            };

            return pagedModel;
        }

        private async Task<MapViewModel> GetModelAsync(Floor floor, bool includeProperties = false)
        {
            if (floor == null)
            {
                if (includeProperties)
                {
                    floor = await _floorRepository.Get(includeProperties: IncludeProperties).FirstOrDefaultAsync();
                }
                else
                {
                    floor = await _floorRepository.Get().FirstOrDefaultAsync();
                }
            }

            if (floor == null)
            {
                return null;
            }

            var office = await _officeRepository.GetByIdAsync(floor.OfficeId);
            var allOffices = await _officeRepository.Get(o => o.Floors.Any(), includeProperties: "Floors").ToListAsync();

            var model = new MapViewModel
            {
                Floor = _mapper.Map<Floor, MapFloorViewModel>(floor),
                AllOffices = _mapper.Map<IEnumerable<Office>, IEnumerable<MapOfficeViewModel>>(allOffices),
                Office = _mapper.Map<Office, MapOfficeViewModel>(office)
            };

            var roomTypes = _roomTypeRepository.Get(rt => rt.Rooms.Any(r => r.FloorId == floor.Id));
            model.Floor.RoomTypes = _mapper.Map<IEnumerable<RoomType>, IEnumerable<MapRoomTypeViewModel>>(roomTypes);

            return model;
        }
    }
}
