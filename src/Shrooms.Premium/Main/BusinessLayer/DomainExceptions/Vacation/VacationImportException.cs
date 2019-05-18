using System;

namespace Shrooms.Premium.Main.BusinessLayer.DomainExceptions.Vacation
{
    public class VacationImportException : Exception
    {
        public VacationImportException(string message)
            : base(message)
        {
        }
    }
}
