using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using X.PagedList;

namespace Shrooms.DataLayer.DAL
{
    public class EfRepository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        private const string ClaimOrganizationId = "OrganizationId";

        private readonly IDbContext _context;
        private readonly IApplicationSettings _appSettings;
        protected readonly DbSet<TEntity> _dbSet;

        public int OrganizationId
        {
            get => _appSettings.DefaultOrganizationId;
            set { }
        }

        public EfRepository(IDbContext context, IApplicationSettings appSettings)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
            _appSettings = appSettings;
        }

        public virtual IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, int maxResults = 0, string orderBy = null, string includeProperties = "",
            int? organizationId = 2)
        {
            IQueryable<TEntity> queryableSet = _dbSet;

            if (typeof(IOrganization).IsAssignableFrom(typeof(TEntity)))
            {
                queryableSet = queryableSet.Where(string.Format("{0}={1} || {0}=null", ClaimOrganizationId, OrganizationId));
            }

            if (filter != null)
            {
                queryableSet = queryableSet.Where(filter);
            }

            if (maxResults > 0)
            {
                queryableSet = queryableSet.Take(() => maxResults);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    queryableSet = queryableSet.Include(includeProperty);
                }
            }

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                queryableSet = queryableSet.OrderBy(orderBy);
            }

            return queryableSet;
        }

        public virtual IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, int maxResults = 0, Expression<Func<TEntity, DateTime>> orderBy = null, string includeProperties = "",
           int? organizationId = 2)
        {
            IQueryable<TEntity> queryableSet = _dbSet;

            if (typeof(IOrganization).IsAssignableFrom(typeof(TEntity)))
            {
                queryableSet = queryableSet.Where(string.Format("{0}={1} || {0}=null", ClaimOrganizationId, OrganizationId));
            }

            if (filter != null)
            {
                queryableSet = queryableSet.Where(filter);
            }

            if (maxResults > 0)
            {
                queryableSet = queryableSet.Take(() => maxResults);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    queryableSet = queryableSet.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                queryableSet = queryableSet.OrderBy(orderBy);
            }

            return queryableSet;
        }

        public virtual async Task<IPagedList<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter = null,
            int maxResults = 0,
            string orderBy = null,
            string includeProperties = "",
            int? page = null,
            int pageSize = 30)
        {
            var queryableSet = Get(filter, maxResults, orderBy, includeProperties);

            page = page ?? 1;

            return await queryableSet.ToPagedListAsync(page.Value, pageSize);
        }

        public virtual async Task<TEntity> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual void Insert(TEntity entity)
        {
            if (typeof(IOrganization).IsAssignableFrom(typeof(TEntity)))
            {
                var type = entity.GetType();
                var property = type.GetProperty(ClaimOrganizationId);
                property?.SetValue(entity, OrganizationId);
            }

            _dbSet.Add(entity);
        }

        public virtual void Update(TEntity entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual async Task DeleteByIdAsync(object id)
        {
            var entityToDelete = await GetByIdAsync(id);
            Delete(entityToDelete);
        }

        public virtual void Delete(TEntity entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }

            _dbSet.Remove(entity);
        }
    }
}