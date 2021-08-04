using System.Data.Entity;
using System.Threading.Tasks;

namespace Shrooms.Contracts.DAL
{
    public interface IUnitOfWork2
    {
        string ConnectionName { get; }

        DbSet<T> GetDbSet<T>()
            where T : class;

        Task<int> SaveChangesAsync(string userId);

        Task<int> SaveChangesAsync(bool useMetaTracking = true);
    }
}
