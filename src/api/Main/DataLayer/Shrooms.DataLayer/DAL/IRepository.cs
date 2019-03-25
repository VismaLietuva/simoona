using System;
using System.Linq;
using System.Linq.Expressions;
using PagedList;

namespace Shrooms.DataLayer
{
    public interface IRepository<TEntity>
        where TEntity : class
    {
        int OrganizationId { get; set; }

        IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, int maxResults = 0, string orderBy = null, string includeProperties = "", int? organizationId = 2);

        IPagedList<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter = null, int maxResults = 0, string orderBy = null, string includeProperties = "", int? page = null, int pageSize = 30);

        TEntity GetByID(object id);

        void Insert(TEntity entity);

        void DeleteById(object id);

        void Delete(TEntity entity);

        void Update(TEntity entity);
    }
}