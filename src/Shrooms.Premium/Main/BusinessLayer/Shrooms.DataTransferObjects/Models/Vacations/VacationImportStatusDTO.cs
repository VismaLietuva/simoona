using System.Collections.Generic;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Vacations
{
    public class VacationImportStatusDTO
    {
        public IList<VacationImportEntryDTO> Imported { get; set; }
        public IList<VacationImportEntryDTO> Skipped { get; set; }
    }

    public class VacationImportEntryDTO
    {
        public string Code { get; set; }
        public string FullName { get; set; }
    }
}