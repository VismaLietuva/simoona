using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Shrooms.Authentification
{
    public class ShroomsPasswordValidator : PasswordValidator
    {
        public async override Task<IdentityResult> ValidateAsync(string item)
        {
            var result = await base.ValidateAsync(item);
            var errors = TranslatePasswordHasherErrors(result.Errors).ToArray();
            return result.Succeeded ? IdentityResult.Success : new IdentityResult(errors);
        }

        //TODO: workaround for Microsoft.AspNet.Identity.Core.dll, v2.0.0.0, in future version 3.0 IdentityResult will return status codes
        //check - https://github.com/aspnet/Identity/issues/86
        protected IEnumerable<string> TranslatePasswordHasherErrors(IEnumerable<string> errors)
        {
            foreach (var error in errors)
            {
                var strBuilder = new StringBuilder(error);
                strBuilder = strBuilder.Replace("Passwords must have at least one digit ('0'-'9').", Resources.Models.ApplicationUser.ApplicationUser.PasswordErrorMustContainDigit);
                strBuilder = strBuilder.Replace("Passwords must have at least one lowercase ('a'-'z').", Resources.Models.ApplicationUser.ApplicationUser.PasswordErrorMustContainLowerCase);
                strBuilder = strBuilder.Replace("Passwords must have at least one uppercase ('A'-'Z').", Resources.Models.ApplicationUser.ApplicationUser.PasswordErrorMustContainUpperCase);
                yield return strBuilder.ToString();
            }
        }
    }
}
