using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using AutoMapper;
using AutoMapper.Internal;
using Mapster;
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
            cyclicConfig.Default.MaxDepth(MappingSamples.CycleGuardDepth);
            foreach (var typeMap in selfReferencingMaps)
            {
                cyclicConfig.ForType(typeMap.SourceType, typeMap.DestinationType).PreserveReference(true);
            }

            var cyclicMapper = new MapsterMapper.Mapper(cyclicConfig);

            var rows = new List<string>();
            var sharingRows = new List<string>();
            var sharedMembers = 0;
            var totals = variants.ToDictionary(v => v.Name, _ => new SortedDictionary<string, int>());
            foreach (var typeMap in typeMaps)
            {
                var source = MappingSamples.Fill(typeMap.SourceType);
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

                var sharing = SharingDifferences(source, typeMap, autoMapper, variants[1].Mapper);
                if (sharing.Count > 0)
                {
                    sharingRows.Add($"| {Name(typeMap)} | {string.Join(", ", sharing)} |");
                    sharedMembers += sharing.Count;
                }
            }

            var cyclicRows = new List<string>();
            var cyclicTotals = new SortedDictionary<string, int>();
            foreach (var typeMap in selfReferencingMaps)
            {
                var source = MappingSamples.Fill(typeMap.SourceType);
                if (source == null || MappingSamples.CloseCycles(source, typeMap.SourceType) == 0)
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
            report.AppendLine("The comparison is on JSON, so it sees values, not identity: \"same\" means equal values. " +
                              "Members where one library hands over the source instance and the other a copy are listed under Shared instances.");
            report.AppendLine();
            foreach (var variant in variants)
            {
                report.AppendLine($"- **{variant.Name}:** " + Summary(totals[variant.Name]));
            }

            report.AppendLine();
            report.AppendLine($"## Shared instances (MapsterSettings): {sharedMembers} members in {sharingRows.Count} maps");
            report.AppendLine();
            report.AppendLine("Same-type members whose reference identity differs between AutoMapper and Mapster. Harmless unless a caller " +
                              "mutates the result; review these with the phase 3 call sites.");
            report.AppendLine();
            report.AppendLine("| Map | Members |");
            report.AppendLine("|---|---|");
            sharingRows.ForEach(r => report.AppendLine(r));
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

        // Same-name, same-type reference members where only one library returns the source instance (e.g. Mapster shares a
        // List<string> AutoMapper copies). JSON cannot show this, so it is checked on the objects themselves.
        private static List<string> SharingDifferences(object source, TypeMap typeMap, IMapper autoMapper, MapsterMapper.IMapper mapster)
        {
            object autoMapperResult;
            object mapsterResult;
            try
            {
                autoMapperResult = autoMapper.Map(source, typeMap.SourceType, typeMap.DestinationType);
                mapsterResult = mapster.Map(source, typeMap.SourceType, typeMap.DestinationType);
            }
            catch (Exception)
            {
                return new List<string>();
            }

            var differences = new List<string>();
            if (autoMapperResult == null || mapsterResult == null)
            {
                return differences;
            }

            foreach (var destination in typeMap.DestinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var sourceProperty = typeMap.SourceType.GetProperty(destination.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (sourceProperty == null || sourceProperty.PropertyType != destination.PropertyType || destination.PropertyType.IsValueType
                    || destination.PropertyType == typeof(string) || destination.GetIndexParameters().Length > 0 || destination.GetMethod == null)
                {
                    continue;
                }

                try
                {
                    var sourceValue = sourceProperty.GetValue(source);
                    if (sourceValue == null)
                    {
                        continue;
                    }

                    var autoMapperShares = ReferenceEquals(sourceValue, destination.GetValue(autoMapperResult));
                    var mapsterShares = ReferenceEquals(sourceValue, destination.GetValue(mapsterResult));
                    if (autoMapperShares != mapsterShares)
                    {
                        differences.Add(mapsterShares ? $"{destination.Name} (Mapster shares, AutoMapper copies)" : $"{destination.Name} (AutoMapper shares, Mapster copies)");
                    }
                }
                catch (TargetInvocationException)
                {
                    // Computed getter that throws on sample data: nothing to compare.
                }
            }

            return differences;
        }

        private static bool TryMapWithAutoMapper(IMapper autoMapper, object source, TypeMap typeMap, out JToken expected, out List<string> noisePaths, out string cell)
        {
            var errors = new List<string>();
            try
            {
                expected = MappingSamples.MapWithAutoMapper(autoMapper, source, typeMap.SourceType, typeMap.DestinationType, errors, out noisePaths);
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
                var actual = MappingSamples.ToJson(mapper.Map(source, typeMap.SourceType, typeMap.DestinationType), errors);
                MappingSamples.Blank(actual, noisePaths);

                var differing = MappingSamples.DifferingPaths(expected, actual);
                var unexplained = customMembers.Contains("*")
                    ? new List<string>()
                    : differing.Where(p => !customMembers.Contains(TopMember(p))).ToList();

                (outcome, detail) = differing.Count == 0 ? ("same", string.Empty)
                    : unexplained.Count == 0 ? ("differs only under configured members", MappingSamples.Paths(differing))
                    : ("differs by convention", MappingSamples.Paths(unexplained));
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

        private static string TopMember(string path)
        {
            var end = path.IndexOfAny(new[] { '.', '[' });
            return end < 0 ? path : path.Substring(0, end);
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
            return errors.Count == 0 ? cell : $"{cell} (serialization errors at {MappingSamples.Paths(errors.Distinct().ToList())})";
        }

        private static string Row(TypeMap typeMap, string autoMapper, string defaults, string tuned)
        {
            return $"| {typeMap.Profile.Name.Split('.').Last()} | {Name(typeMap)} | {autoMapper} | {defaults} | {tuned} |";
        }

        private static string Name(TypeMap typeMap)
        {
            return MappingSamples.Name(typeMap.SourceType, typeMap.DestinationType);
        }
    }
}
