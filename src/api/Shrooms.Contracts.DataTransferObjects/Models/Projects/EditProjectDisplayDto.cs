using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects.Users;

namespace Shrooms.Contracts.DataTransferObjects.Models.Projects
{
    public class EditProjectDisplayDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public UserDto Owner { get; set; }

        public string Logo { get; set; }

        public IEnumerable<UserDto> Members { get; set; }

        public IEnumerable<string> Attributes { get; set; }
    }
}
