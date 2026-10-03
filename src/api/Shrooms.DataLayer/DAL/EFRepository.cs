using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using X.PagedList;
using X.PagedList.Extensions;

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
                queryableSet = queryableSet.Where(OrganizationFilter(OrganizationId));
            }

            if (filter != null)
            {
                queryableSet = queryableSet.Where(filter);
            }

            if (maxResults > 0)
            {
                queryableSet = queryableSet.Take(maxResults);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in SafeQuery.ValidIncludePaths<TEntity>(includeProperties, IsMapped))
                {
                    queryableSet = queryableSet.Include(includeProperty.Trim());
                }

                queryableSet = queryableSet.AsSplitQuery();
            }

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                queryableSet = SafeQuery.OrderBy(queryableSet, orderBy, IsMapped);
            }

            return queryableSet;
        }

        public virtual IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, int maxResults = 0, Expression<Func<TEntity, DateTime>> orderBy = null, string includeProperties = "",
           int? organizationId = 2)
        {
            IQueryable<TEntity> queryableSet = _dbSet;

            if (typeof(IOrganization).IsAssignableFrom(typeof(TEntity)))
            {
                queryableSet = queryableSet.Where(OrganizationFilter(OrganizationId));
            }

            if (filter != null)
            {
                queryableSet = queryableSet.Where(filter);
            }

            if (maxResults > 0)
            {
                queryableSet = queryableSet.Take(maxResults);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in SafeQuery.ValidIncludePaths<TEntity>(includeProperties, IsMapped))
                {
                    queryableSet = queryableSet.Include(includeProperty.Trim());
                }

                queryableSet = queryableSet.AsSplitQuery();
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

            // Use ToPagedList() which is synchronous, as X.PagedList doesn't have proper EF Core async support
            return await Task.FromResult(queryableSet.ToPagedList(page.Value, pageSize));
        }

        // Only members the EF model knows about (scalars, navigations, skip navigations) are queryable;
        // [NotMapped] and fluent-ignored CLR properties would fail at translation time.
        private bool IsMapped(PropertyInfo property)
        {
            if (_context is not DbContext dbContext)
            {
                return true;
            }

            var entityType = dbContext.Model.FindEntityType(property.DeclaringType!);
            if (entityType == null)
            {
                // Owned/complex types or non-entities: let the provider decide.
                return true;
            }

            return entityType.FindProperty(property.Name) != null
                || entityType.FindNavigation(property.Name) != null
                || entityType.FindSkipNavigation(property.Name) != null
                || entityType.FindComplexProperty(property.Name) != null;
        }

        // Typed replacement for the former Dynamic LINQ "OrganizationId=N || OrganizationId=null" filter.
        private static Expression<Func<TEntity, bool>> OrganizationFilter(int organizationId)
        {
            var entity = Expression.Parameter(typeof(TEntity), "e");
            var property = Expression.Property(entity, ClaimOrganizationId);

            Expression body = property.Type == typeof(int?)
                ? Expression.OrElse(
                    Expression.Equal(property, Expression.Constant(organizationId, typeof(int?))),
                    Expression.Equal(property, Expression.Constant(null, typeof(int?))))
                : Expression.Equal(property, Expression.Constant(organizationId));

            return Expression.Lambda<Func<TEntity, bool>>(body, entity);
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