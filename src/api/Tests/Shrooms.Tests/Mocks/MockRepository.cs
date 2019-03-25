using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DataLayer;
using PagedList;
using System.Data.Entity;
using DataLayer.Models;
using System.Linq.Dynamic;

namespace Ace.Shrooms.Tests
{
    public class MockRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly MockDbContext _context;
        private List<TEntity> _listContext;
        public DbSet<TEntity> DbSet { get; set; }

        public int OrganizationId
        {
            get { return 2; }
            set {}
        }

        public MockRepository(MockDbContext context)
        {
            _context = context;
            _listContext = _context.GetList<TEntity>();
            DbSet = _context.Set<TEntity>();
        }

        public TEntity GetByID(object id)
        {
            TEntity entity = _listContext.Find(t =>
            {
                var idProperty = t.GetType().GetProperty("Id");

                if (idProperty != null)
                {
                    var idValue = idProperty.GetValue(t, null);

                    if (idValue is int)
                    {
                        return (int)idValue == (int)id;
                    }
                    else if (idValue is string)
                    {
                        return idValue.ToString() == id.ToString();
                    }
                }

                return false;
            });

            return entity;
        }

        public void Insert(TEntity entity)
        {
            _listContext.Add(entity);
        }

        public void Delete(TEntity entityToDelete)
        {
            this._listContext.Remove(entityToDelete);
        }

        public void Update(TEntity entity)
        {
            // Pretty dirty method, but for testing should be sufficient
            var idProperty = entity.GetType().GetProperty("Id");

            if (idProperty != null)
            {
                var id = idProperty.GetValue(entity, null);

                var entityInList = GetByID(id);
                _listContext.Remove(entityInList);
                Insert(entity);
            }
        }

        public IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, int maxResults = 0, string orderBy = null, string includeProperties = "", int? organizationId = default(int?))
        {
            IQueryable<TEntity> queryableSet = DbSet;

            if (typeof(IOrganization).IsAssignableFrom(typeof(TEntity)))
            {
                queryableSet = queryableSet.Where(string.Format("{0}={1} || {0}=null", Constants.ClaimOrganizationId, OrganizationId));
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

        public IPagedList<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter = null, int maxResults = 0, string orderBy = null, string includeProperties = "", int? page = default(int?), int pageSize = 30)
        {
            var queryableSet = this.Get(filter, maxResults, orderBy, includeProperties);

            page = page ?? 1;

            return queryableSet.ToPagedList(page.Value, pageSize);
        }

        public void DeleteById(object id)
        {
            TEntity entityToDelete = this.GetByID(id);
            Delete(entityToDelete);
        }
    }
}