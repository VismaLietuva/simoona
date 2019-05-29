using System;

namespace Shrooms.DomainExceptions.Exceptions.Vacation
{
    public class VacationImportException : Exception
    {
        public VacationImportException(string message)
            : base(message)
        {
        }
    }
}
