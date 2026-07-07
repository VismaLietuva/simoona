using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class PostEntityConfig : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.Property(x => x.LastActivity)
                .HasColumnType("datetime2");

            builder.HasIndex(x => x.LastActivity)
                .HasDatabaseName("IX_LastActivity");
        }
    }
}
