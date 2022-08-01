using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using NSubstitute;
using Shrooms.Contracts.DAL;
using Shrooms.Tests.Mocks;

namespace Shrooms.Tests.Extensions
{
    public static class MockingExtensions
    {
        public static void SetUpControllerForTesting(this ApiController controller)
        {
            controller.ControllerContext = Substitute.For<HttpControllerContext>();
            controller.Request = new HttpRequestMessage();
            controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            controller.Request.SetConfiguration(new HttpConfiguration());
            controller.RequestContext.Principal =
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim("OrganizationId", "1") }));
        }

        public static void SetDbSetDataForAsync<T>(this DbSet<T> mockedDbSet, IEnumerable<T> data)
            where T : class
        {
            var dataQueryable = data.AsQueryable();

            var queryableMockSet = (IQueryable<T>)mockedDbSet;
            var dbAsyncEnumerableMockSet = (IDbAsyncEnumerable<T>)mockedDbSet;
            dbAsyncEnumerableMockSet.GetAsyncEnumerator().Returns(new MockDbAsyncEnumerator<T>(dataQueryable.GetEnumerator()));
            queryableMockSet.Provider.Returns(new MockDbAsyncQueryProvider<T>(dataQueryable.Provider));

            queryableMockSet.Expression.Returns(dataQueryable.Expression);
            queryableMockSet.ElementType.Returns(dataQueryable.ElementType);
            queryableMockSet.GetEnumerator().Returns(dataQueryable.GetEnumerator());
            queryableMockSet.AsNoTracking().Returns(mockedDbSet);

            mockedDbSet.Include(Arg.Any<string>()).Returns(mockedDbSet);
        }

        public static DbSet<T> MockDbSetForAsync<T>(this IUnitOfWork2 uow, IEnumerable<T> data = null)
            where T : class
        {
            var dbSetMock = Substitute.For<DbSet<T>, IQueryable<T>, IDbAsyncEnumerable<T>>();
            uow.GetDbSet<T>().Returns(dbSetMock);

            if (data != null)
            {
                dbSetMock.SetDbSetDataForAsync(data);
            }

            dbSetMock.Include(Arg.Any<string>()).Returns(dbSetMock);

            return dbSetMock;
        }
    }
}