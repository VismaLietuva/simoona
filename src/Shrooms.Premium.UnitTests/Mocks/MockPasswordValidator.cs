using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Shrooms.Premium.UnitTests.Mocks
{
    public class MockPasswordValidator<T> : IIdentityValidator<T>
    {
        public MockPasswordValidator(bool validateSuccessfully)
        {
            ValidateSuccessfully = validateSuccessfully;
        }

        /// <summary>
        /// If true, then ValidateAsync will return IdentityResult.Success.
        /// </summary>
        public bool ValidateSuccessfully { get; set; }

        /// <summary>
        /// True if ValidateAsync was called.
        /// </summary>
        public bool Validated { get; set; }

        public Task<IdentityResult> ValidateAsync(T item)
        {
            Validated = true;
            if (ValidateSuccessfully)
            {
                return Task.FromResult(IdentityResult.Success);
            }
            else
            {
                return Task.FromResult(IdentityResult.Failed("Failed"));
            }
        }
    }
}
