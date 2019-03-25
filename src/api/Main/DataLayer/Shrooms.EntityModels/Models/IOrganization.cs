namespace Shrooms.EntityModels.Models
{
    public interface IOrganization
    {
        int OrganizationId { get; set; }

        Organization Organization { get; set; }
    }
}
