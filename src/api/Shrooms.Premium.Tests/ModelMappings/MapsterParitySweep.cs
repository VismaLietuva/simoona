using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using AutoMapper;
using AutoMapper.Internal;
using Mapster;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Shrooms.Premium.Presentation.ModelMappings.Profiles;
using Shrooms.Presentation.ModelMappings.MapsterSetup;
using Shrooms.Presentation.ModelMappings.Profiles;

namespace Shrooms.Premium.Tests.ModelMappings
{
    /// <summary>
    /// Phase 0 spike for the AutoMapper to Mapster migration: maps a filled sample of every AutoMapper type map with
    /// AutoMapper and with convention-only Mapster (no registrations yet), and reports where the output differs, down to
    /// nested paths. Differences under members a profile configures explicitly are expected; the rest show which
    /// conventions differ. Self-referencing maps are also run on cyclic samples.
    /// Run on demand: dotnet test --filter FullyQualifiedName~MapsterParitySweep
    /// </summary>
    [TestFixture]
    [Explicit("Report, not a check: run on demand during the migration.")]
    public class MapsterParitySweep
    {
        private const int FillDepth = 3;

        // Backstop for the cyclic pass: a cycle Mapster does not track shows as a difference instead of a stack overflow.
        // Kept small on purpose: an untracked cycle with several back-links grows exponentially with depth.
        private const int CycleGuardDepth = 8;

        private const int MaxPathsShown = 6;

        [Test]
        public void Sweep()
        {
            var autoMapperConfig = new MapperConfiguration(cfg => cfg.AddMaps(typeof(Other).Assembly, typeof(EventsProfile).Assembly));
            var autoMapper = autoMapperConfig.CreateMapper();
            var typeMaps = autoMapperConfig.Internal().GetAllTypeMaps()
                .OrderBy(m => m.Profile.Name).ThenBy(m => m.SourceType.Name).ThenBy(m => m.DestinationType.Name)
                .ToList();
            var selfReferencingMaps = typeMaps.Where(m => m.PreserveReferences).ToList();

            // One mapper per variant for the whole run: Mapster compiles each pair once and reuses nested mappings.
            var variants = new (string Name, MapsterMapper.IMapper Mapper)[]
            {
                ("Mapster defaults", new MapsterMapper.Mapper(new TypeAdapterConfig())),
                ("MapsterSettings", new MapsterMapper.Mapper(MapsterSettings.Create()))
            };

            // Reference tracking on the self-referencing maps, as the translated profiles will set it.
            var cyclicConfig = MapsterSettings.Create();
            cyclicConfig.Default.MaxDepth(CycleGuardDepth);
            foreach (var typeMap in selfReferencingMaps)
            {
                cyclicConfig.ForType(typeMap.SourceType, typeMap.DestinationType).PreserveReference(true);
            }

            var cyclicMapper = new MapsterMapper.Mapper(cyclicConfig);

            var rows = new List<string>();
            var totals = variants.ToDictionary(v => v.Name, _ => new SortedDictionary<string, int>());
            foreach (var typeMap in typeMaps)
            {
                var source = Fill(typeMap.SourceType, 0);
                if (source == null)
                {
                    rows.Add(Row(typeMap, "skipped: source cannot be built", string.Empty, string.Empty));
                    continue;
                }

                if (!TryMapWithAutoMapper(autoMapper, source, typeMap, out var expected, out var noisePaths, out var autoMapperCell))
                {
                    rows.Add(Row(typeMap, autoMapperCell, string.Empty, string.Empty));
                    continue;
                }

                var customMembers = CustomMembers(typeMap);
                var cells = variants.Select(v => Compare(v.Mapper, source, typeMap, expected, noisePaths, customMembers, totals[v.Name])).ToList();
                rows.Add(Row(typeMap, autoMapperCell, cells[0], cells[1]));
            }

            var cyclicRows = new List<string>();
            var cyclicTotals = new SortedDictionary<string, int>();
            foreach (var typeMap in selfReferencingMaps)
            {
                TestContext.Progress.WriteLine("cyclic: " + Name(typeMap));
                var source = Fill(typeMap.SourceType, 0);
                if (source == null || CloseCycles(source, typeMap.SourceType) == 0)
                {
                    cyclicRows.Add($"| {Name(typeMap)} | skipped: no member to close a cycle on | |");
                    continue;
                }

                if (!TryMapWithAutoMapper(autoMapper, source, typeMap, out var expected, out var noisePaths, out var autoMapperCell))
                {
                    cyclicRows.Add($"| {Name(typeMap)} | {autoMapperCell} | |");
                    continue;
                }

                var cell = Compare(cyclicMapper, source, typeMap, expected, noisePaths, CustomMembers(typeMap), cyclicTotals);
                cyclicRows.Add($"| {Name(typeMap)} | {autoMapperCell} | {cell} |");
            }

            var report = new StringBuilder();
            report.AppendLine("# AutoMapper vs Mapster parity sweep");
            report.AppendLine();
            report.AppendLine($"{typeMaps.Count} AutoMapper type maps. Mapster runs by convention only (no registrations). " +
                              "Paths that differ between two AutoMapper runs (constructor GUIDs and timestamps) are left out.");
            report.AppendLine();
            foreach (var variant in variants)
            {
                report.AppendLine($"- **{variant.Name}:** " + Summary(totals[variant.Name]));
            }

            report.AppendLine();
            report.AppendLine($"## Cyclic samples ({selfReferencingMaps.Count} self-referencing maps, MapsterSettings + PreserveReference)");
            report.AppendLine();
            report.AppendLine("- " + Summary(cyclicTotals));
            report.AppendLine();
            report.AppendLine("| Map | AutoMapper | Mapster |");
            report.AppendLine("|---|---|---|");
            cyclicRows.ForEach(r => report.AppendLine(r));
            report.AppendLine();
            report.AppendLine("## Every map");
            report.AppendLine();
            report.AppendLine("| Profile | Map | AutoMapper | Mapster defaults | MapsterSettings |");
            report.AppendLine("|---|---|---|---|---|");
            rows.ForEach(r => report.AppendLine(r));

            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "mapster-parity-sweep.md");
            File.WriteAllText(path, report.ToString());
            TestContext.Out.WriteLine(report.ToString());
            TestContext.Out.WriteLine("Report written to " + path);
        }

