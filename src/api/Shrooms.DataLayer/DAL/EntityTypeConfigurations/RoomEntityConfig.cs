using System.Data.Entity.ModelConfiguration;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class RoomEntityConfig : EntityTypeConfiguration<Room>
    {
        public RoomEntityConfig()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false))
                .HasMany(r => r.ApplicationUsers)
                .WithOptional(e => e.Room)
                .HasForeignKey(e => e.RoomId)
                .WillCascadeOnDelete(false);

            HasRequired(r => r.Organization)
                .WithMany()
                .HasForeignKey(r => r.OrganizationId)
                .WillCascadeOnDelete(false);
        }
    }
}
