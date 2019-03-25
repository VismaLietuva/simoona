using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.EntityModels.Models
{
    public class ServiceRequestComment : BaseModelWithOrg
    {
        public string EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public ApplicationUser Employee { get; set; }

        public int ServiceRequestId { get; set; }

        [ForeignKey("ServiceRequestId")]
        public ServiceRequest ServiceRequest { get; set; }

        public string Content { get; set; }
    }
}
