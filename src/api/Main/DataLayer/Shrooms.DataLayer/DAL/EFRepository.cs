using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using PagedList;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Host.Contracts.Infrastructure;

namespace Shrooms.DataLayer.DAL
{
    public class EfRepository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        protected IDbContext _context;
        private readonly IApplicationSettings _appSettings;

        protected DbSet<TEntity> _dbSet;
        public const string ClaimOrganizationId = "OrganizationId";

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

        public virtual IPagedList<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter = null, int maxResults = 0, string orderBy = null, string includeProperties = "",
            int? page = null, int pageSize = 30)
        {
            var queryableSet = this.Get(filter, maxResults, orderBy, includeProperties);

            page = page ?? 1;

            return queryableSet.ToPagedList(page.Value, pageSize);
        }

        public virtual TEntity GetByID(object id)
        {
            return _dbSet.Find(id);
        }

        public virtual void Insert(TEntity entity)
        {
            if (typeof(IOrganization).IsAssignableFrom(typeof(TEntity)))
            {
                var type = entity.GetType();
                var property = type.GetProperty(ClaimOrganizationId);
                property.SetValue(entity, OrganizationId);
            }

            _dbSet.Add(entity);
        }

        public virtual void Update(TEntity entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void DeleteById(object id)
        {
            TEntity entityToDelete = this.GetByID(id);
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