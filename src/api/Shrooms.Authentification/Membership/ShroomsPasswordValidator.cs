using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Authentification.Membership
{
    public class ShroomsPasswordValidator : IPasswordValidator<ApplicationUser>
    {
        public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string password)
        {
            var errors = new List<IdentityError>();

            // Basic validation
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                errors.Add(new IdentityError { Description = "Passwords must be at least 6 characters." });
            }

            if (!password.Any(char.IsDigit))
            {
                errors.Add(new IdentityError { Description = Resources.Models.ApplicationUser.ApplicationUser.PasswordErrorMustContainDigit });
            }

            if (!password.Any(char.IsLower))
            {
                errors.Add(new IdentityError { Description = Resources.Models.ApplicationUser.ApplicationUser.PasswordErrorMustContainLowerCase });
            }

            if (!password.Any(char.IsUpper))
            {
                errors.Add(new IdentityError { Description = Resources.Models.ApplicationUser.ApplicationUser.PasswordErrorMustContainUpperCase });
            }

            return await Task.FromResult(errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray()));
        }
    }
}
