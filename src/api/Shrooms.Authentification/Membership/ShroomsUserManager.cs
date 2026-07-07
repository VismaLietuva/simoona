using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Authentification.Membership
{
    public class ShroomsUserManager : UserManager<ApplicationUser>
    {
        private readonly ICustomCache<string, IEnumerable<string>> _permissionsCache;

        public ShroomsUserManager(IUserStore<ApplicationUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IEnumerable<IUserValidator<ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<ApplicationUser>> logger,
            ICustomCache<string, IEnumerable<string>> permissionsCache)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _permissionsCache = permissionsCache;
        }

        public async Task RemoveLoginsAsync(string id)
        {
            var user = await FindByIdAsync(id);
            var logins = (await GetLoginsAsync(user)).ToList();

            foreach (var login in logins)
            {
                await RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
            }
        }

        public override async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
        {
            var identityResult = await base.AddToRoleAsync(user, role);
            _permissionsCache.TryRemoveEntry(user.Id);
            return identityResult;
        }

        public override async Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role)
        {
            var identityResult = await base.RemoveFromRoleAsync(user, role);
            _permissionsCache.TryRemoveEntry(user.Id);
            return identityResult;
        }

        public override async Task<IdentityResult> AddLoginAsync(ApplicationUser user, UserLoginInfo login)
        {
            var existingUser = await FindByLoginAsync(login.LoginProvider, login.ProviderKey);
            if (existingUser == null)
            {
                return await base.AddLoginAsync(user, login);
            }

            return IdentityResult.Failed(new IdentityError { Description = "Login already associated with another user" });
        }
    }
}
