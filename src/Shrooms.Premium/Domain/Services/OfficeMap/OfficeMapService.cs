using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<IEnumerable<OfficeDTO>> GetOfficesAsync()
        {
            var offices = await _officeDbSet.ToListAsync();

            return _mapper.Map<IEnumerable<Office>, IEnumerable<OfficeDTO>>(offices);
        }

        public async Task<int> GetOfficesCountAsync()
        {
            return await _officeDbSet.CountAsync();
        }

        public async Task<IEnumerable<OfficeUserDTO>> GetOfficeUsersAsync(int floorId, string includeProperties)
        {
            var newUserRole = await _roleService.GetRoleIdByNameAsync(Roles.NewUser);

            var applicationUsers = await _applicationUserRepository
                .Get(e => e.Room.FloorId == floorId, includeProperties: includeProperties)
                .Where(_roleService.ExcludeUsersWithRole(newUserRole))
                .ToListAsync();

            return _mapper.Map<IEnumerable<ApplicationUser>, IEnumerable<OfficeUserDTO>>(applicationUsers);
        }

        public async Task<IEnumerable<string>> GetEmailsByOfficeAsync(int officeId)
        {
            var newUserRole = await _roleService.GetRoleIdByNameAsync(Roles.NewUser);

            var usersEmail = await _usersDbSet
               .Include(x => x.Room)
               .Include(x => x.Room.Floor)
               .Where(x => x.Room.Floor.OfficeId == officeId)
               .Where(_roleService.ExcludeUsersWithRole(newUserRole))
               .Select(x => x.Email)
               .ToListAsync();

            return usersEmail;
        }

        public async Task<IEnumerable<string>> GetEmailsByFloorAsync(int floorId)
        {
            var newUserRole = await _roleService.GetRoleIdByNameAsync(Roles.NewUser);

            var usersEmail = await _usersDbSet
               .Include(x => x.Room)
               .Where(x => x.Room.FloorId == floorId)
               .Where(_roleService.ExcludeUsersWithRole(newUserRole))
               .Select(x => x.Email)
               .ToListAsync();

            return usersEmail;
        }

        public async Task<IEnumerable<string>> GetEmailsByRoomAsync(int roomId)
        {
            var newUserRole = await _roleService.GetRoleIdByNameAsync(Roles.NewUser);

            var usersEmail = await _usersDbSet
                .Where(x => x.RoomId == roomId)
                .Where(_roleService.ExcludeUsersWithRole(newUserRole))
                .Select(x => x.Email)
                .ToListAsync();

            return usersEmail;
        }

        public async Task<UserOfficeAndFloorDto> GetUserOfficeAndFloorAsync(string userId)
        {
            var userOfficeAndFloor = await _usersDbSet.Where(u => u.Id == userId)
               .Include(u => u.Room)
               .Include(x => x.Room.Floor)
               .Include(x => x.Room.Floor.Office)
               .Select(u => new UserOfficeAndFloorDto { FloorId = u.Room.FloorId, OfficeId = u.Room.Floor.OfficeId })
               .FirstAsync();

            return userOfficeAndFloor;
        }
    }
}
