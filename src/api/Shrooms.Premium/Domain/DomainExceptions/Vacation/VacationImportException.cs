using System;

namespace Shrooms.Premium.Domain.DomainExceptions.Vacation
{
    public class VacationImportException : Exception
    {
        public VacationImportException(string message)
            : base(message)
        {
        }
    }
}
