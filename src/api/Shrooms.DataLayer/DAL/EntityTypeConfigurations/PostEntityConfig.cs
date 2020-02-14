using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class PostEntityConfig : EntityTypeConfiguration<Post>
    {
        public PostEntityConfig()
        {
            Property(x => x.LastActivity)
                .HasColumnType("datetime2")
                .HasColumnAnnotation("Index", new IndexAnnotation(
                    new IndexAttribute("IX_LastActivity")
                    {
                        IsUnique = false,
                        IsClustered = false
                    }))
                ;
        }
    }
}
