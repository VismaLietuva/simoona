using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Roles;
using Shrooms.Premium.DataTransferObjects.Models.OfficeMap;

namespace Shrooms.Premium.Domain.Services.OfficeMap
{
    public class OfficeMapService : IOfficeMapService
    {
        private readonly IRepository<ApplicationUser> _applicationUserRepository;
        private readonly IDbSet<Office> _officeDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IMapper _mapper;
        private readonly IRoleService _roleService;

        public OfficeMapService(IMapper mapper, IUnitOfWork unitOfWork, IUnitOfWork2 uow, IRoleService roleService)
        {
            _mapper = mapper;
            _applicationUserRepository = unitOfWork.GetRepository<ApplicationUser>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _officeDbSet = uow.GetDbSet<Office>();
            _roleService = roleService;
        }

        public IEnumerable<OfficeDTO> GetOffices()
        {
            var offices = _officeDbSet.ToList();

            return _mapper.Map<IEnumerable<Office>, IEnumerable<OfficeDTO>>(offices);
        }

        public IEnumerable<OfficeUserDTO> GetOfficeUsers(int floorId, string includeProperties)
        {
            var applicationUsers = _applicationUserRepository
                .Get(e => e.Room.FloorId == floorId, includeProperties: includeProperties)
                .Where(_roleService.ExcludeUsersWithRole(Roles.NewUser))
                .ToList();

            return _mapper.Map<IEnumerable<ApplicationUser>, IEnumerable<OfficeUserDTO>>(applicationUsers);
        }

        public IEnumerable<string> GetEmailsByOffice(int officeId)
        {
            var usersEmail = _usersDbSet
               .Include(x => x.Room)
               .Include(x => x.Room.Floor)
               .Where(x => x.Room.Floor.OfficeId == officeId)
               .Where(_roleService.ExcludeUsersWithRole(Roles.NewUser))
               .Select(x => x.Email)
               .ToList();

            return usersEmail;
        }

        public IEnumerable<string> GetEmailsByFloor(int floorId)
        {
            var usersEmail = _usersDbSet
               .Include(x => x.Room)
               .Where(x => x.Room.FloorId == floorId)
               .Where(_roleService.ExcludeUsersWithRole(Roles.NewUser))
               .Select(x => x.Email)
               .ToList();

            return usersEmail;
        }

        public IEnumerable<string> GetEmailsByRoom(int roomId)
        {
            var usersEmail = _usersDbSet
                .Where(x => x.RoomId == roomId)
                .Where(_roleService.ExcludeUsersWithRole(Roles.NewUser))
                .Select(x => x.Email)
                .ToList();

            return usersEmail;
        }

        public UserOfficeAndFloorDto GetUserOfficeAndFloor(string userId)
        {
            var userOfficeAndFloor = _usersDbSet.Where(u => u.Id == userId)
               .Include(u => u.Room)
               .Include(x => x.Room.Floor)
               .Include(x => x.Room.Floor.Office)
               .Select(u => new UserOfficeAndFloorDto { FloorId = u.Room.FloorId, OfficeId = u.Room.Floor.OfficeId })
               .First();

            return userOfficeAndFloor;
        }
    }
}
