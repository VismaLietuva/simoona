using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    public class KudosLogsEntriesDto<T>
    {
        public IEnumerable<T> KudosLogs { get; set; }
        public int TotalKudosCount { get; set; }
    }
}
