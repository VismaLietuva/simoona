using Microsoft.AspNetCore.Identity;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Tests.Mocks
{
    public class MockPasswordHasher : IPasswordHasher<ApplicationUser>
    {
        public bool Hashed { get; set; }

        public bool VerifiedHashedPassword { get; set; }

        public string HashPasswordResult { get; set; }

        public PasswordVerificationResult VerifyHashedPasswordResult { get; set; }

        public string HashPassword(ApplicationUser user, string password)
        {
            Hashed = true;
            return HashPasswordResult;
        }

        public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
        {
            VerifiedHashedPassword = true;
            return VerifyHashedPasswordResult;
        }
    }
}
