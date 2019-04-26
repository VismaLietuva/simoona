using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using PagedList;
using Shrooms.EntityModels.Models;
using Shrooms.Infrastructure.Configuration;

namespace Shrooms.DataLayer
{
    public class EFRepository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        protected IDbContext Context;
        private readonly IApplicationSettings _appSettings;

        protected DbSet<TEntity> DbSet;
        public const string ClaimOrganizationId = "OrganizationId";

        public int OrganizationId
        {
            get => _appSettings.DefaultOrganizationId;
            set { }
        }

        public EFRepository(IDbContext context, IApplicationSettings appSettings)
        {
            Context = context;
            DbSet = Context.Set<TEntity>();
            _appSettings = appSettings;
        }

        public virtual IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, int maxResults = 0, string orderBy = null, string includeProperties = "",
            int? organizationId = 2)
        {
            IQueryable<TEntity> queryableSet = DbSet;

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
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
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
            IQueryable<TEntity> queryableSet = DbSet;

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
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
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
            return DbSet.Find(id);
        }

        public virtual void Insert(TEntity entity)
        {
            if (typeof(IOrganization).IsAssignableFrom(typeof(TEntity)))
            {
                var type = entity.GetType();
                var property = type.GetProperty(ClaimOrganizationId);
                property.SetValue(entity, OrganizationId);
            }

            DbSet.Add(entity);
        }

        public virtual void Update(TEntity entity)
        {
            DbSet.Attach(entity);
            Context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void DeleteById(object id)
        {
            TEntity entityToDelete = this.GetByID(id);
            Delete(entityToDelete);
        }

        public virtual void Delete(TEntity entity)
        {
            if (Context.Entry(entity).State == EntityState.Detached)
            {
                DbSet.Attach(entity);
            }

            DbSet.Remove(entity);
        }
    }
}