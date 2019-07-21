using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Host.Contracts.Infrastructure;
using Shrooms.Infrastructure.Configuration;

namespace Shrooms.DataLayer.DAL
{
    public class EFUnitOfWork : IUnitOfWork
    {
        private readonly Dictionary<Type, object> _repositories;

        public IDbContext DbContext { get; }

        public EFUnitOfWork(IDbContext context)
        {
            DbContext = context;
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

            IApplicationSettings appSettings = new ApplicationSettings();

            repository = new EfRepository<TEntity>(DbContext, appSettings);
            _repositories.Add(typeof(TEntity), repository);
            return repository;
        }

        public void Save()
        {
            DbContext.SaveChanges();
        }

        public T GetDbContextAs<T>()
            where T : class, IDbContext
        {
            return DbContext as T;
        }
    }
}