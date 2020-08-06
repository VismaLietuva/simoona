namespace Shrooms.Contracts.DataTransferObjects.Users
{
    public class MentionedUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName { get { return $"{FirstName} {LastName}"; } }
    }
}
