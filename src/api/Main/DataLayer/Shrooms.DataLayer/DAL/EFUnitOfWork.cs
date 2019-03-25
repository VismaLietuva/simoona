using System;
using System.Collections.Generic;
using System.Linq;

namespace Shrooms.DataLayer
{
    public class EFUnitOfWork : IUnitOfWork
    {
        private readonly IDbContext _dbContext;
        private readonly Dictionary<Type, object> _repositories;

        public IDbContext DbContext
        {
            get { return _dbContext; }
        }

        public EFUnitOfWork(IDbContext context)
        {
            _dbContext = context;
            _repositories = new Dictionary<Type, object>();
        }

        public IRepository<TEntity> GetRepository<TEntity>(int organizationId = 2)
            where TEntity : class
        {
            IRepository<TEntity> repository;

            if (_repositories.Keys.Contains(typeof(TEntity)))
            {
                repository = _repositories[typeof(TEntity)] as IRepository<TEntity>;
                return repository;
            }

            repository = new EFRepository<TEntity>(DbContext);
            _repositories.Add(typeof(TEntity), repository);
            return repository;
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }

        public T GetDbContextAs<T>()
            where T : class, IDbContext
        {
            return _dbContext as T;
        }
    }
}