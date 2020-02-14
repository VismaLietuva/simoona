namespace Shrooms.DataLayer.EntityModels.Models
{
    public interface IOrganization
    {
        int OrganizationId { get; set; }

        Organization Organization { get; set; }
    }
}
