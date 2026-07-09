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
            {
                return (HttpStatusCode)statusCodeResult.StatusCode.Value;
            }

            return HttpStatusCode.OK;
        }

        public static T GetContent<T>(this IActionResult result)
        {
            if (result is ObjectResult objectResult)
            {
                return (T)objectResult.Value;
            }

            return default;
        }

        public static void Validate(this ControllerBase controller, object model)
        {
            if (model == null)
            {
                return;
            }

            ValidateRecursive(controller, model, string.Empty);
        }

        private static void ValidateRecursive(ControllerBase controller, object model, string prefix)
        {
            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            foreach (var validationResult in results)
            {
                var memberNames = validationResult.MemberNames.ToList();
                if (!memberNames.Any())
                {
                    memberNames.Add(string.Empty);
                }

                foreach (var member in memberNames)
                {
                    var key = string.IsNullOrEmpty(prefix) ? member : $"{prefix}.{member}";
                    controller.ModelState.AddModelError(key, validationResult.ErrorMessage);
                }
            }

            foreach (var prop in model.GetType().GetProperties())
            {
                var value = prop.GetValue(model);
                if (value is System.Collections.IEnumerable enumerable && value is not string)
                {
                    var i = 0;
                    foreach (var item in enumerable)
                    {
                        if (item != null && item.GetType().IsClass)
                        {
                            var itemPrefix = string.IsNullOrEmpty(prefix) ? $"{prop.Name}[{i}]" : $"{prefix}.{prop.Name}[{i}]";
                            ValidateRecursive(controller, item, itemPrefix);
                        }
                        i++;
                    }
                }
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
        }

        public static DbSet<T> MockDbSetForAsync<T>(this IUnitOfWork2 uow, IEnumerable<T> data = null)
            where T : class
        {
            var dbSetMock = Substitute.For<DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>>();
            uow.GetDbSet<T>().Returns(dbSetMock);

            dbSetMock.SetDbSetDataForAsync(data ?? Enumerable.Empty<T>());

            return dbSetMock;
        }
    }
}
