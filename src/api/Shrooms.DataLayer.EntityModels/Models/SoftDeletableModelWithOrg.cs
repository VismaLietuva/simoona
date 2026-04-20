namespace Shrooms.DataLayer.EntityModels.Models
{
    public class SoftDeletableModelWithOrg : SoftDeletableModel, IOrganization
    {
        public int OrganizationId { get; set; }

        public Organization Organization { get; set; }
    }
}
