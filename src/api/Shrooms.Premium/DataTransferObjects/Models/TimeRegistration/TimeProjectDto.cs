using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.TimeRegistration
{
    public class TimeProjectDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public bool IsArchived { get; set; }

        public IEnumerable<TimeTaskDto> Tasks { get; set; }

        public IEnumerable<TimeEmployeeDto> Members { get; set; }
    }
}
