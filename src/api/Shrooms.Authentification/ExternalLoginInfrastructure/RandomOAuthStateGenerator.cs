using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace Shrooms.Authentification.ExternalLoginInfrastructure
{
    public static class RandomOAuthStateGenerator
    {
        public static string Generate(int strengthInBits)
        {
            const int bitsPerByte = 8;

            if (strengthInBits % bitsPerByte != 0)
            {
                throw new ArgumentException("strengthInBits must be evenly divisible by 8.", nameof(strengthInBits));
            }

            var strengthInBytes = strengthInBits / bitsPerByte;

            var data = new byte[strengthInBytes];
            RandomNumberGenerator.Fill(data);
            return WebEncoders.Base64UrlEncode(data);
        }
    }
}