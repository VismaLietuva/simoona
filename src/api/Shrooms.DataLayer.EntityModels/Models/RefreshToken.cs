using System;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class RefreshToken : IOrganization, ITrackable
    {
        [Key]
        public string Id { get; set; }

        public string Subject { get; set; }

        public DateTime IssuedUtc { get; set; }

        public DateTime ExpiresUtc { get; set; }

        public string ProtectedTicket { get; set; }

        public int OrganizationId { get; set; }

        public Organization Organization { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime Modified { get; set; }

        public string ModifiedBy { get; set; }
    }
}
