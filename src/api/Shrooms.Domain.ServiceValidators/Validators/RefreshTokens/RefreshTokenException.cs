using System;

namespace Shrooms.Domain.ServiceValidators.Validators.RefreshTokens
{
    public class RefreshTokenException : Exception
    {
        public RefreshTokenException(string message)
            : base(message)
        {
        }
    }
}
