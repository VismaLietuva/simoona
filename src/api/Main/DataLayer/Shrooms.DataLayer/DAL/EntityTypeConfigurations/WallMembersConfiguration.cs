namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    using System.Data.Entity.ModelConfiguration;
    using Shrooms.EntityModels.Models.Multiwall;

    internal class WallMembersConfiguration : EntityTypeConfiguration<WallMember>
    {
        public WallMembersConfiguration()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));
        }
    }
}
