using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using X.PagedList;

namespace Shrooms.Contracts.DAL
{
    public interface IRepository<TEntity>
        where TEntity : class
    {
        int OrganizationId { get; set; }

        IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, int maxResults = 0, string orderBy = null, string includeProperties = "", int? organizationId = 2);

        Task<IPagedList<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter = null, int maxResults = 0, string orderBy = null, string includeProperties = "", int? page = null, int pageSize = 30);

        Task<TEntity> GetByIdAsync(object id);

        void Insert(TEntity entity);

        Task DeleteByIdAsync(object id);

        void Delete(TEntity entity);

        void Update(TEntity entity);
    }
}