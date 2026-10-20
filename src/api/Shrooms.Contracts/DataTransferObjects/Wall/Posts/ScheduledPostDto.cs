using System;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Wall.Posts
{
    public class ScheduledPostDto
    {
        public int Id { get; set; }

        public int WallId { get; set; }

        public string WallName { get; set; }

        public string MessageBody { get; set; }

        public IEnumerable<string> Images { get; set; }

        public DateTime ScheduledFor { get; set; }
    }
}
