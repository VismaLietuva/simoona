using Shrooms.Contracts.Enums;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments
{
    public class CommentCreatedDto
    {
        public int WallId { get; set; }

        public int PostId { get; set; }

        public int CommentId { get; set; }

        public WallType WallType { get; set; }

        public string PostAuthor { get; set; }

        public string CommentAuthor { get; set; }

        public IEnumerable<string> MentionedUserIds { get; set; }
    }
}
