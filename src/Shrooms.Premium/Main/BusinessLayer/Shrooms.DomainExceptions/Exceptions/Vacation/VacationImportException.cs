using System;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.DomainExceptions.Exceptions.Vacation
{
    public class VacationImportException : Exception
    {
        public VacationImportException(string message)
            : base(message)
        {
        }
    }
}
