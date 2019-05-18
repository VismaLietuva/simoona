using System;

namespace Shrooms.DomainServiceValidators.Validators.RefreshTokens
{
    public class RefreshTokenException : Exception
    {
        public RefreshTokenException(string message)
            : base(message)
        {
        }
    }
}
