using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.Internal;
using Mapster;
using Newtonsoft.Json.Linq;

namespace Shrooms.Premium.Tests.ModelMappings
{
    /// <summary>
    /// Checks every map registered in a Mapster config against the AutoMapper map for the same pair: a filled sample must
    /// come out as the same JSON from both, and self-referencing maps must keep their cycles. Maps not translated yet are
    /// left to AutoMapper and not checked.
    /// </summary>
    public static class MappingParity
    {
        // Backstop for the cyclic samples: an untracked cycle shows as a difference instead of a stack overflow that takes
        // the test host down. Kept small on purpose: Mapster unrolls MaxDepth at compile time.
        private const int CycleGuardDepth = 8;

        private const int MaxPathsShown = 6;

        public static ParityResult Check(MapperConfiguration autoMapperConfig, TypeAdapterConfig mapsterConfig, IReadOnlyDictionary<(Type Source, Type Destination), Func<object>> samples)
        {
            var typeMaps = autoMapperConfig.Internal().GetAllTypeMaps().ToDictionary(m => (m.SourceType, m.DestinationType));
            var autoMapper = autoMapperConfig.CreateMapper();

            // Read before anything maps, so the list holds the registered pairs only.
            var translated = mapsterConfig.RuleMap.ToList();
            var mapster = new MapsterMapper.Mapper(mapsterConfig);
            var cycleGuardConfig = mapsterConfig.Clone();
            cycleGuardConfig.Default.MaxDepth(CycleGuardDepth);
            var cycleGuardMapper = new MapsterMapper.Mapper(cycleGuardConfig);

            var failures = new List<string>();
            foreach (var (pair, rule) in translated.OrderBy(r => MappingSamples.Name(r.Key.Source, r.Key.Destination)))
            {
                var name = MappingSamples.Name(pair.Source, pair.Destination);
                if (!typeMaps.TryGetValue((pair.Source, pair.Destination), out var typeMap))
                {
                    failures.Add($"{name}: registered in Mapster, but AutoMapper has no map to compare it with.");
                    continue;
                }

                if (typeMap.PreserveReferences && rule.Settings.PreserveReference != true)
                {
                    failures.Add($"{name}: AutoMapper tracks references on this self-referencing map; add .PreserveReference(true).");
                    continue;
                }

                var problem = Compare(autoMapper, mapster, pair.Source, pair.Destination, samples, cyclic: false);
                if (problem == null && typeMap.PreserveReferences)
                {
                    problem = Compare(autoMapper, cycleGuardMapper, pair.Source, pair.Destination, samples, cyclic: true);
                }

                if (problem != null)
                {
                    failures.Add($"{name}: {problem}");
                }
            }

            return new ParityResult(typeMaps.Count, translated.Count, failures);
        }

        // Null when both mappers produce the same JSON, otherwise what went wrong.
        private static string Compare(IMapper autoMapper, MapsterMapper.IMapper mapster, Type sourceType, Type destinationType, IReadOnlyDictionary<(Type, Type), Func<object>> samples, bool cyclic)
        {
            var handWritten = samples.TryGetValue((sourceType, destinationType), out var sample);
            var source = handWritten ? sample() : MappingSamples.Fill(sourceType);
            if (source == null)
            {
                return "no sample can be generated for the source; add a hand-written one.";
            }

            var on = cyclic ? "cyclic sample" : "sample";
            if (cyclic && MappingSamples.CloseCycles(source, sourceType) == 0)
            {
                // Nothing in the sample can point back at the root, so there is no cycle to check.
                return null;
            }

            // A member whose getter throws is left out of the JSON, so it would go uncompared: both sides must serialize cleanly.
            var fix = handWritten ? "fix the hand-written sample" : "add a hand-written sample";
            var autoMapperErrors = new List<string>();
            JToken expected;
            List<string> noisePaths;
            try
            {
                expected = MappingSamples.MapWithAutoMapper(autoMapper, source, sourceType, destinationType, autoMapperErrors, out noisePaths);
            }
            catch (Exception ex)
            {
                return $"AutoMapper throws {ex.GetBaseException().GetType().Name} on the {on}; {fix}.";
            }

            if (autoMapperErrors.Count > 0)
            {
                return $"AutoMapper's result on the {on} fails to serialize at {Paths(autoMapperErrors.Distinct().ToList())}; {fix}.";
            }

            var mapsterErrors = new List<string>();
            JToken actual;
            try
            {
                actual = MappingSamples.ToJson(mapster.Map(source, sourceType, destinationType), mapsterErrors);
            }
            catch (Exception ex)
            {
                return $"Mapster throws on the {on}: {ex.GetBaseException().Message}";
            }

            if (mapsterErrors.Count > 0)
            {
                return $"Mapster's result on the {on} fails to serialize at {Paths(mapsterErrors.Distinct().ToList())}.";
            }

            MappingSamples.Blank(actual, noisePaths);
            var differing = MappingSamples.DifferingPaths(expected, actual);
            if (differing.Count == 0)
            {
                return null;
            }

            return $"differs from AutoMapper on the {on} at {Paths(differing)}.";
        }

        private static string Paths(List<string> paths)
        {
            var shown = string.Join(", ", paths.Take(MaxPathsShown));
            return paths.Count > MaxPathsShown ? $"{shown} (+{paths.Count - MaxPathsShown} more)" : shown;
        }
    }

    public record ParityResult(int AutoMapperMaps, int Translated, IReadOnlyList<string> Failures);
}