        private static bool TryMapWithAutoMapper(IMapper autoMapper, object source, TypeMap typeMap, out JToken expected, out List<string> noisePaths, out string cell)
        {
            var errors = new List<string>();
            try
            {
                expected = ToJson(autoMapper.Map(source, typeMap.SourceType, typeMap.DestinationType), errors);

                // Paths that differ between two AutoMapper runs come from the destination's constructor (new Guid,
                // DateTime.UtcNow); they say nothing about the mapper, so both sides blank them before comparing.
                var again = ToJson(autoMapper.Map(source, typeMap.SourceType, typeMap.DestinationType), new List<string>());
                noisePaths = DifferingPaths(expected, again, string.Empty).Where(p => p != "(value)").ToList();
                Blank(expected, noisePaths);
            }
            catch (Exception ex)
            {
                expected = null;
                noisePaths = null;
                cell = "skipped: AutoMapper throws " + ex.GetBaseException().GetType().Name;
                return false;
            }

            cell = WithSerializationErrors("ok", errors);
            return true;
        }

        private static string Compare(MapsterMapper.IMapper mapper, object source, TypeMap typeMap, JToken expected, List<string> noisePaths, ISet<string> customMembers, IDictionary<string, int> totals)
        {
            var errors = new List<string>();
            string outcome;
            string detail;
            try
            {
                var actual = ToJson(mapper.Map(source, typeMap.SourceType, typeMap.DestinationType), errors);
                Blank(actual, noisePaths);

                var differing = DifferingPaths(expected, actual, string.Empty);
                var unexplained = customMembers.Contains("*")
                    ? new List<string>()
                    : differing.Where(p => !customMembers.Contains(TopMember(p))).ToList();

                (outcome, detail) = differing.Count == 0 ? ("same", string.Empty)
                    : unexplained.Count == 0 ? ("differs only under configured members", Shorten(differing))
                    : ("differs by convention", Shorten(unexplained));
            }
            catch (Exception ex)
            {
                outcome = customMembers.Contains("*") ? "throws on a custom map" : "throws";
                detail = ex.GetBaseException().GetType().Name;
            }

            Count(totals, outcome);
            if (errors.Count > 0)
            {
                Count(totals, "with serialization errors");
            }

            return WithSerializationErrors(detail.Length == 0 ? outcome : $"{outcome}: {detail}", errors);
        }

