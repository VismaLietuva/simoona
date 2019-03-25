using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Shrooms.EntityModels.Models
{
    public class ApplicationRole : IdentityRole, ISoftDelete, IOrganization
    {
        public ApplicationRole()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="roleName">Role name</param>
        /// <param name="organizationId">Organization to which this role belongs (if null - role is global)</param>
        public ApplicationRole(string roleName, int organizationId)
            : base(roleName)
        {
            this.OrganizationId = organizationId;
            this.CreatedTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Role Name
        /// </summary>
        public new string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

        public int OrganizationId { get; set; }

        public Organization Organization { get; set; }

        /// <summary>
        /// Role allowable permissions
        /// </summary>
        public virtual ICollection<Permission> Permissions { get; set; }

        /// <summary>
        /// Role create time
        /// </summary>
        public DateTime CreatedTime { get; set; }
    }
}
