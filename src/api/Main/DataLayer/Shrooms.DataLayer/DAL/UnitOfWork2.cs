using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Shrooms.DataLayer.DAL
{
    public class UnitOfWork2 : IUnitOfWork2
    {
        private readonly IDbContext _context;

        public UnitOfWork2(IDbContext context)
        {
            _context = context;
        }

        public IDbSet<T> GetDbSet<T>()
            where T : class
        {
            return _context.Set<T>();
        }

        [Obsolete("Use SaveChanges(string userId) or SaveChangesAsync(string userId) instead")]
        public void SaveChanges(bool useMetaTracking)
        {
            _context.SaveChanges(useMetaTracking);
        }

        public void SaveChanges(string userId)
        {
            _context.SaveChanges(userId);
        }

        public async Task<int> SaveChangesAsync(string userId)
        {
            return await _context.SaveChangesAsync(userId);
        }

        public async Task<int> SaveChangesAsync(bool useMetaTracking)
        {
            return await _context.SaveChangesAsync(useMetaTracking);
        }
    }
}
