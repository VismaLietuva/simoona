using Shrooms.Contracts.Constants;
using Shrooms.DataLayer.DAL;

namespace Shrooms.DataLayer.Migrations.DataInitializer
{
    public static class PermissionInitializer
    {
        internal static void CreatePermissions(ShroomsDbContext context)
        {
            var permissionBuilder = new PermissionBuilder(context);

            //basic permissions
            permissionBuilder
                .AddBasicPermission(permissionName: BasicPermissions.Map)
                .AddBasicPermission(permissionName: BasicPermissions.Wall)
                .AddBasicPermission(permissionName: BasicPermissions.Book, module: Modules.Books)
                .AddBasicPermission(permissionName: BasicPermissions.Post)
                .AddBasicPermission(permissionName: BasicPermissions.Like)
                .AddBasicPermission(permissionName: BasicPermissions.Kudos)
                .AddBasicPermission(permissionName: BasicPermissions.Comment)
                .AddBasicPermission(permissionName: BasicPermissions.Birthday)
                .AddBasicPermission(permissionName: BasicPermissions.Vacation, module: Modules.Vacation)
                .AddBasicPermission(permissionName: BasicPermissions.Support)
                .AddBasicPermission(permissionName: BasicPermissions.Recommendation)
                .AddBasicPermission(permissionName: BasicPermissions.Committees)
                .AddBasicPermission(permissionName: BasicPermissions.KudosBasket, module: Modules.KudosBasket)
                .AddBasicPermission(permissionName: BasicPermissions.EmployeeList)
                .AddBasicPermission(permissionName: BasicPermissions.Localization)
                .AddBasicPermission(permissionName: BasicPermissions.Organization)
                .AddBasicPermission(permissionName: BasicPermissions.ExternalLink)
                .AddBasicPermission(permissionName: BasicPermissions.ServiceRequest, module: Modules.ServiceRequest)
                .AddBasicPermission(permissionName: BasicPermissions.OrganizationalStructure)
                .AddBasicPermission(permissionName: BasicPermissions.Exam, withRoleNames: Roles.NewUser)
                
                .AddBasicPermission(permissionName: BasicPermissions.Room, withRoleNames: Roles.NewUser)
                .AddBasicPermission(permissionName: BasicPermissions.Floor, withRoleNames: Roles.NewUser)
                .AddBasicPermission(permissionName: BasicPermissions.Skill, withRoleNames: Roles.NewUser)
                .AddBasicPermission(permissionName: BasicPermissions.Office, withRoleNames: Roles.NewUser)
                .AddBasicPermission(permissionName: BasicPermissions.OfficeUsers, withRoleNames: Roles.NewUser)
                
                .AddBasicPermission(permissionName: BasicPermissions.Event, withRoleNames: new[] { Roles.External, Roles.Intern })
                .AddBasicPermission(permissionName: BasicPermissions.EventWall, withRoleNames: new[] { Roles.External, Roles.Intern })
                .AddBasicPermission(permissionName: BasicPermissions.EventUsers, withRoleNames: Roles.NewUser)
                
                .AddBasicPermission(permissionName: BasicPermissions.Picture, withRoleNames: Roles.NewUser)
                .AddBasicPermission(permissionName: BasicPermissions.Certificate, withRoleNames: Roles.NewUser)
                .AddBasicPermission(permissionName: BasicPermissions.ApplicationUser, withRoleNames: Roles.NewUser)
                .AddBasicPermission(permissionName: BasicPermissions.QualificationLevel, withRoleNames: Roles.NewUser)
                .AddBasicPermission(permissionName: BasicPermissions.Project, withRoleNames: Roles.NewUser, module: Modules.Projects)
                .AddBasicPermission(permissionName: BasicPermissions.Lottery);

            //admin permissions
            permissionBuilder
                .AddAdminPermission(permissionName: AdministrationPermissions.Wall, withRoleNames: Roles.Administration)
                .AddAdminPermission(permissionName: AdministrationPermissions.Post, withRoleNames: Roles.Administration)
                .AddAdminPermission(permissionName: AdministrationPermissions.Book, withRoleNames: Roles.Administration, module: Modules.Books)
                .AddAdminPermission(permissionName: AdministrationPermissions.Role, withRoleNames: Roles.Administration)
                .AddAdminPermission(permissionName: AdministrationPermissions.Room, withRoleNames: Roles.Administration)
                .AddAdminPermission(permissionName: AdministrationPermissions.Floor, withRoleNames: Roles.Administration)
                .AddAdminPermission(permissionName: AdministrationPermissions.Kudos, withRoleNames: Roles.KudosAdmin)
                .AddAdminPermission(permissionName: AdministrationPermissions.KudosBasket, withRoleNames: Roles.KudosAdmin, module: Modules.KudosBasket)
                .AddAdminPermission(permissionName: AdministrationPermissions.Office, withRoleNames: Roles.Administration)
                .AddAdminPermission(permissionName: AdministrationPermissions.Project, withRoleNames: Roles.Administration, module: Modules.Projects)
                .AddAdminPermission(permissionName: AdministrationPermissions.Account, withRoleNames: Roles.Administration)
                .AddAdminPermission(permissionName: AdministrationPermissions.RoomType, withRoleNames: Roles.Administration)
                .AddAdminPermission(permissionName: AdministrationPermissions.Birthdays, withRoleNames: Roles.Administration)
                .AddAdminPermission(permissionName: AdministrationPermissions.Certificate, withRoleNames: Roles.Administration)
                .AddAdminPermission(permissionName: AdministrationPermissions.ExternalLink, withRoleNames: Roles.Administration)
                .AddAdminPermission(permissionName: AdministrationPermissions.Organization, withRoleNames: Roles.Administration)
                .AddAdminPermission(permissionName: AdministrationPermissions.Administration, withRoleNames: Roles.Administration)
                .AddAdminPermission(permissionName: AdministrationPermissions.ApplicationUser, withRoleNames: Roles.Administration)
                .AddAdminPermission(permissionName: AdministrationPermissions.QualificationLevel, withRoleNames: Roles.Administration)

                .AddAdminPermission(permissionName: AdministrationPermissions.Event, withRoleNames: new[] { Roles.Administration, Roles.EventsManagement })
                .AddAdminPermission(permissionName: AdministrationPermissions.EventWall, withRoleNames: Roles.Administration)

                .AddAdminPermission(permissionName: AdministrationPermissions.Vacation, withRoleNames: new[] { Roles.Accountant, Roles.Administration }, module: Modules.Vacation)
                .AddAdminPermission(permissionName: AdministrationPermissions.Committees, withRoleNames: new[] { Roles.Administration, Roles.KudosAdmin })
                .AddAdminPermission(permissionName: AdministrationPermissions.ServiceRequest, withRoleNames: new[] { Roles.ServiceRequest, Roles.ServiceRequestNotification }, module: Modules.ServiceRequest)
                .AddAdminPermission(permissionName: AdministrationPermissions.Monitor, withRoleNames: new[] { Roles.Administration }, module: Modules.Monitor)
                .AddAdminPermission(permissionName: AdministrationPermissions.KudosShop, withRoleNames: Roles.Administration)
                .AddAdminPermission(permissionName: AdministrationPermissions.Job, withRoleNames: Roles.Administration)
                .AddAdminPermission(permissionName: AdministrationPermissions.Lottery, withRoleNames: new[] { Roles.LotteryAdmin, Roles.Administration });

            permissionBuilder.UpdatePermissions();
        }
    }
}
