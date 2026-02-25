using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.DataLayer.DAL
{
    public static class SoftDeleteHandler
    {
        public static void Execute(IEnumerable<EntityEntry> entries, ShroomsDbContext ctx)
        {
            var deletedItems = entries.Where(p => p.State == EntityState.Deleted && p.Entity is ISoftDelete).ToList();

            foreach (var entry in deletedItems)
            {
                var softDeleteEntity = (ISoftDelete)entry.Entity;

                // Switch from hard-delete to a targeted IsDeleted=true update via EF change tracking
                entry.State = EntityState.Unchanged;
                softDeleteEntity.IsDeleted = true;
                entry.Property(nameof(ISoftDelete.IsDeleted)).IsModified = true;
            }
        }

        public static Task ExecuteAsync(IEnumerable<EntityEntry> entries, ShroomsDbContext ctx)
        {
            // Soft delete is synchronous (change tracking only); return completed task for API symmetry
            Execute(entries, ctx);
            return Task.CompletedTask;
        }
    }
}
