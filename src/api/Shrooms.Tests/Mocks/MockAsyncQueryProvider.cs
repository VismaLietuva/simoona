using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Shrooms.Tests.Mocks
{
    public static class MockDbFunctions
    {
        public static DateTime? AddDays(DateTime? date, int? days)
        {
            return date.Value.AddDays(days.Value);
        }
    }

    public class MockDbAsyncQueryProvider<TEntity> : IDbAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal MockDbAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var convertedExpression = MockQueryExpressionDbFunctionsReplacer.Replace(expression);
            return new MockDbAsyncEnumerable<TEntity>(convertedExpression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var convertedExpression = MockQueryExpressionDbFunctionsReplacer.Replace(expression);
            return new MockDbAsyncEnumerable<TElement>(convertedExpression);
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute(expression));
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute<TResult>(expression));
        }
    }

    public static class MockQueryExpressionDbFunctionsReplacer
    {
        public static Expression Replace(Expression general)
        {
            var visitor = new ReplaceDbFunctionsVisitor();
            return visitor.Visit(general);
        }
    }

    public class ReplaceDbFunctionsVisitor : ExpressionVisitor
    {
        private static (MethodInfo, MethodInfo) GetDbFunctionsAddDaysMethodReplacement()
        {
            var methodToReplace = typeof(DbFunctions).GetMethod(nameof(DbFunctions.AddDays), new[] { typeof(DateTime?), typeof(int?) });
            var newMethod = typeof(MockDbFunctions).GetMethod(nameof(MockDbFunctions.AddDays), new[] { typeof(DateTime?), typeof(int?) });
            return (methodToReplace, newMethod);
        }

        private static List<(MethodInfo MethodToReplace, MethodInfo NewMethod)> GetReplacableMethods()
        {
            return new List<(MethodInfo MethodToReplace, MethodInfo NewMethod)>
            {
                GetDbFunctionsAddDaysMethodReplacement()
            };
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var replacableMethods = GetReplacableMethods();
            foreach (var replacableMethod in replacableMethods)
            {
                if (node.Method == replacableMethod.MethodToReplace)
                {
                    return Expression.Call(replacableMethod.NewMethod, node.Arguments);
                }
            }

            return base.VisitMethodCall(node);
        }
    }

    public class MockDbAsyncEnumerable<T> : EnumerableQuery<T>, IDbAsyncEnumerable<T>, IQueryable<T>
    {
        public MockDbAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        {
        }

        public MockDbAsyncEnumerable(Expression expression)
            : base(expression)
        {
        }

        public IDbAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return new MockDbAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumerator();
        }

        IQueryProvider IQueryable.Provider => new MockDbAsyncQueryProvider<T>(this);
    }

    public class MockDbAsyncEnumerator<T> : IDbAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public MockDbAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public void Dispose()
        {
            _inner.Dispose();
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_inner.MoveNext());
        }

        public T Current => _inner.Current;

        object IDbAsyncEnumerator.Current => Current;
    }
}
