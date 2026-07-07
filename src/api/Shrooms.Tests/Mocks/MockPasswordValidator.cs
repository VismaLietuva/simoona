using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Tests.Mocks
{
    public class MockPasswordValidator : IPasswordValidator<ApplicationUser>
    {
        public MockPasswordValidator(bool validateSuccessfully)
        {
            ValidateSuccessfully = validateSuccessfully;
        }

        public bool ValidateSuccessfully { get; set; }

        public bool Validated { get; set; }

        public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string password)
        {
            Validated = true;
            if (ValidateSuccessfully)
            {
                return Task.FromResult(IdentityResult.Success);
            }

            return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Failed" }));
        }
    }
}
