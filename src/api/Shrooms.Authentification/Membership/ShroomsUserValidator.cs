using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Authentification.Membership
{
    public class ShroomsUserValidator : IUserValidator<ApplicationUser>
    {
        public bool RequireUniqueEmail { get; set; } = true;
        public bool AllowOnlyAlphanumericUserNames { get; set; } = false;

        public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var errors = new List<IdentityError>();
            await ValidateUserName(manager, user, errors);

            if (RequireUniqueEmail)
            {
                await ValidateEmail(manager, user, errors);
            }

            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }

        private async Task ValidateEmail(UserManager<ApplicationUser> manager, ApplicationUser user, List<IdentityError> errors)
        {
            var email = user.Email;

            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add(new IdentityError 
                { 
                    Description = string.Format(CultureInfo.CurrentCulture, Resources.Models.Account.Account.PropertyTooShort, "Email") 
                });
            }
            else
            {
                try
                {
                    new MailAddress(email);
                }
                catch (FormatException)
                {
                    errors.Add(new IdentityError 
                    { 
                        Description = string.Format(CultureInfo.CurrentCulture, Resources.Models.Account.Account.InvalidEmail, email) 
                    });
                    return;
                }

                var foundUser = await manager.FindByEmailAsync(email);

                if (foundUser != null && !string.Equals(foundUser.Id, user.Id))
                {
                    errors.Add(new IdentityError 
                    { 
                        Description = string.Format(CultureInfo.CurrentCulture, Resources.Models.Account.Account.DuplicateEmail, email) 
                    });
                }
            }
        }

        private async Task ValidateUserName(UserManager<ApplicationUser> manager, ApplicationUser user, List<IdentityError> errors)
        {
            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                errors.Add(new IdentityError 
                { 
                    Description = string.Format(CultureInfo.CurrentCulture, Resources.Models.Account.Account.PropertyTooShort, "Name") 
                });
            }
            else
            {
                if (AllowOnlyAlphanumericUserNames && !Regex.IsMatch(user.UserName, "^[A-Za-z0-9@_\\.]+$"))
                {
                    errors.Add(new IdentityError 
                    { 
                        Description = string.Format(CultureInfo.CurrentCulture, Resources.Models.Account.Account.InvalidUserName, user.UserName) 
                    });
                }
                else
                {
                    var foundUser = await manager.FindByNameAsync(user.UserName);

                    if (foundUser != null && !string.Equals(foundUser.Id, user.Id))
                    {
                        errors.Add(new IdentityError 
                        { 
                            Description = string.Format(CultureInfo.CurrentCulture, Resources.Models.Account.Account.DuplicateName, user.UserName) 
                        });
                    }
                }
            }
        }
    }
}
