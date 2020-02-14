using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Authentification.Membership
{
    public class ShroomsUserValidator : UserValidator<ApplicationUser>
    {
        private ShroomsUserManager UserManager { get; set; }

        public ShroomsUserValidator(ShroomsUserManager userManager)
            : base(userManager)
        {
            UserManager = userManager;
        }

        public override async Task<IdentityResult> ValidateAsync(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var list = new List<string>();
            await ValidateUserName(user, list);

            if (RequireUniqueEmail)
            {
                await ValidateEmail(user, list);
            }

            return list.Count > 0 ? IdentityResult.Failed(list.ToArray()) : IdentityResult.Success;
        }

        private async Task ValidateEmail(ApplicationUser user, List<string> errors)
        {
            var email = await UserManager.GetEmailAsync(user);

            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture, Resources.Models.Account.Account.PropertyTooShort, "Email"));
            }
            else
            {
                try
                {
                    new MailAddress(email);
                }
                catch (FormatException)
                {
                    errors.Add(string.Format(CultureInfo.CurrentCulture, Resources.Models.Account.Account.InvalidEmail, email));
                    return;
                }

                var foundUser = await UserManager.FindByEmailAsync(email);

                if (foundUser != null && !string.Equals(foundUser.Id, user.Id))
                {
                    errors.Add(string.Format(CultureInfo.CurrentCulture, Resources.Models.Account.Account.DuplicateEmail, email));
                }
            }
        }

        private async Task ValidateUserName(ApplicationUser user, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture, Resources.Models.Account.Account.PropertyTooShort, "Name"));
            }
            else
            {
                if (AllowOnlyAlphanumericUserNames && !Regex.IsMatch(user.UserName, "^[A-Za-z0-9@_\\.]+$"))
                {
                    errors.Add(string.Format(CultureInfo.CurrentCulture, Resources.Models.Account.Account.InvalidUserName, user.UserName));
                }
                else
                {
                    var foundUser = await UserManager.FindByNameAsync(user.UserName);

                    if (foundUser != null && !string.Equals(foundUser.Id, user.Id))
                    {
                        errors.Add(string.Format(CultureInfo.CurrentCulture, Resources.Models.Account.Account.DuplicateName, user.UserName));
                    }
                }
            }
        }
    }
}
