using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading.Tasks;

namespace Shrooms.Contracts.DAL
{
    public interface IDbContext
    {
        string ConnectionName { get; }
        DbSet<T> Set<T>()
            where T : class;

        EntityEntry<T> Entry<T>(T entity)
            where T : class;

        int SaveChanges(bool useMetaTracking = true);

        int SaveChanges(string userId);

        Task<int> SaveChangesAsync(string userId);

        Task<int> SaveChangesAsync(bool useMetaTracking = true);

        void Dispose();
    }
}