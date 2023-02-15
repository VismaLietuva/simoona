using Shrooms.Contracts.DataTransferObjects.Wall;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Events
{
    public class SharedEventEmailDto
    {
        public NewlyCreatedPostDto CreatedPost { get; set; }

        public IEnumerable<WallMemberEmailReceiverDto> Receivers { get; set; }

        public SharedEventEmailDetailsDto Details { get; set; }
    }
}
