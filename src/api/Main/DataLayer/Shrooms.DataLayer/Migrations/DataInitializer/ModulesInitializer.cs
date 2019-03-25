using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.Constants.Authorization;
using Shrooms.DataLayer.DAL;
using Shrooms.EntityModels.Models;

namespace Shrooms.DataLayer.Migrations.DataInitializer
{
    public static class ModulesInitializer
    {
        public static void Create(ShroomsDbContext ctx)
        {
            var allOrganizations = ctx.Organizations.ToList();

            ctx.AddModule(Modules.Books, allOrganizations);
            ctx.AddModule(Modules.ServiceRequest, allOrganizations);
            ctx.AddModule(Modules.KudosBasket, allOrganizations);
            ctx.AddModule(Modules.Monitor, allOrganizations);
            ctx.AddModule(Modules.Vacation, allOrganizations);
            ctx.AddModule(Modules.Projects, allOrganizations);

            ctx.SaveChanges(false);
        }

        private static void AddModule(this ShroomsDbContext ctx, string moduleName, ICollection<Organization> organizations)
        {
            if (!ctx.Modules.Any(x => x.Name == moduleName))
            {
                var module = new Module
                {
                    Created = DateTime.UtcNow,
                    Modified = DateTime.UtcNow,
                    Name = moduleName,
                    Organizations = organizations
                };

                ctx.Modules.Add(module);
            }
        }
    }
}
