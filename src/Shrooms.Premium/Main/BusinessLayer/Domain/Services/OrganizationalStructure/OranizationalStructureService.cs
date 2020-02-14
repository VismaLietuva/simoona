using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using MoreLinq;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Roles;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.OrganizationalStructure;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.OrganizationalStructure
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

        public OrganizationalStructureDTO GetOrganizationalStructure(UserAndOrganizationDTO userAndOrg)
        {
            var userList = _applicationUsersDbSet
                .Where(u => u.OrganizationId == userAndOrg.OrganizationId)
                .Where(_roleService.ExcludeUsersWithRole(Roles.NewUser))
                .Select(MapToOrganizationalStructureUserDTO())
                .ToList();

            var head = userList.First(u => u.IsManagingDirector);
            var result = MapUsersToOrganizationalStructureDTO(head, userList);
            return result;
        }

        private Expression<Func<ApplicationUser, OrganizationalStructureUserDTO>> MapToOrganizationalStructureUserDTO()
        {
            return user => new OrganizationalStructureUserDTO()
            {
                FirstName = user.FirstName,
                IsManagingDirector = user.IsManagingDirector,
                LastName = user.LastName,
                Id = user.Id,
                ManagerId = user.ManagerId,
                PictureId = user.PictureId
            };
        }

        private IEnumerable<OrganizationalStructureDTO> GetChildrens(IEnumerable<OrganizationalStructureUserDTO> userList, OrganizationalStructureUserDTO head)
        {
            var childrenList = new List<OrganizationalStructureDTO>();

            userList
                .Where(user => user.ManagerId == head.Id)
                .ForEach(user => childrenList.Add(MapUsersToOrganizationalStructureDTO(user, userList)));

            return childrenList;
        }

        private OrganizationalStructureDTO MapUsersToOrganizationalStructureDTO(OrganizationalStructureUserDTO user, IEnumerable<OrganizationalStructureUserDTO> userList)
        {
            return new OrganizationalStructureDTO()
            {
                FullName = user.FirstName + " " + user.LastName,
                PictureId = user.PictureId,
                Children = GetChildrens(userList, user)
            };
        }
    }
}
