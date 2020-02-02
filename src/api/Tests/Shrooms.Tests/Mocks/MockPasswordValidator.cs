using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Shrooms.UnitTests.Mocks
{
    public class MockPasswordValidator<T> : IIdentityValidator<T>
    {
        public MockPasswordValidator(bool validateSuccessfully)
        {
            ValidateSuccessfully = validateSuccessfully;
        }

        public bool ValidateSuccessfully { get; set; }

        public bool Validated { get; set; }

        public Task<IdentityResult> ValidateAsync(T item)
        {
            Validated = true;
            if (ValidateSuccessfully)
            {
                return Task.FromResult(IdentityResult.Success);
            }

            return Task.FromResult(IdentityResult.Failed("Failed"));
        }
    }
}
