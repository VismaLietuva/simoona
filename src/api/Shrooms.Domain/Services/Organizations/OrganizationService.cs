using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Exceptions.Exceptions.Organization;
using Shrooms.Domain.Services.Roles;

namespace Shrooms.Domain.Services.Organizations
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IDbSet<Organization> _organizationsDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IRoleService _roleService;
        private readonly IUnitOfWork2 _uow;

        public OrganizationService(IUnitOfWork2 uow, IRoleService roleService)
        {
            _organizationsDbSet = uow.GetDbSet<Organization>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _roleService = roleService;
            _uow = uow;
        }

        public Organization GetOrganizationById(int id)
        {
            return _organizationsDbSet.Find(id);
        }

#pragma warning disable S1449 //EF does does not understand Equals(invarient) or ToLowerInvarient()
        public Organization GetOrganizationByName(string organizationName)
        {
            return _organizationsDbSet
                .Single(organization => organization.ShortName.ToLower() == organizationName.ToLower());
        }

        public async Task<string> GetOrganizationHostNameAsync(string organizationName)
        {
            var hostName = (await _organizationsDbSet.SingleAsync(organization => organization.ShortName.ToLower() == organizationName.ToLower())).HostName;
            return hostName;
        }

        public async Task<bool> HasOrganizationEmailDomainRestrictionAsync(string organizationName)
        {
            var organization = await _organizationsDbSet
                .SingleOrDefaultAsync(o => o.ShortName.ToLower() == organizationName.ToLower());

            if (organization == null)
            {
                throw new InvalidOrganizationException();
            }

            return organization.HasRestrictedAccess;
        }
#pragma warning restore S1449

        public Organization GetUserOrganization(ApplicationUser user)
        {
            return _usersDbSet
                .Find(user.Id).Organization;
        }

        public async Task<bool> IsOrganizationHostValidAsync(string email, string requestedOrganizationName)
        {
            if (!await HasOrganizationEmailDomainRestrictionAsync(requestedOrganizationName))
            {
                return true;
            }

            var validEmailHostName = await GetOrganizationHostNameAsync(requestedOrganizationName);
            if (GetHostFromEmail(email) != validEmailHostName)
            {
                return false;
            }

            return true;
        }

        public bool RequiresUserConfirmation(int organizationId)
        {
            return _organizationsDbSet
                .Where(o => o.Id == organizationId)
                .Select(o => o.RequiresUserConfirmation)
                .First();
        }

        public UserDto GetManagingDirector(int organizationId)
        {
            var managingDirector = _usersDbSet
                .Where(x => x.OrganizationId == organizationId &&
                            x.IsManagingDirector)

                // Don't change to string interpolation, since EF won't handle project it to SQL
                .Select(x => new UserDto
                {
                    UserId = x.Id,
                    FullName = x.FirstName + " " + x.LastName
                })
                .FirstOrDefault();

            return managingDirector;
        }

        public void SetManagingDirector(string userId, UserAndOrganizationDTO userAndOrganizationDTO)
        {
            if (!_roleService.HasRole(userId, Contracts.Constants.Roles.Manager))
            {
                throw new ValidationException(ErrorCodes.UserIsNotAManager, "User need to have manager role to become a managing director");
            }

            var managingDirectors = _usersDbSet
                .Include(x => x.Roles)
                .Where(x => x.OrganizationId == userAndOrganizationDTO.OrganizationId)
                .Where(x => x.IsManagingDirector || x.Id == userId)
                .ToList();

            foreach (var currentDirector in managingDirectors.Where(x => x.IsManagingDirector))
            {
                currentDirector.IsManagingDirector = false;
            }

            var newManagingDirector = managingDirectors.First(x => x.Id == userId);

            newManagingDirector.IsManagingDirector = true;

            _uow.SaveChanges(userAndOrganizationDTO.UserId);
        }

        private static string GetHostFromEmail(string email)
        {
            var mailAddress = new MailAddress(email);
            return mailAddress.Host.ToLowerInvariant();
        }
    }
}
