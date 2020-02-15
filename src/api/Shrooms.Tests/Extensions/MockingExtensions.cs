using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using NSubstitute;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Tests.Mocks;

namespace Shrooms.Tests.Extensions
{
    public static class MockingExtensions
    {
        /// <summary>
        /// This extension method creates mock of IUnitOFWork with IDbContext mock
        /// </summary>
        /// <returns>Mock object of IUnitOFWork</returns>
        public static IUnitOfWork GetUnitOfWorkMock()
        {
            var context = Substitute.For<IDbContext>();
            var unitOfWork = Substitute.For<IUnitOfWork>();

            unitOfWork.GetDbContextAs<IDbContext>().Returns(context);
            return unitOfWork;
        }

        /// <summary>
        /// This mehtod fakes repository that unit of work creates to NewMockRepository
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="unitOfWork">Mock of unit of work</param>
        /// <param name="data">Collection of entity that repository will operate on</param>
        public static void MockRepositoryWithEntities<TEntity>(this IUnitOfWork unitOfWork, IEnumerable<TEntity> data)
            where TEntity : class, ISoftDelete
        {
            var dataList = data.ToList();
            unitOfWork.MockSetWithData(dataList);
            IRepository<TEntity> repository = new NewMockRepository<TEntity>(unitOfWork.DbContext);
            unitOfWork.GetRepository<TEntity>().Returns(repository);
        }

        public static void SetDbSetData<T>(this IQueryable<T> dbSet, IQueryable<T> data)
            where T : class
        {
            dbSet.Provider.Returns(data.Provider);
            dbSet.Expression.Returns(data.Expression);
            dbSet.ElementType.Returns(data.ElementType);
            dbSet.GetEnumerator().Returns(data.GetEnumerator());
        }

        public static void SetDbSetData<T>(this IQueryable<T> dbSet, IEnumerable<T> data)
            where T : class
        {
            var dataQueryable = data.AsQueryable();
            dbSet.Provider.Returns(dataQueryable.Provider);
            dbSet.Expression.Returns(dataQueryable.Expression);
            dbSet.ElementType.Returns(dataQueryable.ElementType);
            dbSet.GetEnumerator().Returns(dataQueryable.GetEnumerator());
        }

        public static void SetDbSetDataForAsync<T>(this IQueryable<T> dbSet, IEnumerable<T> data)
            where T : class
        {
            var dataQueryable = data.AsQueryable();
            ((IDbAsyncEnumerable<T>)dbSet).GetAsyncEnumerator()
                .Returns(new TestDbAsyncEnumerator<T>(dataQueryable.GetEnumerator()));

            dbSet.Provider.Returns(new TestDbAsyncQueryProvider<T>(dataQueryable.Provider));
            dbSet.Expression.Returns(dataQueryable.Expression);
            dbSet.ElementType.Returns(dataQueryable.ElementType);
            dbSet.GetEnumerator().Returns(dataQueryable.GetEnumerator());
        }

        public static IDbSet<T> MockDbSet<T>(this IUnitOfWork2 uow, IEnumerable<T> data = null)
            where T : class
        {
            var dbSetMock = Substitute.For<IDbSet<T>, IQueryable<T>, IDbAsyncEnumerable<T>>();
            uow.GetDbSet<T>().Returns(dbSetMock);

            if (data != null)
            {
                dbSetMock.SetDbSetData(data);
            }

            return dbSetMock;
        }

        public static IDbSet<T> MockDbSetForAsync<T>(this IUnitOfWork2 uow, IEnumerable<T> data = null)
            where T : class
        {
            var dbSetMock = Substitute.For<IDbSet<T>, IQueryable<T>, IDbAsyncEnumerable<T>>();
            uow.GetDbSet<T>().Returns(dbSetMock);

            if (data != null)
            {
                dbSetMock.SetDbSetDataForAsync(data);
            }

            return dbSetMock;
        }

        /// <summary>
        /// Creates Dbset of entity T if one does not exist and mocks with fake data
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="unitOfWork">Unit of work to create DbSet into</param>
        /// <param name="data">Fake entities collection</param>
        private static void MockSetWithData<TEntity>(this IUnitOfWork unitOfWork, IEnumerable<TEntity> data)
            where TEntity : class
        {
            var context = unitOfWork.DbContext;
            var dbSet = context.Set<TEntity>() ?? Substitute.For<DbSet<TEntity>, IQueryable<TEntity>>();
            dbSet.SetDbSetData(data.AsQueryable());
            context.Set<TEntity>().Returns(dbSet);
        }
    }
}