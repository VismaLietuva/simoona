using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shrooms.Contracts.DAL;
using Shrooms.Tests.Mocks;

namespace Shrooms.Tests.Extensions
{
    public static class MockingExtensions
    {
        public static void SetUpControllerForTesting(this ControllerBase controller)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim("OrganizationId", "1") }));
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        public static HttpStatusCode GetStatusCode(this IActionResult result)
        {
            if (result is IStatusCodeActionResult statusCodeResult && statusCodeResult.StatusCode.HasValue)
                return (HttpStatusCode)statusCodeResult.StatusCode.Value;
            return HttpStatusCode.OK;
        }

        public static T GetContent<T>(this IActionResult result)
        {
            if (result is ObjectResult objectResult)
                return (T)objectResult.Value;
            return default;
        }

        public static void Validate(this ControllerBase controller, object model)
        {
            if (model == null)
            {
                controller.ModelState.AddModelError("model", "Value cannot be null.");
                return;
            }
            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            foreach (var validationResult in results)
            {
                var memberNames = validationResult.MemberNames.ToList();
                if (!memberNames.Any())
                    memberNames.Add(string.Empty);
                foreach (var member in memberNames)
                    controller.ModelState.AddModelError(member, validationResult.ErrorMessage);
            }
        }

        public static void SetDbSetDataForAsync<T>(this DbSet<T> mockedDbSet, IEnumerable<T> data)
            where T : class
        {
            var dataQueryable = data.AsQueryable();

            var queryableMockSet = (IQueryable<T>)mockedDbSet;
            var asyncEnumerableMockSet = (IAsyncEnumerable<T>)mockedDbSet;
            asyncEnumerableMockSet.GetAsyncEnumerator(Arg.Any<CancellationToken>()).Returns(new MockAsyncEnumerator<T>(dataQueryable.GetEnumerator()));
            queryableMockSet.Provider.Returns(new MockAsyncQueryProvider<T>(dataQueryable.Provider));

            queryableMockSet.Expression.Returns(dataQueryable.Expression);
            queryableMockSet.ElementType.Returns(dataQueryable.ElementType);
            queryableMockSet.GetEnumerator().Returns(dataQueryable.GetEnumerator());
            queryableMockSet.AsNoTracking().Returns(mockedDbSet);

            mockedDbSet.Include(Arg.Any<string>()).Returns(mockedDbSet);
        }

        public static DbSet<T> MockDbSetForAsync<T>(this IUnitOfWork2 uow, IEnumerable<T> data = null)
            where T : class
        {
            var dbSetMock = Substitute.For<DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>>();
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
