﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;

namespace Shrooms.Tests.Mocks
{
    public class MockUnitOfWork : IUnitOfWork
    {
        private bool _disposed;
        private readonly MockDbContext _mockDbContext;
        private readonly Dictionary<Type, object> _repositories;

        public MockUnitOfWork()
        {
            _mockDbContext = new MockDbContext();
            _repositories = new Dictionary<Type, object>();
        }

        public IRepository<TEntity> GetRepository<TEntity>(int organizationId = 2)
            where TEntity : class
        {
            if (_repositories.Keys.Contains(typeof(TEntity)))
            {
                return _repositories[typeof(TEntity)] as IRepository<TEntity>;
            }

            var repository = new NewMockRepository<TEntity>(DbContext);

            _repositories.Add(typeof(TEntity), repository);
            return repository;
        }

        public Task SaveAsync()
        {
            return Task.CompletedTask;
        }

        public T GetDbContextAs<T>()
            where T : class, IDbContext
        {
            return _mockDbContext as T;
        }

        public IDbContext DbContext => _mockDbContext;

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _mockDbContext.Dispose();
                }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}