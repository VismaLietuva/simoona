using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Mapster;
using NUnit.Framework;
using Shrooms.Presentation.ModelMappings;
using Shrooms.Presentation.ModelMappings.MapsterSetup;

namespace Shrooms.Premium.Tests.ModelMappings
{
    /// <summary>
    /// Gate for the AutoMapper to Mapster migration: every map translated to Mapster must produce what AutoMapper produces
    /// today, and the maps left to Mapster's conventions that still differ are listed exactly. The Check_ tests cover the
    /// check itself on small hand-made configs.
    /// </summary>
    [TestFixture]
    public class MapsterParityTests
    {
        // Samples on top of the generated ones: for a source that cannot be generated (abstract types), for values AutoMapper
        // needs in a certain shape (strings it parses as JSON or time zone ids), for the branches of conditional maps, and for
        // self-referencing maps whose cycle the generated sample cannot close. The check says when a map needs one.
        private static readonly Dictionary<(Type Source, Type Destination), Func<object>[]> HandWrittenSamples = new();

        // Maps not translated yet whose Mapster convention result differs from AutoMapper's: the work left before PR 6
        // switches over, which needs this list empty. A listed map that starts matching, or an unlisted one that stops
        // (say a DTO changed), fails UntranslatedMaps_DifferOnlyWhereListed, so the list stays exact.
        private static readonly string[] UntranslatedMaps =
        {
            "AbstractClassifier → AbstractClassifierPostViewModel",
            "AbstractClassifier → AbstractClassifierViewModel",
            "AbstractClassifierAbstractViewModel → Language",
            "AbstractClassifierPostViewModel → AbstractClassifier",
            "AbstractClassifierViewModel → AbstractClassifier",
            "ApplicationRole → RoleMiniViewModel",
            "ApplicationRoleViewModel → ApplicationRole",
            "ApplicationUser → AbstractViewModel",
            "ApplicationUser → AdministrationUserDto",
            "ApplicationUser → ApplicationUserDetailsViewModel",
            "ApplicationUser → ApplicationUserViewModel",
            "ApplicationUser → ChangeProfileInfoViewModel",
            "ApplicationUser → ChangeProfileOfficeViewModel",
            "ApplicationUser → OfficeUserDto",
            "ApplicationUser → String",
            "ApplicationUser → UserDto",
            "ApplicationUserMinimalDto → ApplicationUser",
            "ApplicationUserMinimalDto → String",
            "ApplicationUserViewModel → ApplicationUser",
            "ApplicationUserViewModel → String",
            "Certificate → CertificatePostViewModel",
            "CertificatePostViewModel → Certificate",
            "CertificatePostViewModel → Int32",
            "CertificateViewModel → Certificate",
            "ChangeProfileInfoViewModel → ApplicationUser",
            "ChangeProfileOfficeViewModel → ApplicationUser",
            "Comment → CommentDto",
            "CommentViewModel → Comment",
            "CommitteePostDto → Committee",
            "CommitteePostViewModel → Committee",
            "CommitteeViewModel → Committee",
            "CreateEventViewModel → CreateEventDto",
            "CreateWallPostViewModel → NewPostDto",
            "CreateWallViewModel → CreateWallDto",
            "EditCommentViewModel → EditCommentDto",
            "EditPostViewModel → EditPostDto",
            "EmployeeDto → EmployeeViewModel",
            "Event → Event",
            "EventChangeOptionViewModel → EventChangeOptionsDto",
            "EventDetailsDto → EventDetailsViewModel",
            "EventEditDetailsDto → EventEditDetailsViewModel",
            "EventJoinMultipleViewModel → EventJoinDto",
            "EventJoinViewModel → EventJoinDto",
            "EventListItemDto → EventListItemViewModel",
            "EventQuestionStructureDto → EventQuestionViewModel",
            "EventQuestionViewModel → EventQuestionStructureDto",
            "EventReminderDetailsDto → EventReminderDetailsViewModel",
            "Exam → Int32",
            "ExamPostViewModel → Exam",
            "ExamPostViewModel → Int32",
            "Floor → FloorViewModel",
            "Floor → MapFloorViewModel",
            "FloorPostViewModel → Floor",
            "ILazyPaged<BooksByOfficeDto> → LazyPaged<BooksByOfficeViewModel>",
            "LotteryDetailsViewModel → LotteryDetailsDto",
            "NewCommentViewModel → NewCommentDto",
            "Notification → NotificationDto",
            "NotificationDto → NotificationViewModel",
            "Office → OfficeViewModel",
            "OfficePostViewModel → Office",
            "OrganizationalStructureDto → OrganizationalStructureViewModel",
            "Post → PostDto",
            "PostDto → EventPostViewModel",
            "QualificationLevelPostViewModel → QualificationLevel",
            "QualificationLevelViewModel → QualificationLevel",
            "RoleMiniViewModel → ApplicationRole",
            "Room → RoomPostViewModel",
            "Room → RoomViewModel",
            "RoomPostViewModel → Room",
            "RoomTypePostViewModel → RoomType",
            "RoomViewModel → Room",
            "ServiceRequestCategoryViewModel → ServiceRequestCategoryDto",
            "ShareEventViewModel → NewPostDto",
            "UpdateAttendStatusViewModel → UpdateAttendStatusDto",
            "UpdateEventViewModel → EditEventDto",
            "UpdateWallViewModel → UpdateWallDto",
            "UserDto → ApplicationUserViewModel",
            "UserKudosInformationDto → UserKudosInformationViewModel",
            "VacationDto → VacationViewModel",
            "WallPostViewModel → NotificationDto",
        };

