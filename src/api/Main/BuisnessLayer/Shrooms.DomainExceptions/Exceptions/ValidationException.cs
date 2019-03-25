using System;

namespace Shrooms.DomainExceptions.Exceptions
{
    public class ValidationException : Exception
    {
        private readonly string _errorMessage;
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
        }

        private readonly int _errorCode;
        public int ErrorCode
        {
            get
            {
                return _errorCode;
            }
        }

        public ValidationException(int errorCode)
        {
            _errorCode = errorCode;
        }

        public ValidationException(int errorCode, string errorMessage)
        {
            _errorMessage = errorMessage;
            _errorCode = errorCode;
        }
    }
}
