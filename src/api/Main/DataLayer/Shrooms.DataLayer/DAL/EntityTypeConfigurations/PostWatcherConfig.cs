using System.Data.Entity.ModelConfiguration;
using Shrooms.EntityModels.Models.Multiwall;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    public class PostWatcherConfig : EntityTypeConfiguration<PostWatcher>
    {
        public PostWatcherConfig()
        {
            ToTable("PostWatchers", "dbo");
        }
    }
}
