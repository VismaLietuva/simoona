using System;

namespace Shrooms.Contracts.DataTransferObjects
{
    public class UserAndOrganizationDTO : IEquatable<UserAndOrganizationDTO>
    {
        public int OrganizationId { get; set; }

        public string UserId { get; set; }

        public override int GetHashCode()
        {
            return Tuple.Create(UserId, OrganizationId).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as UserAndOrganizationDTO);
        }

        public bool Equals(UserAndOrganizationDTO userOrg)
        {
            if (userOrg != null && UserId == userOrg.UserId && OrganizationId == userOrg.OrganizationId)
            {
                return true;
            }

            return false;
        }
    }
}
