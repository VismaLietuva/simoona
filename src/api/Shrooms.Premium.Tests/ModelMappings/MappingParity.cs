using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.Internal;
using Mapster;
using Newtonsoft.Json.Linq;

namespace Shrooms.Premium.Tests.ModelMappings
{
    /// <summary>
    /// Checks every AutoMapper map against what the Mapster config does for the same pair, translated or by convention.
    /// Each map runs on several samples: a filled one, an empty one (the null branches), any hand-written ones, and for
    /// self-referencing maps a cyclic one. Each non-cyclic sample is mapped onto a new destination and onto a populated
    /// existing one, as updates of tracked entities do. Both libraries must give the same JSON every time.
    /// </summary>
    public static class MappingParity
    {
        private enum Outcome
        {
            Same,
            Differs,
            AutoMapperFails
        }

        public static ParityResult Check(MapperConfiguration autoMapperConfig, TypeAdapterConfig mapsterConfig, IReadOnlyDictionary<(Type Source, Type Destination), Func<object>[]> samples)
        {
            var typeMaps = autoMapperConfig.Internal().GetAllTypeMaps().OrderBy(m => MappingSamples.Name(m.SourceType, m.DestinationType)).ToList();

            // Maps are reported and listed by short type names, so two maps must never share one.
            var clash = typeMaps.GroupBy(m => MappingSamples.Name(m.SourceType, m.DestinationType)).FirstOrDefault(g => g.Count() > 1);
            if (clash != null)
            {
                throw new InvalidOperationException($"Two AutoMapper maps share the name {clash.Key}; make MappingSamples.Name tell them apart.");
            }

            // Read before anything maps, so it holds the registered pairs only.
            var rules = mapsterConfig.RuleMap.ToDictionary(r => (r.Key.Source, r.Key.Destination), r => r.Value);
            var cycleGuardConfig = mapsterConfig.Clone();
            cycleGuardConfig.Default.MaxDepth(MappingSamples.CycleGuardDepth);
            var mappers = new Mappers(autoMapperConfig.CreateMapper(), new MapsterMapper.Mapper(mapsterConfig), new MapsterMapper.Mapper(cycleGuardConfig));

            var autoMapperPairs = typeMaps.Select(m => (m.SourceType, m.DestinationType)).ToHashSet();
            var failures = rules.Keys
                .Where(pair => !autoMapperPairs.Contains(pair))
                .Select(pair => $"{MappingSamples.Name(pair.Source, pair.Destination)}: registered in Mapster, but AutoMapper has no map to compare it with.")
                .ToList();
            var untranslated = new SortedDictionary<string, string>(StringComparer.Ordinal);

            foreach (var typeMap in typeMaps)
            {
                var pair = (typeMap.SourceType, typeMap.DestinationType);
                var translated = rules.TryGetValue(pair, out var rule);
                var problem = typeMap.PreserveReferences && rule?.Settings.PreserveReference != true
                    ? "AutoMapper tracks references on this self-referencing map; add .PreserveReference(true)."
                    : CheckMap(mappers, typeMap, samples.TryGetValue(pair, out var handWritten) ? handWritten : Array.Empty<Func<object>>());

                var name = MappingSamples.Name(typeMap.SourceType, typeMap.DestinationType);
                if (problem != null && translated)
                {
                    failures.Add($"{name}: {problem}");
                }
                else if (problem != null)
                {
                    untranslated[name] = problem;
                }
            }

            return new ParityResult(typeMaps.Count, rules.Count, failures, untranslated);
        }

        // Null when every sample comes out the same from both mappers, otherwise what went wrong and how to fix it.
        private static string CheckMap(Mappers mappers, TypeMap typeMap, Func<object>[] handWritten)
        {
            var sourceType = typeMap.SourceType;

            // The filled sample is the main one: where AutoMapper cannot map it, a hand-written sample has to stand in.
            var generated = MappingSamples.Fill(sourceType);
            string generatedProblem = null;
            if (generated != null)
            {
                var (outcome, problem) = CompareSample(mappers, typeMap, generated, "generated sample");
                if (outcome == Outcome.Differs)
                {
                    return problem + ".";
                }

                generatedProblem = problem;
            }

            // The empty sample adds the null branches, wherever AutoMapper can map it.
            var empty = MappingSamples.Empty(sourceType);
            if (empty != null)
            {
                var (outcome, problem) = CompareSample(mappers, typeMap, empty, "empty sample");
                if (outcome == Outcome.Differs)
                {
                    return problem + ".";
                }
            }

            var cyclicCompared = false;
            for (var i = 0; i < handWritten.Length; i++)
            {
                var label = handWritten.Length == 1 ? "hand-written sample" : $"hand-written sample {i + 1}";
                var source = handWritten[i]();
                var cyclic = typeMap.PreserveReferences && MappingSamples.HasCycle(source);
                var (outcome, problem) = cyclic ? CompareCyclic(mappers, typeMap, source, label) : CompareSample(mappers, typeMap, source, label);
                if (outcome != Outcome.Same)
                {
                    return outcome == Outcome.AutoMapperFails ? $"{problem}; fix the hand-written sample." : problem + ".";
                }

                cyclicCompared |= cyclic;
            }

            if (handWritten.Length == 0 && generated == null)
            {
                return "no sample can be generated for the source; add a hand-written one.";
            }

            if (handWritten.Length == 0 && generatedProblem != null)
            {
                return $"{generatedProblem}; add a hand-written sample.";
            }

            return typeMap.PreserveReferences && !cyclicCompared ? CheckGeneratedCycle(mappers, typeMap) : null;
        }

