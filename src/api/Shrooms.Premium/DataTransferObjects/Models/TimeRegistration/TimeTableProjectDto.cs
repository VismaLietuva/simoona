using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.TimeRegistration
{
    public class TimeTableProjectDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CustomerName { get; set; }

        public bool IsArchived { get; set; }

        public bool IsAssigned { get; set; }

        public IEnumerable<TimeTaskDto> Tasks { get; set; }
    }
}
