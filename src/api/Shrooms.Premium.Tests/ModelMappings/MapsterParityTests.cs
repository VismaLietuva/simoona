using System;
using System.Collections.Generic;
using AutoMapper;
using Mapster;
using NUnit.Framework;
using Shrooms.Presentation.ModelMappings.MapsterSetup;

namespace Shrooms.Premium.Tests.ModelMappings
{
    /// <summary>
    /// Gate for the AutoMapper to Mapster migration: every map translated to Mapster must produce what AutoMapper produces
    /// today, until the switch-over. The Check_ tests cover the check itself on small hand-made configs.
    /// </summary>
    [TestFixture]
    public class MapsterParityTests
    {
        // Samples for translated maps whose source cannot be generated (abstract types) or that AutoMapper cannot map
        // generated values for (strings it parses as JSON or time zone ids). The check says when a map needs one.
        private static readonly Dictionary<(Type Source, Type Destination), Func<object>> HandWrittenSamples = new();

        private static readonly Dictionary<(Type Source, Type Destination), Func<object>> NoSamples = new();

        [Test]
        public void TranslatedMaps_MatchAutoMapper()
        {
            var autoMapperConfig = new MapperConfiguration(cfg => cfg.AddMaps(MapsterConfiguration.AssemblyNames));

            var result = MappingParity.Check(autoMapperConfig, MapsterConfiguration.Create(), HandWrittenSamples);

            TestContext.Out.WriteLine($"{result.Translated} of {result.AutoMapperMaps} AutoMapper maps are translated to Mapster.");
            Assert.That(result.Failures, Is.Empty, string.Join(Environment.NewLine, result.Failures));
        }

        [Test]
        public void Configuration_Compiles()
        {
            Assert.That(() => MapsterConfiguration.Create().Compile(), Throws.Nothing);
        }

        [Test]
        public void Check_PassesAFaithfulTranslation()
        {
            var mapster = MapsterSettings.Create();
            mapster.NewConfig<Person, PersonDto>().Map(d => d.FullName, s => s.FirstName + " " + s.LastName);

            var result = MappingParity.Check(AutoMapperConfig(), mapster, NoSamples);

            Assert.That(result.Translated, Is.EqualTo(1));
            Assert.That(result.Failures, Is.Empty);
        }

        [Test]
        public void Check_ReportsAMemberThatDiffers()
        {
            var mapster = MapsterSettings.Create();
            mapster.NewConfig<Person, PersonDto>().Map(d => d.FullName, s => s.FirstName);

            var result = MappingParity.Check(AutoMapperConfig(), mapster, NoSamples);

            Assert.That(result.Failures, Has.One.Contains("Person → PersonDto").And.Contains("at FullName"));
        }

        [Test]
        public void Check_ReportsAMapAutoMapperDoesNotHave()
        {
            var mapster = MapsterSettings.Create();
            mapster.NewConfig<PersonDto, Person>();

            var result = MappingParity.Check(AutoMapperConfig(), mapster, NoSamples);

            Assert.That(result.Failures, Has.One.Contains("AutoMapper has no map"));
        }

        [Test]
        public void Check_ReportsASelfReferencingMapWithoutPreserveReference()
        {
            var mapster = MapsterSettings.Create();
            mapster.NewConfig<Node, NodeDto>();

            var result = MappingParity.Check(AutoMapperConfig(), mapster, NoSamples);

            Assert.That(result.Failures, Has.One.Contains("PreserveReference(true)"));
        }

        [Test]
        public void Check_ComparesCyclicSamples_OnSelfReferencingMaps()
        {
            var mapster = MapsterSettings.Create();
            mapster.NewConfig<Node, NodeDto>().PreserveReference(true);

            var result = MappingParity.Check(AutoMapperConfig(), mapster, NoSamples);

            Assert.That(result.Failures, Is.Empty);
        }

        [Test]
        public void Check_ReportsACycleTheTranslationLoses()
        {
            // Right on the plain sample, wrong on the cyclic one. The JSON leaves out AutoMapper's preserved back-reference,
            // so Mapster's explicit null in its place must count as a difference.
            var mapster = MapsterSettings.Create();
            mapster.NewConfig<Node, NodeDto>().PreserveReference(true).Map(d => d.Next, s => s.Next == s ? null : s.Next);

            var result = MappingParity.Check(AutoMapperConfig(), mapster, NoSamples);

            Assert.That(result.Failures, Has.One.Contains("on the cyclic sample at Next"));
        }

        [Test]
        public void Check_ReportsAResultAutoMapperCannotSerialize()
        {
            // Text throws on both sides, so comparing what did serialize would pass without ever looking at it.
            var mapster = MapsterSettings.Create();
            mapster.NewConfig<Reading, ReadingDto>();
            var samples = new Dictionary<(Type Source, Type Destination), Func<object>> { [(typeof(Reading), typeof(ReadingDto))] = () => new Reading { Value = -1 } };

            var result = MappingParity.Check(AutoMapperConfig(), mapster, samples);

            Assert.That(result.Failures, Has.One.Contains("AutoMapper's result on the sample fails to serialize at Text; fix the hand-written sample"));
        }

        [Test]
        public void Check_ReportsAResultMapsterCannotSerialize()
        {
            var mapster = MapsterSettings.Create();
            mapster.NewConfig<Reading, ReadingDto>().Map(d => d.Value, s => -1);

            var result = MappingParity.Check(AutoMapperConfig(), mapster, NoSamples);

            Assert.That(result.Failures, Has.One.Contains("Mapster's result on the sample fails to serialize at Text"));
        }

        [Test]
        public void Check_AsksForAHandWrittenSample_WhenNoneCanBeGenerated()
        {
            var mapster = MapsterSettings.Create();
            mapster.NewConfig<Shape, ShapeDto>();
            var samples = new Dictionary<(Type Source, Type Destination), Func<object>> { [(typeof(Shape), typeof(ShapeDto))] = () => new Circle { Name = "c" } };

            var withoutSample = MappingParity.Check(AutoMapperConfig(), mapster, NoSamples);
            var withSample = MappingParity.Check(AutoMapperConfig(), mapster, samples);

            Assert.That(withoutSample.Failures, Has.One.Contains("add a hand-written one"));
            Assert.That(withSample.Failures, Is.Empty);
        }

        private static MapperConfiguration AutoMapperConfig()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Person, PersonDto>(MemberList.None)
                    .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName));
                cfg.CreateMap<Node, NodeDto>(MemberList.None);
                cfg.CreateMap<Shape, ShapeDto>(MemberList.None);
                cfg.CreateMap<Reading, ReadingDto>(MemberList.None);
            });
        }

        public class Person
        {
            public int Id { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }
        }

        public class PersonDto
        {
            public int Id { get; set; }

            public string FullName { get; set; }
        }

        public class Node
        {
            public string Name { get; set; }

            public Node Next { get; set; }
        }

        public class NodeDto
        {
            public string Name { get; set; }

            public NodeDto Next { get; set; }
        }

        public class Reading
        {
            public int Value { get; set; }
        }

        public class ReadingDto
        {
            public int Value { get; set; }

            public string Text => Value < 0 ? throw new InvalidOperationException("No reading.") : Value.ToString();
        }

        public abstract class Shape
        {
            public string Name { get; set; }
        }

        public class Circle : Shape
        {
        }

        public class ShapeDto
        {
            public string Name { get; set; }
        }
    }
}
