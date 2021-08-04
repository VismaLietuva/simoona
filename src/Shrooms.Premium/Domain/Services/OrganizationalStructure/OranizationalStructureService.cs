using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MoreLinq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Roles;
using Shrooms.Premium.DataTransferObjects.Models.OrganizationalStructure;

namespace Shrooms.Premium.Domain.Services.OrganizationalStructure
{
    public class OrganizationalStructureService : IOrganizationalStructureService
    {
        private readonly IDbSet<ApplicationUser> _applicationUsersDbSet;
        private readonly IRoleService _roleService;

        public OrganizationalStructureService(IUnitOfWork2 uow, IRoleService roleService)
        {
            _applicationUsersDbSet = uow.GetDbSet<ApplicationUser>();
            _roleService = roleService;
        }

        public async Task<OrganizationalStructureDTO> GetOrganizationalStructureAsync(UserAndOrganizationDTO userAndOrg)
        {
            var newUserRole = await _roleService.GetRoleIdByNameAsync(Roles.NewUser);

            var userList = await _applicationUsersDbSet
                .Where(u => u.OrganizationId == userAndOrg.OrganizationId)
                .Where(_roleService.ExcludeUsersWithRole(newUserRole))
                .Select(MapToOrganizationalStructureUserDTO())
                .ToListAsync();

            var head = userList.First(u => u.IsManagingDirector);
            var result = MapUsersToOrganizationalStructureDTO(head, userList);
            return result;
        }

        private static Expression<Func<ApplicationUser, OrganizationalStructureUserDTO>> MapToOrganizationalStructureUserDTO()
        {
            return user => new OrganizationalStructureUserDTO
            {
                FirstName = user.FirstName,
                IsManagingDirector = user.IsManagingDirector,
                LastName = user.LastName,
                Id = user.Id,
                ManagerId = user.ManagerId,
                PictureId = user.PictureId
            };
        }

        private IEnumerable<OrganizationalStructureDTO> GetChildren(IList<OrganizationalStructureUserDTO> userList, OrganizationalStructureUserDTO head)
        {
            var childrenList = new List<OrganizationalStructureDTO>();

            userList
                .Where(user => user.ManagerId == head.Id)
                .ForEach(user => childrenList.Add(MapUsersToOrganizationalStructureDTO(user, userList)));

            return childrenList;
        }

        private OrganizationalStructureDTO MapUsersToOrganizationalStructureDTO(OrganizationalStructureUserDTO user, IList<OrganizationalStructureUserDTO> userList)
        {
            return new OrganizationalStructureDTO
            {
                FullName = user.FirstName + " " + user.LastName,
                PictureId = user.PictureId,
                Children = GetChildren(userList, user)
            };
        }
    }
}
