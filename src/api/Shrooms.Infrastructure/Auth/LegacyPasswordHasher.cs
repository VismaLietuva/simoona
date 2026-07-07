using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Shrooms.Infrastructure.Auth
{
    /// <summary>
    /// Password hasher that supports both ASP.NET Identity v2 (SHA1-based, format version 0x00)
    /// and ASP.NET Core Identity v3 (PBKDF2, format version 0x01) hashes.
    /// Returns SuccessRehashNeeded on v2 so UserManager re-hashes to v3 on next save.
    /// </summary>
    public class LegacyPasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : class
    {
        private readonly PasswordHasher<TUser> _v3Hasher = new();

        public string HashPassword(TUser user, string password)
        {
            return _v3Hasher.HashPassword(user, password);
        }

        public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
            {
                return PasswordVerificationResult.Failed;
            }

            byte[] decoded;
            try
            {
                decoded = Convert.FromBase64String(hashedPassword);
            }
            catch (FormatException)
            {
                return PasswordVerificationResult.Failed;
            }

            // Identity v2 hash: first byte is 0x00, followed by salt (16 bytes) + PBKDF1-SHA1 subkey (32 bytes)
            if (decoded.Length > 0 && decoded[0] == 0x00)
            {
                return VerifyV2Hash(decoded, providedPassword)
                    ? PasswordVerificationResult.SuccessRehashNeeded
                    : PasswordVerificationResult.Failed;
            }

            // Identity v3 hash (first byte 0x01)
            return _v3Hasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
        }

        private static bool VerifyV2Hash(byte[] hashedBytes, string password)
        {
            try
            {
                // Format: 0x00 | salt (16 bytes) | subkey (32 bytes) = 49 bytes total
                if (hashedBytes.Length != 49) return false;

                var salt = new byte[16];
                Buffer.BlockCopy(hashedBytes, 1, salt, 0, 16);
                var storedSubkey = new byte[32];
                Buffer.BlockCopy(hashedBytes, 17, storedSubkey, 0, 32);

                var generatedSubkey = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.Unicode.GetBytes(password),
                    salt,
                    1000,
                    HashAlgorithmName.SHA1,
                    32);

                return CryptographicOperations.FixedTimeEquals(generatedSubkey, storedSubkey);
            }
            catch
            {
                return false;
            }
        }
    }
}