        // Destination members a profile sets up by hand (Ignore, MapFrom other than the same-named source member,
        // resolvers); "*" when the whole map is custom (ConvertUsing, ConstructUsing).
        private static ISet<string> CustomMembers(TypeMap typeMap)
        {
            var members = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (typeMap.TypeConverter != null || typeMap.CustomMapExpression != null || typeMap.CustomCtorFunction != null)
            {
                members.Add("*");
            }

            foreach (var propertyMap in typeMap.PropertyMaps)
            {
                if (propertyMap.Ignored || (propertyMap.IsResolveConfigured && !IsSameNameMapFrom(propertyMap)))
                {
                    members.Add(propertyMap.DestinationName);
                }
            }

            return members;
        }

        // MapFrom(src => src.Projects) onto Projects is what the convention does anyway: differences under it are not explained.
        private static bool IsSameNameMapFrom(PropertyMap propertyMap)
        {
            var body = propertyMap.CustomMapExpression?.Body;
            while (body is UnaryExpression { NodeType: ExpressionType.Convert } convert)
            {
                body = convert.Operand;
            }

            return body is MemberExpression { Expression: ParameterExpression } member
                   && string.Equals(member.Member.Name, propertyMap.DestinationName, StringComparison.OrdinalIgnoreCase);
        }

        // Leaf paths ("Projects[0].Name") where the two trees differ; "(value)" when the roots themselves differ.
        private static List<string> DifferingPaths(JToken expected, JToken actual, string path)
        {
            if (expected is JObject left && actual is JObject right)
            {
                return left.Properties().Select(p => p.Name)
                    .Union(right.Properties().Select(p => p.Name))
                    .SelectMany(name => DifferingPaths(
                        left[name] ?? JValue.CreateNull(),
                        right[name] ?? JValue.CreateNull(),
                        path.Length == 0 ? name : $"{path}.{name}"))
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

        private static void Blank(JToken token, IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                token.SelectToken(path)?.Replace(JValue.CreateNull());
            }
        }

        private static string TopMember(string path)
        {
            var end = path.IndexOfAny(new[] { '.', '[' });
            return end < 0 ? path : path.Substring(0, end);
        }

        private static string Shorten(List<string> paths)
        {
            var shown = string.Join(" ", paths.Take(MaxPathsShown));
            return paths.Count > MaxPathsShown ? $"{shown} (+{paths.Count - MaxPathsShown} more)" : shown;
        }

        private static void Count(IDictionary<string, int> totals, string key)
        {
            totals[key] = totals.TryGetValue(key, out var count) ? count + 1 : 1;
        }

        private static string Summary(IDictionary<string, int> totals)
        {
            return totals.Count == 0 ? "nothing compared" : string.Join(", ", totals.Select(t => $"{t.Key} {t.Value}"));
        }

        // A member whose getter throws is missing from the JSON on that side; say so rather than compare silently.
        private static string WithSerializationErrors(string cell, List<string> errors)
        {
            return errors.Count == 0 ? cell : $"{cell} (serialization errors at {string.Join(", ", errors.Distinct().Take(MaxPathsShown))})";
        }

        private static JToken ToJson(object value, List<string> errors)
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
                    errors.Add(args.ErrorContext.Path);
                    args.ErrorContext.Handled = true;
                }
            });

            return JToken.FromObject(value, serializer);
        }

        // Points every member that can hold the root type (directly or as a list element) back at the root, turning the
        // filled tree into the cyclic graph EF hands over (Room -> Floor -> Rooms -> Room). Returns how many links it made.
        private static int CloseCycles(object root, Type rootType)
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

        private static Type ElementType(Type type)
        {
            return type.GetInterfaces().Append(type)
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                ?.GetGenericArguments()[0];
        }

        private static string Row(TypeMap typeMap, string autoMapper, string defaults, string tuned)
        {
            return $"| {typeMap.Profile.Name.Split('.').Last()} | {Name(typeMap)} | {autoMapper} | {defaults} | {tuned} |";
        }

        private static string Name(TypeMap typeMap)
        {
            return $"{TypeName(typeMap.SourceType)} → {TypeName(typeMap.DestinationType)}";
        }

        private static string TypeName(Type type)
        {
            return type.IsGenericType
                ? $"{type.Name.Split('`')[0]}<{string.Join(", ", type.GetGenericArguments().Select(TypeName))}>"
                : type.Name;
        }

        // Deterministic sample: each member gets a value derived from its name, so a value landing on the wrong member shows.
        private static object Fill(Type type, int depth, string seed = "root")
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
                // A member whose setter validates or throws keeps its default; the sweep only needs most members filled.
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
