using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;

namespace Shrooms.DataLayer.DAL
{
    public static class SoftDeleteHandler
    {
        private static readonly Dictionary<Type, IEntityType> _mappingCache = new Dictionary<Type, IEntityType>();
        private static ShroomsDbContext _context;

        public static void Execute(IEnumerable<EntityEntry> entries, ShroomsDbContext ctx)
        {
            _context = ctx;

            var deletedItems = entries.Where(p => p.State == EntityState.Deleted && p.Entity is ISoftDelete).ToList();

            foreach (var entry in deletedItems)
            {
                var e = entry.Entity;
                var id = GetEntityId(e);

                var tableName = GetTableName(e.GetType());
                _context.Database.ExecuteSqlRaw($"UPDATE {tableName} SET IsDeleted = 1 WHERE ID = @id", new SqlParameter("id", id));

                // Marking it Unchanged prevents the hard delete - entry.State = EntityState.Unchanged;
                // So does setting it to Detached and that is what EF does when it deletes an item: http://msdn.microsoft.com/en-us/data/jj592676.aspx
                entry.State = EntityState.Detached;
            }
        }

        public static async Task ExecuteAsync(IEnumerable<EntityEntry> entries, ShroomsDbContext ctx)
        {
            _context = ctx;

            var deletedItems = entries.Where(p => p.State == EntityState.Deleted && p.Entity is ISoftDelete).ToList();

            foreach (var entry in deletedItems)
            {
                var e = entry.Entity;
                var id = GetEntityId(e);

                var tableName = GetTableName(e.GetType());
                await _context.Database.ExecuteSqlRawAsync($"UPDATE {tableName} SET IsDeleted = 1 WHERE ID = @id", new SqlParameter("id", id));

                // Marking it Unchanged prevents the hard delete - entry.State = EntityState.Unchanged;
                // So does setting it to Detached and that is what EF does when it deletes an item: http://msdn.microsoft.com/en-us/data/jj592676.aspx
                entry.State = EntityState.Detached;
            }
        }

        private static string GetEntityId(object e)
        {
            if (e is IdentityUser identityUser)
            {
                return identityUser.Id;
            }
            else if (e is ApplicationRole role)
            {
                return role.Id;
            }
            else if (e is BaseModel model)
            {
                return model.Id.ToString();
            }
            else if (e is Event @event)
            {
                return @event.Id.ToString();
            }
            else if (e is EventReminder reminder)
            {
                return reminder.Id.ToString();
            }

            throw new ArgumentException("Id not found in SoftDelete() method");
        }

        internal static string GetTableName(Type type)
        {
            var entityType = GetEntityType(type);
            var schema = entityType.GetSchema() ?? "dbo";
            var tableName = entityType.GetTableName();

            return $"[{schema}].[{tableName}]";
        }

        internal static Type GetObjectType(Type type)
        {
            // EF Core doesn't have dynamic proxies like EF6, but we keep this for compatibility
            if (type.Namespace == "Castle.Proxies")
            {
                return type.BaseType;
            }
            return type;
        }

        private static IEntityType GetEntityType(Type type)
        {
            if (_mappingCache.ContainsKey(type))
            {
                return _mappingCache[type];
            }

            type = GetObjectType(type);

            var entityType = _context.Model.FindEntityType(type);

            if (entityType == null)
            {
                // Try with base type
                if (type.BaseType != null && type.BaseType != typeof(object))
                {
                    entityType = _context.Model.FindEntityType(type.BaseType);
                }
            }

            if (entityType == null)
            {
                throw new ArgumentException($"Entity type not found in GetEntityType() method: {type.Name}");
            }

            _mappingCache.Add(type, entityType);

            return entityType;
        }
    }
}
