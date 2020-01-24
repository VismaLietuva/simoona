using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.Constants.Authorization;
using Shrooms.DataLayer.DAL;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.Constants;

namespace Shrooms.DataLayer.Migrations.DataInitializer
{
    public static class RoleInitializer
    {
        public static void CreateRoles(ShroomsDbContext context)
        {
            var timestamp = DateTime.UtcNow;
            var newRoles = new List<ApplicationRole>
            {
                new ApplicationRole { Name = Roles.Admin, CreatedTime = timestamp },
                new ApplicationRole { Name = Roles.Administration, CreatedTime = timestamp },
                new ApplicationRole { Name = Roles.EventsManagement, CreatedTime = timestamp },
                new ApplicationRole { Name = Roles.External, CreatedTime = timestamp },
                new ApplicationRole { Name = Roles.FirstLogin, CreatedTime = timestamp },
                new ApplicationRole { Name = Roles.Manager, CreatedTime = timestamp },
                new ApplicationRole { Name = Roles.NewUser, CreatedTime = timestamp },
                new ApplicationRole { Name = Roles.ServiceRequest, CreatedTime = timestamp },
                new ApplicationRole { Name = Roles.ServiceRequestNotification, CreatedTime = timestamp },
                new ApplicationRole { Name = Roles.User, CreatedTime = timestamp },
                new ApplicationRole { Name = Roles.KudosAdmin, CreatedTime = timestamp },
                new ApplicationRole { Name = Roles.LotteryAdmin, CreatedTime = timestamp},
            };

            var rolesWithOrg = GenerateRolesForEveryOrganization(newRoles, context);

            AddRolesIfNotExist(rolesWithOrg, context);
        }

        private static List<ApplicationRole> GenerateRolesForEveryOrganization(List<ApplicationRole> newRoles, ShroomsDbContext context)
        {
            var currentOrganizations = context.Organizations.ToList();

            var rolesWithOrg = newRoles
                .SelectMany(x =>
                    currentOrganizations.Select(o => new ApplicationRole { Name = x.Name, CreatedTime = x.CreatedTime, OrganizationId = o.Id }))
                .ToList();

            return rolesWithOrg;
        }

        private static void AddRolesIfNotExist(List<ApplicationRole> newRoles, ShroomsDbContext context)
        {
            var currentRoles = context.Roles.ToList();
            foreach (var newRole in newRoles)
            {
                if (!currentRoles.Any(x => x.Name == newRole.Name && x.OrganizationId == newRole.OrganizationId))
                {
                    context.Roles.Add(newRole);
                }
            }

            context.SaveChanges(false);
        }
    }
}
