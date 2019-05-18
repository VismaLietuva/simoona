using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using PagedList;
using Shrooms.API.Controllers;
using Shrooms.API.Filters;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Premium.Constants;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.OfficeMap;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.OfficeMap;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.Map;

namespace Shrooms.Premium.Main.PresentationLayer.API.Controllers
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
        public MapViewModel GetDefault()
        {
            var userAndOrg = GetUserAndOrganization();
            var userOfficeAndFloor = _officeMapService.GetUserOfficeAndFloor(userAndOrg.UserId);

            if (userOfficeAndFloor.FloorId.HasValue)
            {
                return GetByFloor(userOfficeAndFloor.FloorId.Value);
            }

            var office = _officeRepository.Get(o => o.IsDefault).FirstOrDefault();

            if (office == null)
            {
                office = _officeRepository.Get().FirstOrDefault();
            }

            if (office != null)
            {
                return GetByOffice(office.Id);
            }

            return new MapViewModel();
        }

        [PermissionAuthorize(Permission = BasicPermissions.Map)]
        public MapViewModel GetByOffice(int officeId)
        {
            var floor = _floorRepository.Get(f => f.OfficeId == officeId, includeProperties: IncludeProperties).FirstOrDefault();

            return GetModel(floor);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Map)]
        public MapViewModel GetByRoom(int roomId)
        {
            var floor = _floorRepository.Get(f => f.Rooms.Any(r => r.Id == roomId)).FirstOrDefault();

            return GetModel(floor);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Map)]
        public MapViewModel GetByApplicationUser(string userName)
        {
            var floor = _floorRepository.Get(f => f.Rooms.Any(r => r.ApplicationUsers.Any(e => e.UserName == userName))).FirstOrDefault();

            return GetModel(floor);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Map)]
        public MapViewModel GetByFloor(int floorId)
        {
            var floor = _floorRepository.Get(f => f.Id == floorId, includeProperties: IncludeProperties).FirstOrDefault();

            return GetModel(floor, true);
        }

        private MapViewModel GetModel(Floor floor, bool includeProperties = false)
        {
            if (floor == null)
            {
                if (includeProperties)
                {
                    floor = _floorRepository.Get(includeProperties: IncludeProperties).FirstOrDefault();
                }
                else
                {
                    floor = _floorRepository.Get().FirstOrDefault();
                }
            }

            if (floor == null)
            {
                return null;
            }

            var model = new MapViewModel
            {
                Floor = _mapper.Map<Floor, MapFloorViewModel>(floor),
                AllOffices = _mapper.Map<IEnumerable<Office>, IEnumerable<MapOfficeViewModel>>(_officeRepository.Get(o => o.Floors.Any(), includeProperties: "Floors")),
                Office = _mapper.Map<Office, MapOfficeViewModel>(_officeRepository.GetByID(floor.OfficeId))
            };

            var roomTypes = _roomTypeRepository.Get(rt => rt.Rooms.Any(r => r.FloorId == floor.Id));
            model.Floor.RoomTypes = _mapper.Map<IEnumerable<RoomType>, IEnumerable<MapRoomTypeViewModel>>(roomTypes);

            return model;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Office)]
        public IEnumerable<string> GetUsersEmailsByOffice(int officeId)
        {
            var emails = _officeMapService.GetEmailsByOffice(officeId);
            return emails;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Office)]
        public IEnumerable<string> GetUsersEmailsByFloor(int floorId)
        {
            var emails = _officeMapService.GetEmailsByFloor(floorId);
            return emails;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Office)]
        public IEnumerable<string> GetUsersEmailsByRoom(int roomId)
        {
            var emails = _officeMapService.GetEmailsByRoom(roomId);
            return emails;
        }

        [HttpGet, HttpPost]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public PagedViewModel<OfficeUserDTO> GetPagedByFloor(int floorId, int page = 1, int pageSize = WebApiConstants.DefaultPageSize, string includeProperties = "")
        {
            var officeUsersDto = _officeMapService.GetOfficeUsers(floorId, includeProperties);

            var officeUserPagedViewModel = officeUsersDto.ToPagedList(page, pageSize);

            var pagedModel = new PagedViewModel<OfficeUserDTO>
            {
                PagedList = officeUserPagedViewModel,
                PageCount = officeUserPagedViewModel.PageCount,
                ItemCount = officeUserPagedViewModel.TotalItemCount,
                PageSize = pageSize
            };

            return pagedModel;
        }
    }
}