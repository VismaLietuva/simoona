using System;

namespace Shrooms.Host.Contracts.Exceptions
{
    public class ValidationException : Exception
    {
        public string ErrorMessage { get; }

        public int ErrorCode { get; }

        public ValidationException(int errorCode)
        {
            ErrorCode = errorCode;
        }

        public ValidationException(int errorCode, string errorMessage)
        {
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }
    }
}
