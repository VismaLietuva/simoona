using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;

namespace Shrooms.DataLayer.DAL
{
    public static class SoftDeleteHandler
    {
        private static readonly Dictionary<Type, EntitySetBase> _mappingCache = new Dictionary<Type, EntitySetBase>();
        private static ShroomsDbContext _context;

        public static void Execute(IEnumerable<DbEntityEntry> entries, ShroomsDbContext ctx)
        {
            _context = ctx;

            var deletedItems = entries.Where(p => p.State == EntityState.Deleted && p.Entity is ISoftDelete).ToList();

            foreach (var entry in deletedItems)
            {
                var e = entry.Entity;
                var id = string.Empty;

                if (e is IdentityUser || e is ApplicationRole)
                {
                    id = ((IdentityUser)e).Id;
                }
                else if (e is BaseModel model)
                {
                    id = model.Id.ToString();
                }
                else if (e is Event @event)
                {
                    id = @event.Id.ToString();
                }

                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Id not found in SoftDelete() method", id);
                }

                var tableName = GetTableName(e.GetType());
                _context.Database.ExecuteSqlCommand($"UPDATE {tableName} SET IsDeleted = 1 WHERE ID = @id", new SqlParameter("id", id));

                // Marking it Unchanged prevents the hard delete - entry.State = EntityState.Unchanged;
                // So does setting it to Detached and that is what EF does when it deletes an item: http://msdn.microsoft.com/en-us/data/jj592676.aspx
                entry.State = EntityState.Detached;
            }
        }

        public static async Task ExecuteAsync(IEnumerable<DbEntityEntry> entries, ShroomsDbContext ctx)
        {
            _context = ctx;

            var deletedItems = entries.Where(p => p.State == EntityState.Deleted && p.Entity is ISoftDelete).ToList();

            foreach (var entry in deletedItems)
            {
                var e = entry.Entity;
                var id = string.Empty;

                if (e is IdentityUser || e is ApplicationRole)
                {
                    id = ((IdentityUser)e).Id;
                }
                else if (e is BaseModel model)
                {
                    id = model.Id.ToString();
                }
                else if (e is Event @event)
                {
                    id = @event.Id.ToString();
                }

                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Id not found in SoftDelete() method", id);
                }

                var tableName = GetTableName(e.GetType());
                await _context.Database.ExecuteSqlCommandAsync($"UPDATE {tableName} SET IsDeleted = 1 WHERE ID = @id", new SqlParameter("id", id));

                // Marking it Unchanged prevents the hard delete - entry.State = EntityState.Unchanged;
                // So does setting it to Detached and that is what EF does when it deletes an item: http://msdn.microsoft.com/en-us/data/jj592676.aspx
                entry.State = EntityState.Detached;
            }
        }

        internal static string GetTableName(Type type)
        {
            var entitySet = GetEntitySet(type);

            return $"[{entitySet.Schema}].[{entitySet.Table}]";
        }

        internal static Type GetObjectType(Type type)
        {
            return ObjectContext.GetObjectType(type);
        }

        private static EntitySetBase GetEntitySet(Type type)
        {
            if (_mappingCache.ContainsKey(type))
            {
                return _mappingCache[type];
            }

            type = GetObjectType(type);
            var baseTypeName = type.BaseType?.Name;
            var typeName = type.Name;

            var context = ((IObjectContextAdapter)_context).ObjectContext;
            var entitySet = context.MetadataWorkspace
                            .GetItemCollection(DataSpace.SSpace)
                            .GetItems<EntityContainer>()
                            .SelectMany(c => c.BaseEntitySets.Where(e => e.Name == typeName || e.Name == baseTypeName))
                            .FirstOrDefault();

            if (entitySet == null)
            {
                throw new ArgumentException("Entity type not found in GetEntitySet() method", typeName);
            }

            _mappingCache.Add(type, entitySet);

            return entitySet;
        }
    }
}
