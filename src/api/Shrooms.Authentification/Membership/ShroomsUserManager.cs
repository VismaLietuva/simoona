using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Authentification.Membership
{
    public class ShroomsUserManager : UserManager<ApplicationUser>
    {
        private readonly ICustomCache<string, IEnumerable<string>> _permissionsCache;

        public ShroomsUserManager(
            IUserStore<ApplicationUser> store,
            IDataProtectionProvider dataProtectionProvider,
            IIdentityMessageService emailService,
            ShroomsClaimsIdentityFactory claimsIdentityFactory,
            ICustomCache<string, IEnumerable<string>> permissionsCache)
            : base(store)
        {
            _permissionsCache = permissionsCache;
            UserValidator = new ShroomsUserValidator(this);
            ClaimsIdentityFactory = claimsIdentityFactory;
            EmailService = emailService;

            UserValidator = new ShroomsUserValidator(this)
            {
                RequireUniqueEmail = true,
                AllowOnlyAlphanumericUserNames = false
            };

            PasswordValidator = new ShroomsPasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true
            };
            UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
        }

        public async Task<string> GetEmailAsync(ApplicationUser user)
        {
            var email = await ((IUserEmailStore<ApplicationUser, string>)Store).GetEmailAsync(user);
            return email;
        }

        public void RemoveLogins(string id)
        {
            var logins = this.FindById(id).Logins.ToList();
            foreach (var login in logins)
            {
                this.RemoveLogin(id, new UserLoginInfo(login.LoginProvider, login.ProviderKey));
            }
        }

        public override Task<IdentityResult> AddToRoleAsync(string userId, string role)
        {
            var identityResult = base.AddToRoleAsync(userId, role);
            _permissionsCache.TryRemoveEntry(userId);
            return identityResult;
        }

        public override Task<IdentityResult> RemoveFromRoleAsync(string userId, string role)
        {
            var identityRresult = base.RemoveFromRoleAsync(userId, role);
            _permissionsCache.TryRemoveEntry(userId);
            return identityRresult;
        }

        public override async Task<IdentityResult> AddLoginAsync(string userId, UserLoginInfo login)
        {
            var user = await FindAsync(login);
            if (user == null)
            {
                return await base.AddLoginAsync(userId, login);
            }

            return null;
        }
    }
}
