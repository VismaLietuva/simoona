namespace Shrooms.DataLayer.EntityModels.Models
{
    public class BaseModelWithOrg : BaseModel, IOrganization
    {
        public int OrganizationId { get; set; }

        public Organization Organization { get; set; }
    }
}
