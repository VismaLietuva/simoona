using Microsoft.AspNet.Identity;

namespace Shrooms.Premium.Tests.Mocks
{
    public class MockPasswordHasher : IPasswordHasher
    {
        public bool Hashed { get; set; }

        public bool VerifiedHashedPassword { get; set; }

        public string HashPasswordResult { get; set; }

        public PasswordVerificationResult VerifyHashedPasswordResult { get; set; }

        public string HashPassword(string password)
        {
            Hashed = true;
            return HashPasswordResult;
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            VerifiedHashedPassword = true;
            return VerifyHashedPasswordResult;
        }
    }
}
