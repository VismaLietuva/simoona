using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    public class PostWatcherConfig : IEntityTypeConfiguration<PostWatcher>
    {
        public void Configure(EntityTypeBuilder<PostWatcher> builder)
        {
            builder.ToTable("PostWatchers", "dbo");
            builder.HasKey(pw => new { pw.PostId, pw.UserId });
        }
    }
}
