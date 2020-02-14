using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;

namespace Shrooms.Contracts.DAL
{
    public interface IDbContext
    {
        string ConnectionName { get; }
        DbSet<T> Set<T>()
            where T : class;

        DbEntityEntry<T> Entry<T>(T entity)
            where T : class;

        int SaveChanges(bool useMetaTracking = true);

        int SaveChanges(string userId);

        Task<int> SaveChangesAsync(string userId);

        Task<int> SaveChangesAsync(bool useMetaTracking);

        void Dispose();
    }
}