using System.Threading.Tasks;

namespace Shrooms.Contracts.DAL
{
    public interface IUnitOfWork
    {
        IDbContext DbContext { get; }

        IRepository<TEntity> GetRepository<TEntity>(int organizationId = 2)
            where TEntity : class;

        T GetDbContextAs<T>()
            where T : class, IDbContext;

        void Save();

        Task SaveAsync();
    }
}