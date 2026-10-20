using System;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Wall.Posts
{
    public class EditScheduledPostDto : UserAndOrganizationDto
    {
        public int Id { get; set; }

        public string MessageBody { get; set; }

        public IEnumerable<string> Images { get; set; }

        public DateTime ScheduledFor { get; set; }
    }
}