        private static readonly Dictionary<(Type Source, Type Destination), Func<object>[]> NoSamples = new();

        private static readonly Lazy<ParityResult> Production = new(() => MappingParity.Check(
            new MapperConfiguration(cfg => cfg.AddMaps(MappingAssemblies.Names)),
            MapsterConfiguration.Create(),
            HandWrittenSamples));

        [Test]
        public void TranslatedMaps_MatchAutoMapper()
        {
            var result = Production.Value;

            TestContext.Out.WriteLine($"{result.Translated} of {result.AutoMapperMaps} AutoMapper maps are translated to Mapster; " +
                                      $"{result.Untranslated.Count} untranslated maps still differ by convention.");
            Assert.That(result.Failures, Is.Empty, string.Join(Environment.NewLine, result.Failures));
        }

        [Test]
        public void UntranslatedMaps_DifferOnlyWhereListed()
        {
            var differing = Production.Value.Untranslated;
            var newlyDiffering = differing.Where(m => !UntranslatedMaps.Contains(m.Key)).Select(m => $"  {m.Key}: {m.Value}").ToList();
            var nowMatching = UntranslatedMaps.Where(m => !differing.ContainsKey(m)).Select(m => $"  {m}").ToList();

            var message = string.Join(Environment.NewLine, new[]
                {
                    newlyDiffering.Count > 0 ? "Differ from AutoMapper but are not in UntranslatedMaps (translate them):" : null,
                    string.Join(Environment.NewLine, newlyDiffering),
                    nowMatching.Count > 0 ? "Match AutoMapper now (remove them from UntranslatedMaps):" : null,
                    string.Join(Environment.NewLine, nowMatching)
                }.Where(line => !string.IsNullOrEmpty(line)));
            Assert.That(newlyDiffering.Count + nowMatching.Count, Is.Zero, message);
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
            Assert.That(result.Untranslated.Keys, Does.Not.Contain("Person → PersonDto"));
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
        public void Check_ListsUntranslatedMapsThatDifferByConvention()
        {
            // Nothing registered: Mapster's convention misses FullName, which AutoMapper builds with MapFrom.
            var result = MappingParity.Check(AutoMapperConfig(), MapsterSettings.Create(), NoSamples);

            Assert.That(result.Failures, Is.Empty);
            Assert.That(result.Untranslated["Person → PersonDto"], Does.Contain("at FullName"));
            Assert.That(result.Untranslated.Keys, Does.Not.Contain("Reading → ReadingDto"));
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
        public void Check_ComparesTheEmptySample()
        {
            // Right whenever the sample has a manager, which the filled sample always does.
            var mapster = MapsterSettings.Create();
            mapster.NewConfig<Employee, EmployeeDto>().Map(d => d.HasManager, s => true);

            var result = MappingParity.Check(AutoMapperConfig(), mapster, NoSamples);

            Assert.That(result.Failures, Has.One.Contains("on the empty sample at HasManager"));
        }

        [Test]
        public void Check_ComparesEveryHandWrittenSample()
        {
            // Right while ShowIf has an option; the second sample takes AutoMapper's fallback to ShowIfOptionId.
            var mapster = MapsterSettings.Create();
            mapster.NewConfig<Question, QuestionDto>().Map(d => d.Condition, s => s.ShowIf != null && s.ShowIf.Count > 0 ? s.ShowIf[0] : 0);
            var samples = Samples<Question, QuestionDto>(
                () => new Question { ShowIf = new List<int> { 3 } },
                () => new Question { ShowIf = new List<int>(), ShowIfOptionId = 7 });

            var result = MappingParity.Check(AutoMapperConfig(), mapster, samples);

            Assert.That(result.Failures, Has.One.Contains("on the hand-written sample 2 at Condition"));
        }

        [Test]
        public void Check_ComparesMappingOntoAnExistingDestination()
        {
            // Right on a new Room, where Id is 0 either way; on an existing one AutoMapper keeps the Id it ignores.
            var mapster = MapsterSettings.Create();
            mapster.NewConfig<RoomUpdate, Room>().Map(d => d.Id, s => 0);

            var result = MappingParity.Check(AutoMapperConfig(), mapster, NoSamples);

            Assert.That(result.Failures, Has.One.Contains("on the generated sample mapped onto an existing destination at Id"));
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
        public void Check_AsksForACyclicSample_WhenTheGeneratedOneCannotCloseTheCycle()
        {
            // Ring -> Link1 -> Link2 -> Link3 -> Ring: the generated sample stops before Link3, so it holds no cycle.
            var autoMapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Ring, RingDto>(MemberList.None);
                cfg.CreateMap<Link1, Link1Dto>(MemberList.None);
                cfg.CreateMap<Link2, Link2Dto>(MemberList.None);
                cfg.CreateMap<Link3, Link3Dto>(MemberList.None);
            });
            var mapster = MapsterSettings.Create();
            mapster.NewConfig<Ring, RingDto>().PreserveReference(true);
            mapster.NewConfig<Link1, Link1Dto>().PreserveReference(true);
            mapster.NewConfig<Link2, Link2Dto>().PreserveReference(true);
            mapster.NewConfig<Link3, Link3Dto>().PreserveReference(true);
            var samples = Samples<Ring, RingDto>(() =>
            {
                var ring = new Ring { Name = "r", Link = new Link1 { Link = new Link2 { Link = new Link3() } } };
                ring.Link.Link.Link.Ring = ring;
                return ring;
            });

            var withoutSample = MappingParity.Check(autoMapper, mapster, NoSamples);
            var withSample = MappingParity.Check(autoMapper, mapster, samples);

            Assert.That(withoutSample.Failures, Has.Some.StartsWith("Ring → RingDto").And.Contains("no sample has a cycle"));
            Assert.That(withSample.Failures, Has.None.StartsWith("Ring → RingDto"));
        }

        [Test]
        public void Check_ReportsAResultAutoMapperCannotSerialize()
        {
            // Text throws on both sides, so comparing what did serialize would pass without ever looking at it.
            var mapster = MapsterSettings.Create();
            mapster.NewConfig<Reading, ReadingDto>();
            var samples = Samples<Reading, ReadingDto>(() => new Reading { Value = -1 });

            var result = MappingParity.Check(AutoMapperConfig(), mapster, samples);

            Assert.That(result.Failures, Has.One.Contains("AutoMapper's result on the hand-written sample fails to serialize at Text; fix the hand-written sample"));
        }

        [Test]
        public void Check_ReportsAResultMapsterCannotSerialize()
        {
            var mapster = MapsterSettings.Create();
            mapster.NewConfig<Reading, ReadingDto>().Map(d => d.Value, s => -1);

            var result = MappingParity.Check(AutoMapperConfig(), mapster, NoSamples);

            Assert.That(result.Failures, Has.One.Contains("Mapster's result on the generated sample fails to serialize at Text"));
        }

        [Test]
        public void Check_AsksForAHandWrittenSample_WhenNoneCanBeGenerated()
        {
            var mapster = MapsterSettings.Create();
            mapster.NewConfig<Shape, ShapeDto>();
            var samples = Samples<Shape, ShapeDto>(() => new Circle { Name = "c" });

            var withoutSample = MappingParity.Check(AutoMapperConfig(), mapster, NoSamples);
            var withSample = MappingParity.Check(AutoMapperConfig(), mapster, samples);

            Assert.That(withoutSample.Failures, Has.One.Contains("add a hand-written one"));
            Assert.That(withSample.Failures, Is.Empty);
        }

        private static Dictionary<(Type Source, Type Destination), Func<object>[]> Samples<TSource, TDestination>(params Func<object>[] samples)
        {
            return new Dictionary<(Type Source, Type Destination), Func<object>[]> { [(typeof(TSource), typeof(TDestination))] = samples };
        }

        private static MapperConfiguration AutoMapperConfig()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Person, PersonDto>(MemberList.None)
                    .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName));
                cfg.CreateMap<Employee, EmployeeDto>(MemberList.None)
                    .ForMember(d => d.HasManager, o => o.MapFrom(s => s.Manager != null));
                cfg.CreateMap<Question, QuestionDto>(MemberList.None)
                    .ForMember(d => d.Condition, o => o.MapFrom(s => s.ShowIf != null && s.ShowIf.Count > 0 ? s.ShowIf[0] : s.ShowIfOptionId));
                cfg.CreateMap<RoomUpdate, Room>(MemberList.None)
                    .ForMember(d => d.Id, o => o.Ignore());
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

        public class Employee
        {
            public Person Manager { get; set; }
        }

        public class EmployeeDto
        {
            public bool HasManager { get; set; }
        }

        public class Question
        {
            public List<int> ShowIf { get; set; }

            public int ShowIfOptionId { get; set; }
        }

        public class QuestionDto
        {
            public int Condition { get; set; }
        }

        public class RoomUpdate
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public class Room
        {
            public int Id { get; set; }

            public string Name { get; set; }
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

        public class Ring
        {
            public string Name { get; set; }

            public Link1 Link { get; set; }
        }

        public class Link1
        {
            public Link2 Link { get; set; }
        }

        public class Link2
        {
            public Link3 Link { get; set; }
        }

        public class Link3
        {
            public Ring Ring { get; set; }
        }

        public class RingDto
        {
            public string Name { get; set; }

            public Link1Dto Link { get; set; }
        }

        public class Link1Dto
        {
            public Link2Dto Link { get; set; }
        }

        public class Link2Dto
        {
            public Link3Dto Link { get; set; }
        }

        public class Link3Dto
        {
            public RingDto Ring { get; set; }
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
