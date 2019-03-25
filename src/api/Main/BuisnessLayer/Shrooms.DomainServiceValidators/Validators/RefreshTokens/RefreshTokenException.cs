using System;

namespace DomainServiceValidators.Validators.RefreshTokens
{
    public class RefreshTokenException : Exception
    {
        public RefreshTokenException(string message)
            : base(message)
        {
        }
    }
}
