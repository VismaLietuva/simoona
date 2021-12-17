using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Vacations
{
    public class VacationImportStatusDto
    {
        public IList<VacationImportEntryDto> Imported { get; set; }
        public IList<VacationImportEntryDto> Skipped { get; set; }
    }
}