using System.Collections.Generic;
using System.Linq;
using Shrooms.DataLayer;
using Shrooms.UnitTests.Extensions;

namespace Shrooms.UnitTests.Mocks
{
    public class NewMockRepository<TEntity> : EFRepository<TEntity>
        where TEntity : class
    {
        private readonly List<TEntity> _listContext;

        public NewMockRepository(IDbContext context)
            : base(context)
        {
            _listContext = context.Set<TEntity>().ToList();
        }

        public override TEntity GetByID(object id)
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

                    if (idValue is string)
                    {
                        return idValue.ToString() == id.ToString();
                    }
                }

                return false;
            });

            DbSet.SetDbSetData(_listContext.AsQueryable());

            return entity;
        }

        public override void Insert(TEntity entity)
        {
            _listContext.Add(entity);
            DbSet.SetDbSetData(_listContext.AsQueryable());
        }

        public override void Delete(TEntity entityToDelete)
        {
            this._listContext.Remove(entityToDelete);
            DbSet.SetDbSetData(_listContext.AsQueryable());
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

            DbSet.SetDbSetData(_listContext.AsQueryable());
        }
    }
}
