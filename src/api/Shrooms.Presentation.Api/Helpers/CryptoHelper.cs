using System;
using System.Security.Cryptography;
using System.Text;

namespace Shrooms.Presentation.Api.Helpers
{
    public static class CryptoHelper
    {
        public static string GetHash(string input)
        {
            var hashAlgorithm = new SHA256CryptoServiceProvider();
            var byteValue = Encoding.UTF8.GetBytes(input);
            var byteHash = hashAlgorithm.ComputeHash(byteValue);
            var base64Representation = Convert.ToBase64String(byteHash);
            return base64Representation;
        }
    }
}