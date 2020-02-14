namespace Shrooms.Contracts.DataTransferObjects.Models.Wall
{
    public class WallMemberDto
    {
        public string FullName { get; set; }

        public string ProfilePicture { get; set; }

        public bool IsCurrentUser { get; set; }

        public string Id { get; set; }

        public string JobTitle { get; set; }

        public bool IsModerator { get; set; }
    }
}
