using System.Collections.Generic;
using System.Linq;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.DAL;
using Shrooms.Infrastructure.Configuration;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.Mocks
{
    public class NewMockRepository<TEntity> : EfRepository<TEntity>
        where TEntity : class
    {
        private readonly List<TEntity> _listContext;

        public NewMockRepository(IDbContext context)
            : base(context, new ApplicationSettings())
        {
            _listContext = context.Set<TEntity>().ToList();
        }

        public override TEntity GetByID(object id)
        {
            var entity = _listContext.Find(t =>
            {
                var idProperty = t.GetType().GetProperty("Id");

                if (idProperty == null)
                {
                    return false;
                }

                var idValue = idProperty.GetValue(t, null);

                if (idValue is int value)
                {
                    return value == (int)id;
                }

                if (idValue is string)
                {
                    return idValue.ToString() == id.ToString();
                }

                return false;
            });

            _dbSet.SetDbSetData(_listContext.AsQueryable());

            return entity;
        }

        public override void Insert(TEntity entity)
        {
            _listContext.Add(entity);
            _dbSet.SetDbSetData(_listContext.AsQueryable());
        }

        public override void Delete(TEntity entityToDelete)
        {
            _listContext.Remove(entityToDelete);
            _dbSet.SetDbSetData(_listContext.AsQueryable());
        }

        public override void Update(TEntity entity)
        {
            // Pretty dirty method, but for testing should be sufficient
            var idProperty = entity.GetType().GetProperty("Id");

            if (idProperty != null)
            {
                var id = idProperty.GetValue(entity, null);

                var entityInList = GetByID(id);
                _listContext.Remove(entityInList);
                _listContext.Add(entity);
            }

            _dbSet.SetDbSetData(_listContext.AsQueryable());
        }
    }
}
