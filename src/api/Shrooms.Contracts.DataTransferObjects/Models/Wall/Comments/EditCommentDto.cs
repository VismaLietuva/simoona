using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments
{
    public class EditCommentDto : UserAndOrganizationDto
    {
        public int Id { get; set; }
        
        public string MessageBody { get; set; }

        public ICollection<string> Images { get; set; }

        public IEnumerable<string> MentionedUserIds { get; set; }
    }
}
