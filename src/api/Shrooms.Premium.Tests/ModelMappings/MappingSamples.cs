using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Shrooms.Premium.Tests.ModelMappings
{
    /// <summary>
    /// Sample data and JSON comparison shared by the AutoMapper to Mapster parity check and the parity sweep.
    /// </summary>
    public static class MappingSamples
    {
        private const int FillDepth = 3;

        // Deterministic sample: each member gets a value derived from its name, so a value landing on the wrong member shows.
        public static object Fill(Type type)
        {
            return Fill(type, 0, "root");
        }

        /// <summary>
        /// Maps the source with AutoMapper twice and returns the first result as JSON, with the paths that differ between
        /// the two runs blanked. Those come from the destination's constructor (new Guid, DateTime.UtcNow); they say nothing
        /// about the mapper, so the caller blanks them on the Mapster side too. Serialization errors from both runs go to
        /// errors.
        /// </summary>
        public static JToken MapWithAutoMapper(IMapper autoMapper, object source, Type sourceType, Type destinationType, List<string> errors, out List<string> noisePaths)
        {
            var expected = ToJson(autoMapper.Map(source, sourceType, destinationType), errors);
            var again = ToJson(autoMapper.Map(source, sourceType, destinationType), errors);
            noisePaths = DifferingPaths(expected, again).Where(p => p != "(value)").ToList();
            Blank(expected, noisePaths);
            return expected;
        }

        // A member whose getter throws is left out of the JSON and its path added to errors.
        public static JToken ToJson(object value, List<string> errors)
        {
            if (value == null)
            {
                return JValue.CreateNull();
            }

            var serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Error = (_, args) =>
                {
                    // Path is the parent's ("" at the root); Member names the property whose getter threw.
                    var context = args.ErrorContext;
                    errors.Add(context.Member is string member && !context.Path.EndsWith(member, StringComparison.Ordinal)
                        ? (context.Path.Length == 0 ? member : $"{context.Path}.{member}")
                        : context.Path);
                    context.Handled = true;
                }
            });

            return JToken.FromObject(value, serializer);
        }

        // Leaf paths ("Projects[0].Name") where the two trees differ; "(value)" when the roots themselves differ. A property
        // present on one side only differs even when the other side holds null: the JSON leaves out a preserved
        // back-reference, so an explicit null in its place means the cycle was lost.
        public static List<string> DifferingPaths(JToken expected, JToken actual)
        {
            return DifferingPaths(expected, actual, string.Empty);
        }

        public static void Blank(JToken token, IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                token.SelectToken(path)?.Replace(JValue.CreateNull());
            }
        }

        // Points every member that can hold the root type (directly or as a list element) back at the root, turning the
        // filled tree into the cyclic graph EF hands over (Room -> Floor -> Rooms -> Room). Returns how many links it made.
        public static int CloseCycles(object root, Type rootType)
        {
            var links = 0;
            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            var pending = new Stack<object>();
            pending.Push(root);

            while (pending.Count > 0)
            {
                var current = pending.Pop();
                if (!visited.Add(current))
                {
                    continue;
                }

                foreach (var property in current.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (property.GetIndexParameters().Length > 0 || property.GetMethod == null
                        || property.PropertyType == typeof(string) || property.PropertyType == typeof(object))
                    {
                        continue;
                    }

                    object value;
                    try
                    {
                        value = property.GetValue(current);
                    }
                    catch (TargetInvocationException)
                    {
                        // Computed getters can throw on sample data (Event.LocalStartDate parses a time zone id): not a link.
                        continue;
                    }

                    if (property.PropertyType.IsAssignableFrom(rootType) && property.SetMethod?.IsPublic == true)
                    {
                        if (!ReferenceEquals(value, root))
                        {
                            property.SetValue(current, root);
                            links++;
                        }

                        continue;
                    }

                    if (value is IList list && !list.IsFixedSize && !list.IsReadOnly && ElementType(property.PropertyType)?.IsAssignableFrom(rootType) == true)
                    {
                        list.Add(root);
                        links++;
                        continue;
                    }

                    if (value is IEnumerable items and not string)
                    {
                        foreach (var item in items)
                        {
                            if (item != null && !item.GetType().IsValueType && item is not string)
                            {
                                pending.Push(item);
                            }
                        }
                    }
                    else if (value != null && !property.PropertyType.IsValueType)
                    {
                        pending.Push(value);
                    }
                }
            }

            return links;
        }

        public static string Name(Type sourceType, Type destinationType)
        {
            return $"{TypeName(sourceType)} → {TypeName(destinationType)}";
        }

        private static string TypeName(Type type)
        {
            return type.IsGenericType
                ? $"{type.Name.Split('`')[0]}<{string.Join(", ", type.GetGenericArguments().Select(TypeName))}>"
                : type.Name;
        }

        private static List<string> DifferingPaths(JToken expected, JToken actual, string path)
        {
            if (expected is JObject left && actual is JObject right)
            {
                return left.Properties().Select(p => p.Name)
                    .Union(right.Properties().Select(p => p.Name))
                    .SelectMany(name =>
                    {
                        var memberPath = path.Length == 0 ? name : $"{path}.{name}";
                        return left.ContainsKey(name) && right.ContainsKey(name)
                            ? DifferingPaths(left[name], right[name], memberPath)
                            : new List<string> { memberPath };
                    })
                    .ToList();
            }

            if (expected is JArray leftArray && actual is JArray rightArray && leftArray.Count == rightArray.Count)
            {
                return leftArray.SelectMany((item, i) => DifferingPaths(item, rightArray[i], $"{path}[{i}]")).ToList();
            }

            if (JToken.DeepEquals(expected, actual))
            {
                return new List<string>();
            }

            return new List<string> { path.Length == 0 ? "(value)" : path };
        }

        private static object Fill(Type type, int depth, string seed)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            var number = Math.Abs(StableHash(seed) % 1000) + 1;

            if (underlying == typeof(string))
            {
                return "s-" + seed;
            }

            if (underlying.IsEnum)
            {
                var values = Enum.GetValues(underlying);
                return values.Length > 1 ? values.GetValue(1) : values.Length == 1 ? values.GetValue(0) : Activator.CreateInstance(underlying);
            }

            if (underlying == typeof(bool))
            {
                return true;
            }

            if (underlying == typeof(DateTime))
            {
                return new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(number);
            }

            if (underlying == typeof(DateTimeOffset))
            {
                return new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).AddHours(number);
            }

            if (underlying == typeof(TimeSpan))
            {
                return TimeSpan.FromMinutes(number);
            }

            if (underlying == typeof(Guid))
            {
                return new Guid(number, 0, 0, new byte[8]);
            }

            if (underlying == typeof(byte[]))
            {
                return new byte[] { 1, 2, 3 };
            }

            if (underlying.IsPrimitive || underlying == typeof(decimal))
            {
                return Convert.ChangeType(number % 100, underlying);
            }

            if (depth >= FillDepth || underlying.IsAbstract && !IsCollection(underlying) || typeof(Delegate).IsAssignableFrom(underlying)
                || typeof(Stream).IsAssignableFrom(underlying) || underlying == typeof(Type) || underlying == typeof(object))
            {
                return null;
            }

            if (IsCollection(underlying))
            {
                return FillCollection(underlying, depth, seed);
            }

            object instance;
            try
            {
                instance = Activator.CreateInstance(underlying, nonPublic: true);
            }
            catch
            {
                return null;
            }

            foreach (var property in underlying.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.GetIndexParameters().Length == 0 && property.SetMethod?.IsPublic == true)
                {
                    TrySet(() => property.SetValue(instance, Fill(property.PropertyType, depth + 1, property.Name)));
                }
            }

            foreach (var field in underlying.GetFields(BindingFlags.Public | BindingFlags.Instance).Where(f => !f.IsInitOnly))
            {
                TrySet(() => field.SetValue(instance, Fill(field.FieldType, depth + 1, field.Name)));
            }

            return instance;
        }

        private static object FillCollection(Type type, int depth, string seed)
        {
            var elementType = type.IsArray ? type.GetElementType() : ElementType(type);

            if (elementType == null || IsDictionary(type))
            {
                return null;
            }

            var element = Fill(elementType, depth + 1, seed + "-item");
            if (type.IsArray)
            {
                var array = Array.CreateInstance(elementType, element == null ? 0 : 1);
                if (element != null)
                {
                    array.SetValue(element, 0);
                }

                return array;
            }

            var concrete = type.IsInterface ? typeof(List<>).MakeGenericType(elementType) : type;
            object collection;
            try
            {
                collection = Activator.CreateInstance(concrete, nonPublic: true);
            }
            catch
            {
                return null;
            }

            var add = concrete.GetMethod("Add", new[] { elementType });
            if (element != null && add != null)
            {
                TrySet(() => add.Invoke(collection, new[] { element }));
            }

            return collection;
        }

        private static Type ElementType(Type type)
        {
            return type.GetInterfaces().Append(type)
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                ?.GetGenericArguments()[0];
        }

        private static bool IsCollection(Type type)
        {
            return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
        }

        private static bool IsDictionary(Type type)
        {
            return type.GetInterfaces().Append(type).Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        private static void TrySet(Action set)
        {
            try
            {
                set();
            }
            catch
            {
                // A member whose setter validates or throws keeps its default; the samples only need most members filled.
            }
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                return value.Aggregate(17, (hash, c) => hash * 31 + c);
            }
        }
    }
}
