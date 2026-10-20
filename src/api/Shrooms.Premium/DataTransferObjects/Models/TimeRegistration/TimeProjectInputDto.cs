using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.TimeRegistration
{
    public class TimeProjectInputDto
    {
        public int? Id { get; set; }

        public int CustomerId { get; set; }

        public string Name { get; set; }

        public IList<TimeTaskInputDto> Tasks { get; set; }

        public IList<string> MemberIds { get; set; }
    }
}
