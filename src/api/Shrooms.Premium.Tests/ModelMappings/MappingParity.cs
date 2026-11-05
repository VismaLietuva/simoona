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
        private const string ExistingSalt = "existing-";

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

            // Read before anything maps, so they hold what was registered only.
            var registered = new Registrations(mapsterConfig);
            var cycleGuardConfig = mapsterConfig.Clone();
            cycleGuardConfig.Default.MaxDepth(MappingSamples.CycleGuardDepth);
            var mappers = new Mappers(autoMapperConfig.CreateMapper(), new MapsterMapper.Mapper(mapsterConfig), new MapsterMapper.Mapper(cycleGuardConfig));

            var autoMapperPairs = typeMaps.Select(m => (m.SourceType, m.DestinationType)).ToHashSet();
            var failures = registered.Pairs
                .Where(pair => !autoMapperPairs.Contains(pair))
                .Select(pair => $"{MappingSamples.Name(pair.Source, pair.Destination)}: registered in Mapster, but AutoMapper has no map to compare it with.")
                .ToList();
            var untranslated = new SortedDictionary<string, string>(StringComparer.Ordinal);
            var translatedCount = 0;

            foreach (var typeMap in typeMaps)
            {
                var pair = (typeMap.SourceType, typeMap.DestinationType);
                var translated = registered.Translates(pair);
                translatedCount += translated ? 1 : 0;
                var problem = typeMap.PreserveReferences && !registered.PreservesReferences(pair)
                    ? new Problem("AutoMapper tracks references on this self-referencing map; add .PreserveReference(true).")
                    : CheckMap(mappers, typeMap, samples.TryGetValue(pair, out var handWritten) ? handWritten : Array.Empty<Func<object>>());

                var name = MappingSamples.Name(typeMap.SourceType, typeMap.DestinationType);
                if (problem != null && translated)
                {
                    failures.Add($"{name}: {problem.WithDetail}");
                }
                else if (problem != null)
                {
                    untranslated[name] = problem.Reason;
                }
            }

            return new ParityResult(typeMaps.Count, translatedCount, failures, untranslated);
        }

        // Null when every sample comes out the same from both mappers, otherwise what went wrong and how to fix it.
        private static Problem CheckMap(Mappers mappers, TypeMap typeMap, Func<object>[] handWritten)
        {
            var sourceType = typeMap.SourceType;

            // The filled sample is the main one: where AutoMapper cannot map it, a hand-written sample has to stand in.
            var generated = MappingSamples.Fill(sourceType);
            Problem generatedProblem = null;
            if (generated != null)
            {
                var (outcome, problem) = CompareSample(mappers, typeMap, generated, "generated sample");
                if (outcome == Outcome.Differs)
                {
                    return problem;
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
                    return problem;
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
                    return outcome == Outcome.AutoMapperFails ? problem.Then("fix the hand-written sample") : problem;
                }

                cyclicCompared |= cyclic;
            }

            if (handWritten.Length == 0 && generated == null)
            {
                return new Problem("no sample can be generated for the source; add a hand-written one.");
            }

            if (handWritten.Length == 0 && generatedProblem != null)
            {
                return generatedProblem.Then("add a hand-written sample");
            }

            return typeMap.PreserveReferences && !cyclicCompared ? CheckGeneratedCycle(mappers, typeMap) : null;
        }

        private static Problem CheckGeneratedCycle(Mappers mappers, TypeMap typeMap)
        {
            var source = MappingSamples.Fill(typeMap.SourceType);
            if (source == null || MappingSamples.CloseCycles(source, typeMap.SourceType) == 0)
            {
                // No back-reference within the generated sample's depth does not mean the map never sees a cycle.
                return new Problem("AutoMapper treats this map as self-referencing, but no sample has a cycle; add a hand-written sample that has one.");
            }

            var (outcome, problem) = CompareCyclic(mappers, typeMap, source, "cyclic sample");
            return outcome == Outcome.AutoMapperFails ? problem.Then("add a hand-written sample that has a cycle") : problem;
        }

        // The sample mapped onto a new destination and, where the destination type allows it, onto a populated existing one.
        // Same, or the first problem found.
        private static (Outcome, Problem) CompareSample(Mappers mappers, TypeMap typeMap, object source, string label)
        {
            var created = Compare(mappers.AutoMapper, mappers.Mapster, typeMap, source, null, label);
            if (created.Item1 != Outcome.Same || !CanMapOnto(typeMap.DestinationType))
            {
                return created;
            }

            // Where AutoMapper cannot map onto the existing destination, only the new-destination result counts.
            var existing = (Func<object>)(() => MappingSamples.Fill(typeMap.DestinationType, ExistingSalt));
            var onto = Compare(mappers.AutoMapper, mappers.Mapster, typeMap, source, existing, $"{label} mapped onto an existing destination");
            return onto.Item1 == Outcome.Differs ? onto : created;
        }

        // On the MaxDepth-guarded config first, so an untracked cycle shows as a difference instead of a stack overflow.
        // Once that matches, nothing was cut off and the cycle is tracked, so the production config can map it safely too.
        private static (Outcome, Problem) CompareCyclic(Mappers mappers, TypeMap typeMap, object source, string label)
        {
            var guarded = Compare(mappers.AutoMapper, mappers.CycleGuard, typeMap, source, null, label);
            return guarded.Item1 == Outcome.Same ? Compare(mappers.AutoMapper, mappers.Mapster, typeMap, source, null, label) : guarded;
        }

        private static (Outcome, Problem) Compare(IMapper autoMapper, MapsterMapper.IMapper mapster, TypeMap typeMap, object source, Func<object> existing, string on)
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
                return (Outcome.AutoMapperFails, new Problem($"AutoMapper throws {ex.GetBaseException().GetType().Name} on the {on}."));
            }

            if (autoMapperErrors.Count > 0)
            {
                return (Outcome.AutoMapperFails, new Problem($"AutoMapper's result on the {on} fails to serialize at {MappingSamples.Paths(autoMapperErrors.Distinct().ToList())}."));
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
                var exception = ex.GetBaseException();
                return (Outcome.Differs, new Problem($"Mapster throws {exception.GetType().Name} on the {on}.", exception.Message));
            }

            if (mapsterErrors.Count > 0)
            {
                return (Outcome.Differs, new Problem($"Mapster's result on the {on} fails to serialize at {MappingSamples.Paths(mapsterErrors.Distinct().ToList())}."));
            }

            MappingSamples.Blank(actual, noisePaths);
            var differing = MappingSamples.DifferingPaths(expected, actual);
            return differing.Count == 0
                ? (Outcome.Same, null)
                : (Outcome.Differs, new Problem($"differs from AutoMapper on the {on} at {MappingSamples.Paths(differing)}."));
        }

        // Updates map onto objects: classes with members, not strings or collections, that a sample can be built for.
        private static bool CanMapOnto(Type destinationType)
        {
            return destinationType.IsClass && !destinationType.IsAbstract && destinationType != typeof(string)
                   && !typeof(IEnumerable).IsAssignableFrom(destinationType)
                   && MappingSamples.Fill(destinationType, ExistingSalt) != null;
        }

        private record Mappers(IMapper AutoMapper, MapsterMapper.IMapper Mapster, MapsterMapper.IMapper CycleGuard);

        // Reason is stable (no exception text), so UntranslatedMaps can pin it; Detail adds the exception message for failures.
        private record Problem(string Reason, string Detail = null)
        {
            public string WithDetail => Detail == null ? Reason : $"{Reason} {Detail}";

            public Problem Then(string fix)
            {
                return this with { Reason = $"{Reason.TrimEnd('.')}; {fix}." };
            }
        }

        /// <summary>
        /// What the Mapster config says about a pair, decided the way Mapster decides it: by every rule whose priority applies,
        /// so a rule for a base source type or a ForDestinationType rule counts, not only NewConfig for the exact pair.
        /// </summary>
        private sealed class Registrations
        {
            private readonly IReadOnlyList<TypeAdapterRule> _rules;
            private readonly TypeAdapterRule _default;

            public Registrations(TypeAdapterConfig config)
            {
                _rules = config.Rules.ToList();
                _default = _rules.Single(r => ReferenceEquals(r.Settings, config.Default.Settings));
                Pairs = config.RuleMap.Keys
                    .Where(k => k.Source != typeof(void) && k.Destination != typeof(void))
                    .Select(k => (k.Source, k.Destination))
                    .ToList();
            }

            // Pairs registered by name (NewConfig, ForType), which must each have an AutoMapper map. ForSourceType and
            // ForDestinationType register under void for the open side, so they are not pairs.
            public IReadOnlyList<(Type Source, Type Destination)> Pairs { get; }

            // A rule ranked above the defaults applies to the pair (Mapster's built-in rules rank below them).
            public bool Translates((Type Source, Type Destination) pair)
            {
                var applying = Applying(pair).ToList();
                var defaultPriority = applying.First(r => ReferenceEquals(r.Rule, _default)).Priority;
                return applying.Any(r => r.Priority > defaultPriority);
            }

            // The setting Mapster ends up with: the highest-priority rule that sets it.
            public bool PreservesReferences((Type Source, Type Destination) pair)
            {
                return Applying(pair).Select(r => r.Rule.Settings.PreserveReference).FirstOrDefault(p => p != null) == true;
            }

            private IEnumerable<(TypeAdapterRule Rule, int Priority)> Applying((Type Source, Type Destination) pair)
            {
                var argument = new PreCompileArgument { SourceType = pair.Source, DestinationType = pair.Destination, MapType = MapType.Map, ExplicitMapping = true };
                return _rules
                    .Select(rule => (Rule: rule, Priority: rule.Priority(argument)))
                    .Where(r => r.Priority != null)
                    .Select(r => (r.Rule, r.Priority.Value))
                    .OrderByDescending(r => r.Item2);
            }
        }
    }

    /// <summary>
    /// Translated: the AutoMapper maps a Mapster rule applies to. Failures: translated maps that differ from AutoMapper, and
    /// registrations AutoMapper has no map for. Untranslated: maps not translated yet whose Mapster convention result
    /// differs, with the reason, i.e. the work left before the switch-over.
    /// </summary>
    public record ParityResult(int AutoMapperMaps, int Translated, IReadOnlyList<string> Failures, IReadOnlyDictionary<string, string> Untranslated);
}
