using System;

namespace Shrooms.DataTransferObjects.Models
{
    public class UserAndOrganizationHubDto : UserAndOrganizationDTO
    {
        public string OrganizationName { get; set; }

        public override int GetHashCode()
        {
            return Tuple.Create(UserId, OrganizationId, OrganizationName).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as UserAndOrganizationHubDto);
        }

        public bool Equals(UserAndOrganizationHubDto userOrg)
        {
            if (userOrg != null &&
                this.UserId == userOrg.UserId &&
                this.OrganizationId == userOrg.OrganizationId &&
                this.OrganizationName == userOrg.OrganizationName)
            {
                return true;
            }

            return false;
        }
    }
}
