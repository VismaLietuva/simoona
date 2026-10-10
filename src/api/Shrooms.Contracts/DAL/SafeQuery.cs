using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Shrooms.Contracts.DAL
{
    /// <summary>Fixed-grammar replacement for Dynamic LINQ sort/include strings: "Prop[.Nested] [asc|desc]" resolved by reflection; unparsable input is ignored, never thrown.</summary>
    public static class SafeQuery
    {
        private const int MaxClauses = 5;
        private const int MaxPathDepth = 4;

        // Members that must never be usable as a sort key or include path, whichever entity exposes them.
        private static readonly string[] BlockedFragments = { "password", "securitystamp", "concurrencystamp", "token", "secret", "authorizationguid", "phonenumber" };

        /// <param name="isQueryable">Extra filter, e.g. the EF model, for CLR properties that are not mapped.</param>
        public static IQueryable<T> OrderBy<T>(IQueryable<T> query, string orderBy, Func<PropertyInfo, bool> isQueryable = null)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            IOrderedQueryable<T> ordered = null;

            foreach (var (path, descending, countOfCollection) in ParseOrderBy(typeof(T), orderBy, isQueryable))
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                Expression body = parameter;
                foreach (var property in path)
                {
                    body = Expression.Property(body, property);
                }

                if (countOfCollection)
                {
                    body = Expression.Call(typeof(Enumerable), nameof(Enumerable.Count), new[] { ElementType(body.Type) }, body);
                }

                var lambda = Expression.Lambda(body, parameter);
                var method = ordered == null
                    ? (descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy))
                    : (descending ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy));

                var call = Expression.Call(
                    typeof(Queryable),
                    method,
                    new[] { typeof(T), body.Type },
                    ordered?.Expression ?? query.Expression,
                    Expression.Quote(lambda));

                ordered = (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(call);
            }

            return ordered ?? query;
        }

        /// <summary>Include paths that name real navigation properties of <typeparamref name="T"/>, canonically cased.</summary>
        public static IReadOnlyList<string> ValidIncludePaths<T>(string includeProperties, Func<PropertyInfo, bool> isQueryable = null)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(includeProperties))
            {
                return result;
            }

            foreach (var rawPath in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var resolved = ResolvePath(typeof(T), rawPath, requireNavigationLeaf: true, isQueryable, allowIntermediateCollections: true);
                if (resolved != null)
                {
                    result.Add(string.Join(".", resolved.Select(p => p.Name)));
                }
            }

            return result;
        }

        /// <summary>True when <paramref name="orderBy"/> contains at least one usable clause for <typeparamref name="T"/>.</summary>
        public static bool IsValidOrderBy<T>(string orderBy, Func<PropertyInfo, bool> isQueryable = null)
        {
            return ParseOrderBy(typeof(T), orderBy, isQueryable).Any();
        }

        private static IEnumerable<(PropertyInfo[] Path, bool Descending, bool CountOfCollection)> ParseOrderBy(Type elementType, string orderBy, Func<PropertyInfo, bool> isQueryable)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                yield break;
            }

            var clauses = orderBy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var emitted = 0;

            foreach (var clause in clauses)
            {
                if (emitted >= MaxClauses)
                {
                    yield break;
                }

                var parts = clause.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 0 || parts.Length > 2)
                {
                    continue;
                }

                var descending = false;
                if (parts.Length == 2)
                {
                    if (parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase) || parts[1].Equals("descending", StringComparison.OrdinalIgnoreCase))
                    {
                        descending = true;
                    }
                    else if (!parts[1].Equals("asc", StringComparison.OrdinalIgnoreCase) && !parts[1].Equals("ascending", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                // "Skills.Count()" / "Skills.Count": the only computed key allowed, the row count of a collection.
                var key = parts[0];
                var countOfCollection = false;
                if (key.EndsWith(".Count()", StringComparison.OrdinalIgnoreCase) || key.EndsWith(".Count", StringComparison.OrdinalIgnoreCase))
                {
                    key = key.Substring(0, key.LastIndexOf('.'));
                    countOfCollection = true;
                }

                var path = ResolvePath(elementType, key, requireNavigationLeaf: countOfCollection, isQueryable);
                if (path == null)
                {
                    continue;
                }

                if (countOfCollection ? !IsCollectionType(path[^1].PropertyType) || ElementType(path[^1].PropertyType) == null : !IsSortableType(path[^1].PropertyType))
                {
                    continue;
                }

                emitted++;
                yield return (path, descending, countOfCollection);
            }
        }

        private static PropertyInfo[] ResolvePath(Type type, string path, bool requireNavigationLeaf, Func<PropertyInfo, bool> isQueryable, bool allowIntermediateCollections = false)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var segments = path.Split('.');
            if (segments.Length > MaxPathDepth)
            {
                return null;
            }

            var current = type;
            var resolved = new PropertyInfo[segments.Length];

            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                if (!IsIdentifier(segment) || IsBlocked(segment))
                {
                    return null;
                }

                var property = current.GetProperty(segment, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property == null || property.GetIndexParameters().Length > 0)
                {
                    return null;
                }

                // Unmapped CLR properties would fail inside the query provider.
                if (property.IsDefined(typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute), inherit: true)
                    || (isQueryable != null && !isQueryable(property)))
                {
                    return null;
                }

                resolved[i] = property;
                var isLeaf = i == segments.Length - 1;
                var isNavigation = IsNavigationType(property.PropertyType);

                if (!isLeaf)
                {
                    if (!isNavigation)
                    {
                        return null;
                    }

                    var isCollection = IsCollectionType(property.PropertyType);

                    // Sort keys never pass through a collection; include paths may (Floors.Rooms.ApplicationUsers).
                    if (isCollection && !allowIntermediateCollections)
                    {
                        return null;
                    }

                    current = isCollection ? ElementType(property.PropertyType) : property.PropertyType;
                    if (current == null)
                    {
                        return null;
                    }
                }
                else if (requireNavigationLeaf && !isNavigation)
                {
                    return null;
                }
            }

            return resolved;
        }

        private static bool IsIdentifier(string segment)
        {
            if (segment.Length == 0 || segment.Length > 64 || !(char.IsLetter(segment[0]) || segment[0] == '_'))
            {
                return false;
            }

            return segment.All(c => char.IsLetterOrDigit(c) || c == '_');
        }

        private static bool IsBlocked(string segment)
        {
            return BlockedFragments.Any(f => segment.Contains(f, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsSortableType(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            return underlying.IsPrimitive
                || underlying.IsEnum
                || underlying == typeof(string)
                || underlying == typeof(decimal)
                || underlying == typeof(DateTime)
                || underlying == typeof(DateTimeOffset)
                || underlying == typeof(TimeSpan)
                || underlying == typeof(Guid);
        }

        private static bool IsNavigationType(Type type)
        {
            if (type == typeof(string) || type.IsValueType || type == typeof(byte[]))
            {
                return false;
            }

            return type.IsClass || type.IsInterface;
        }

        private static Type ElementType(Type collectionType)
        {
            var enumerable = collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                ? collectionType
                : collectionType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            var element = enumerable?.GetGenericArguments()[0];
            return element != null && IsNavigationType(element) ? element : null;
        }

        private static bool IsCollectionType(Type type)
        {
            return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
        }
    }
}
