using System;

namespace Shrooms.Contracts.DataTransferObjects.Events
{
    public class SharedEventEmailDetailsDto
    {
        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public string TypeName { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }
    }
}