        private static string CheckGeneratedCycle(Mappers mappers, TypeMap typeMap)
        {
            var source = MappingSamples.Fill(typeMap.SourceType);
            if (source == null || MappingSamples.CloseCycles(source, typeMap.SourceType) == 0)
            {
                // No back-reference within the generated sample's depth does not mean the map never sees a cycle.
                return "AutoMapper treats this map as self-referencing, but no sample has a cycle; add a hand-written sample that has one.";
            }

            var (outcome, problem) = CompareCyclic(mappers, typeMap, source, "cyclic sample");
            return outcome switch
            {
                Outcome.Same => null,
                Outcome.AutoMapperFails => $"{problem}; add a hand-written sample that has a cycle.",
                _ => problem + "."
            };
        }

        // The sample mapped onto a new destination and, where the destination type allows it, onto a populated existing one.
        // Same, or the first problem found.
        private static (Outcome, string) CompareSample(Mappers mappers, TypeMap typeMap, object source, string label)
        {
            var created = Compare(mappers.AutoMapper, mappers.Mapster, typeMap, source, null, label);
            if (created.Item1 != Outcome.Same || !CanMapOnto(typeMap.DestinationType))
            {
                return created;
            }

            // Where AutoMapper cannot map onto the existing destination, only the new-destination result counts.
            var existing = (Func<object>)(() => MappingSamples.Fill(typeMap.DestinationType, "existing-"));
            var onto = Compare(mappers.AutoMapper, mappers.Mapster, typeMap, source, existing, $"{label} mapped onto an existing destination");
            return onto.Item1 == Outcome.Differs ? onto : created;
        }

        // On the MaxDepth-guarded config first, so an untracked cycle shows as a difference instead of a stack overflow.
        // Once that matches, nothing was cut off and the cycle is tracked, so the production config can map it safely too.
        private static (Outcome, string) CompareCyclic(Mappers mappers, TypeMap typeMap, object source, string label)
        {
            var guarded = Compare(mappers.AutoMapper, mappers.CycleGuard, typeMap, source, null, label);
            return guarded.Item1 == Outcome.Same ? Compare(mappers.AutoMapper, mappers.Mapster, typeMap, source, null, label) : guarded;
        }

        private static (Outcome, string) Compare(IMapper autoMapper, MapsterMapper.IMapper mapster, TypeMap typeMap, object source, Func<object> existing, string on)
        {
            var (sourceType, destinationType) = (typeMap.SourceType, typeMap.DestinationType);

            // A member whose getter throws is left out of the JSON, so it would go uncompared: both sides must serialize cleanly.
            var autoMapperErrors = new List<string>();
            JToken expected;
            List<string> noisePaths;
            try
            {
                expected = MappingSamples.MapWithAutoMapper(autoMapper, source, sourceType, destinationType, autoMapperErrors, out noisePaths, existing);
            }
            catch (Exception ex)
            {
                return (Outcome.AutoMapperFails, $"AutoMapper throws {ex.GetBaseException().GetType().Name} on the {on}");
            }

            if (autoMapperErrors.Count > 0)
            {
                return (Outcome.AutoMapperFails, $"AutoMapper's result on the {on} fails to serialize at {MappingSamples.Paths(autoMapperErrors.Distinct().ToList())}");
            }

            var mapsterErrors = new List<string>();
            JToken actual;
            try
            {
                var result = existing == null ? mapster.Map(source, sourceType, destinationType) : mapster.Map(source, existing(), sourceType, destinationType);
                actual = MappingSamples.ToJson(result, mapsterErrors);
            }
            catch (Exception ex)
            {
                return (Outcome.Differs, $"Mapster throws on the {on}: {ex.GetBaseException().Message}");
            }

            if (mapsterErrors.Count > 0)
            {
                return (Outcome.Differs, $"Mapster's result on the {on} fails to serialize at {MappingSamples.Paths(mapsterErrors.Distinct().ToList())}");
            }

            MappingSamples.Blank(actual, noisePaths);
            var differing = MappingSamples.DifferingPaths(expected, actual);
            return differing.Count == 0
                ? (Outcome.Same, null)
                : (Outcome.Differs, $"differs from AutoMapper on the {on} at {MappingSamples.Paths(differing)}");
        }

        // Updates map onto objects: classes with members, not strings or collections.
        private static bool CanMapOnto(Type destinationType)
        {
            return destinationType.IsClass && !destinationType.IsAbstract && destinationType != typeof(string) && !typeof(IEnumerable).IsAssignableFrom(destinationType);
        }

        private record Mappers(IMapper AutoMapper, MapsterMapper.IMapper Mapster, MapsterMapper.IMapper CycleGuard);
    }

    /// <summary>
    /// Failures: translated maps that differ from AutoMapper, and registrations AutoMapper has no map for. Untranslated: maps
    /// not translated yet whose Mapster convention result differs, with the reason, i.e. the work left before the switch-over.
    /// </summary>
    public record ParityResult(int AutoMapperMaps, int Translated, IReadOnlyList<string> Failures, IReadOnlyDictionary<string, string> Untranslated);
}
