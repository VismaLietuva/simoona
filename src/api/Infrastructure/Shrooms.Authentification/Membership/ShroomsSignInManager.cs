using DataLayer.Models;
using Logger;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shrooms.Authorization
{
    public class ShroomsSignInManager : SignInManager<ApplicationUser, string>
    {
        private readonly ShroomsUserManager _userManager;
        private static readonly ILogger _logger = LogFactory.GetLogger();

        public ShroomsSignInManager(ShroomsUserManager userManager, IAuthenticationManager authenticationManager) 
            : base(userManager, authenticationManager)
        {
            _userManager = userManager;
            _logger.Debug("ShroomsSignInManager created");
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return _userManager.CreateIdentityAsync(user, OAuthDefaults.AuthenticationType);
        }
    }
}

