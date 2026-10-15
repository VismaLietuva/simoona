using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// AutoMapper and with convention-only Mapster (no registrations yet), and reports where the output differs.
    /// Differences on members a profile configures explicitly are expected; the rest show which conventions differ.
    /// Run on demand: dotnet test --filter FullyQualifiedName~MapsterParitySweep
    /// </summary>
    [TestFixture]
    [Explicit("Report, not a check: run on demand during the migration.")]
    public class MapsterParitySweep
    {
        private const int FillDepth = 3;

        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 16,
            Error = (_, args) => args.ErrorContext.Handled = true
        };

        [Test]
        public void Sweep()
        {
            var autoMapperConfig = new MapperConfiguration(cfg => cfg.AddMaps(typeof(Other).Assembly, typeof(EventsProfile).Assembly));
            var autoMapper = autoMapperConfig.CreateMapper();
            var typeMaps = autoMapperConfig.Internal().GetAllTypeMaps()
                .OrderBy(m => m.Profile.Name).ThenBy(m => m.SourceType.Name).ThenBy(m => m.DestinationType.Name)
                .ToList();

            var variants = new (string Name, Func<TypeAdapterConfig> Create)[]
            {
                ("Mapster defaults", () => new TypeAdapterConfig()),
                ("MapsterSettings", MapsterSettings.Create)
            };

            var rows = new List<string>();
            var totals = variants.ToDictionary(v => v.Name, _ => new Dictionary<string, int>());

            foreach (var typeMap in typeMaps)
            {
                var customMembers = CustomMembers(typeMap);
                var source = Fill(typeMap.SourceType, 0);
                if (source == null)
                {
                    rows.Add(Row(typeMap, "skipped: source cannot be built", string.Empty, string.Empty));
                    continue;
                }

                JToken expected;
                try
                {
                    expected = ToJson(autoMapper.Map(source, typeMap.SourceType, typeMap.DestinationType));

                    // Members that differ between two AutoMapper runs come from the destination's constructor (new Guid,
                    // DateTime.UtcNow); they say nothing about the mapper, so they are left out of every comparison.
                    var again = ToJson(autoMapper.Map(source, typeMap.SourceType, typeMap.DestinationType));
                    customMembers.UnionWith(DifferingMembers(expected, again).Where(m => m != "(value)"));
                }
                catch (Exception ex)
                {
                    rows.Add(Row(typeMap, "skipped: AutoMapper throws " + ex.GetBaseException().GetType().Name, string.Empty, string.Empty));
                    continue;
                }

                var cells = new List<string>();
                foreach (var variant in variants)
                {
                    var outcome = Compare(variant.Create(), source, typeMap, expected, customMembers, out var detail);
                    totals[variant.Name][outcome] = totals[variant.Name].GetValueOrDefault(outcome) + 1;
                    cells.Add(detail.Length == 0 ? outcome : $"{outcome}: {detail}");
                }

                rows.Add(Row(typeMap, "ok", cells[0], cells[1]));
            }

            var report = new StringBuilder();
            report.AppendLine("# AutoMapper vs Mapster parity sweep");
            report.AppendLine();
            report.AppendLine($"{typeMaps.Count} AutoMapper type maps. Mapster runs by convention only (no registrations).");
            report.AppendLine();
            foreach (var variant in variants)
            {
                report.AppendLine($"- **{variant.Name}:** " + string.Join(", ", totals[variant.Name].OrderBy(t => t.Key).Select(t => $"{t.Key} {t.Value}")));
            }

            report.AppendLine();
            report.AppendLine("Self-referencing maps AutoMapper tracks references for (need PreserveReference): " +
                              string.Join(", ", typeMaps.Where(m => m.PreserveReferences).Select(Name).DefaultIfEmpty("none")));
            report.AppendLine();
            report.AppendLine("| Profile | Map | AutoMapper | Mapster defaults | MapsterSettings |");
            report.AppendLine("|---|---|---|---|---|");
            rows.ForEach(r => report.AppendLine(r));

            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "mapster-parity-sweep.md");
            File.WriteAllText(path, report.ToString());
            TestContext.Out.WriteLine(report.ToString());
            TestContext.Out.WriteLine("Report written to " + path);
        }

        private static string Compare(TypeAdapterConfig config, object source, TypeMap typeMap, JToken expected, ISet<string> customMembers, out string detail)
        {
            JToken actual;
            try
            {
                actual = ToJson(new MapsterMapper.Mapper(config).Map(source, typeMap.SourceType, typeMap.DestinationType));
            }
            catch (Exception ex)
            {
                detail = ex.GetBaseException().GetType().Name;
                return customMembers.Contains("*") ? "throws on a custom map" : "throws";
            }

            if (JToken.DeepEquals(expected, actual))
            {
                detail = string.Empty;
                return "same";
            }

            var differing = DifferingMembers(expected, actual);
            var unexplained = differing.Where(m => !customMembers.Contains(m)).ToList();
            if (unexplained.Count == 0 || customMembers.Contains("*"))
            {
                detail = string.Join(" ", differing);
                return "differs only on configured members";
            }

            detail = string.Join(" ", unexplained);
            return "differs by convention";
        }

        // Destination members a profile sets up by hand (MapFrom, Ignore, resolvers); "*" when the whole map is custom
        // (ConvertUsing, ConstructUsing).
        private static ISet<string> CustomMembers(TypeMap typeMap)
        {
            var members = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (typeMap.TypeConverter != null || typeMap.CustomMapExpression != null || typeMap.CustomCtorFunction != null)
            {
                members.Add("*");
            }

            foreach (var propertyMap in typeMap.PropertyMaps)
            {
                if (propertyMap.Ignored || propertyMap.IsResolveConfigured)
                {
                    members.Add(propertyMap.DestinationName);
                }
            }

            return members;
        }

        private static List<string> DifferingMembers(JToken expected, JToken actual)
        {
            if (expected is JObject left && actual is JObject right)
            {
                return left.Properties().Select(p => p.Name)
                    .Union(right.Properties().Select(p => p.Name))
                    .Where(name => !JToken.DeepEquals(left[name], right[name]))
                    .ToList();
            }

            return new List<string> { "(value)" };
        }

        private static JToken ToJson(object value)
        {
            return value == null ? JValue.CreateNull() : JToken.Parse(JsonConvert.SerializeObject(value, JsonSettings));
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
            var elementType = type.IsArray
                ? type.GetElementType()
                : type.GetInterfaces().Append(type)
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    ?.GetGenericArguments()[0];

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
