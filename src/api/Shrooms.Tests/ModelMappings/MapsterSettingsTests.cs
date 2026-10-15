using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Mapster;
using NUnit.Framework;
using Shrooms.Presentation.ModelMappings.MapsterSetup;

namespace Shrooms.Tests.ModelMappings
{
    /// <summary>
    /// Pins each MapsterSettings default against what AutoMapper does today (phase 0 of the AutoMapper to Mapster migration).
    /// Every test maps the same source with both libraries and requires the same result.
    /// </summary>
    [TestFixture]
    public class MapsterSettingsTests
    {
        private AutoMapper.IMapper _autoMapper;
        private MapsterMapper.IMapper _mapster;

        [SetUp]
        public void SetUp()
        {
            _autoMapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>(MemberList.None);
                cfg.CreateMap<Child, ChildDto>(MemberList.None);
                cfg.CreateMap<Node, NodeDto>(MemberList.None);
            }).CreateMapper();

            _mapster = new MapsterMapper.Mapper(MapsterSettings.Create());
        }

        [Test]
        public void MemberNames_MatchIgnoringCase()
        {
            var source = new Source { UserId = 7 };

            Assert.That(_autoMapper.Map<Destination>(source).UserID, Is.EqualTo(7));
            Assert.That(_mapster.Map<Destination>(source).UserID, Is.EqualTo(7));
        }

        [Test]
        public void MemberNames_MatchExactlyWithoutTheSetting()
        {
            // Documents why the setting exists: Mapster's own default would silently drop the value.
            var source = new Source { UserId = 7 };

            Assert.That(new MapsterMapper.Mapper(new TypeAdapterConfig()).Map<Destination>(source).UserID, Is.EqualTo(0));
        }

        [Test]
        public void AdaptExtension_UsesTheSameDefaultsAsTheMapper()
        {
            // Adapt() reads TypeAdapterConfig.GlobalSettings; MapsterSettings.Create() (run in SetUp) applies the defaults there too.
            var result = new Source { UserId = 7, Children = null }.Adapt<Destination>();

            Assert.That(result.UserID, Is.EqualTo(7));
            Assert.That(result.Children, Is.Not.Null.And.Empty);
        }

        [Test]
        public void NestedMembers_AreFlattened()
        {
            var source = new Source { Manager = new Child { FirstName = "Bo" } };

            Assert.That(_autoMapper.Map<Destination>(source).ManagerFirstName, Is.EqualTo("Bo"));
            Assert.That(_mapster.Map<Destination>(source).ManagerFirstName, Is.EqualTo("Bo"));
        }

        [Test]
        public void NullCollections_BecomeEmpty()
        {
            var source = new Source { Children = null };

            Assert.That(_autoMapper.Map<Destination>(source).Children, Is.Not.Null.And.Empty);
            Assert.That(_mapster.Map<Destination>(source).Children, Is.Not.Null.And.Empty);
        }

        [Test]
        public void PublicFields_AreMapped()
        {
            var source = new Source { Flag = true };

            Assert.That(_autoMapper.Map<Destination>(source).Flag, Is.True);
            Assert.That(_mapster.Map<Destination>(source).Flag, Is.True);
        }

        [Test]
        public void SameTypeMembers_AreAssignedByReference()
        {
            var shared = new Child { FirstName = "same" };
            var source = new Source { Same = shared };

            Assert.That(_autoMapper.Map<Destination>(source).Same, Is.SameAs(shared));
            Assert.That(_mapster.Map<Destination>(source).Same, Is.SameAs(shared));
        }

        [Test]
        public void MapIntoExisting_KeepsUnmatchedMembersAndNestedInstances()
        {
            var autoMapperTarget = ExistingDestination();
            var mapsterTarget = ExistingDestination();
            var autoMapperNested = autoMapperTarget.Manager2;
            var mapsterNested = mapsterTarget.Manager2;

            _autoMapper.Map(NewValues(), autoMapperTarget);
            _mapster.Map(NewValues(), mapsterTarget);

            foreach (var (target, nested) in new[] { (autoMapperTarget, autoMapperNested), (mapsterTarget, mapsterNested) })
            {
                Assert.That(target.Untouched, Is.EqualTo("keep"));
                Assert.That(target.Manager2, Is.SameAs(nested));
                Assert.That(target.Manager2.FirstName, Is.EqualTo("new manager"));
                Assert.That(target.List, Has.Count.EqualTo(1));
                Assert.That(target.List[0].FirstName, Is.EqualTo("new item"));
            }
        }

        [Test]
        public void MapIntoExisting_ReplacesTheCollectionInstance_UnlikeAutoMapper()
        {
            // Known difference, reviewed per call site in phase 3: AutoMapper refills the existing list, Mapster assigns a
            // new one. It matters only when the target is a tracked EF entity whose collection EF watches.
            var autoMapperTarget = ExistingDestination();
            var mapsterTarget = ExistingDestination();
            var autoMapperList = autoMapperTarget.List;
            var mapsterList = mapsterTarget.List;

            _autoMapper.Map(NewValues(), autoMapperTarget);
            _mapster.Map(NewValues(), mapsterTarget);

            Assert.That(autoMapperTarget.List, Is.SameAs(autoMapperList));
            Assert.That(mapsterTarget.List, Is.Not.SameAs(mapsterList));
        }

        [Test]
        public void SelfReferencingGraph_KeepsTheCycle_WhenPreserveReferenceIsSetForTheType()
        {
            // AutoMapper enables reference tracking on its own for self-referencing types; Mapster needs it per type.
            var config = MapsterSettings.Create();
            config.NewConfig<Node, NodeDto>().PreserveReference(true);
            var node = new Node { Name = "root" };
            node.Next = node;

            var autoMapperResult = _autoMapper.Map<NodeDto>(node);
            var mapsterResult = new MapsterMapper.Mapper(config).Map<NodeDto>(node);

            Assert.That(autoMapperResult.Next, Is.SameAs(autoMapperResult));
            Assert.That(mapsterResult.Next, Is.SameAs(mapsterResult));
        }

        [Test]
        public void CycleAcrossTwoTypes_KeepsTheBackReference()
        {
            // Room -> Floor -> Rooms -> Room, as EF hands it over. Needs PreserveReference on both maps and
            // AvoidInlineMapping (in MapsterSettings): inlined nested mappings skip the reference tracking.
            var config = MapsterSettings.Create();
            config.NewConfig<Room, RoomDto>().PreserveReference(true);
            config.NewConfig<Floor, FloorDto>().PreserveReference(true);
            var room = new Room { Name = "A1", Floor = new Floor { Name = "1st" } };
            room.Floor.Rooms.Add(room);

            var result = new MapsterMapper.Mapper(config).Map<RoomDto>(room);

            Assert.That(result.Floor.Rooms.Single(), Is.SameAs(result));
        }

        private static Destination ExistingDestination()
        {
            return new Destination
            {
                Untouched = "keep",
                Manager2 = new ChildDto { FirstName = "old manager" },
                List = new List<ChildDto> { new ChildDto { FirstName = "old item" } }
            };
        }

        private static Source NewValues()
        {
            return new Source
            {
                Manager2 = new Child { FirstName = "new manager" },
                List = new List<Child> { new Child { FirstName = "new item" } }
            };
        }

        public class Child
        {
            public string FirstName { get; set; }
        }

        public class ChildDto
        {
            public string FirstName { get; set; }
        }

        public class Source
        {
            public bool Flag;

            public int UserId { get; set; }

            public Child Manager { get; set; }

            public Child Manager2 { get; set; }

            public List<Child> Children { get; set; }

            public List<Child> List { get; set; }

            public Child Same { get; set; }
        }

        public class Destination
        {
            public bool Flag;

            public int UserID { get; set; }

            public string ManagerFirstName { get; set; }

            public ChildDto Manager2 { get; set; }

            public List<ChildDto> Children { get; set; }

            public List<ChildDto> List { get; set; }

            public string Untouched { get; set; }

            public Child Same { get; set; }
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

        public class Room
        {
            public string Name { get; set; }

            public Floor Floor { get; set; }
        }

        public class Floor
        {
            public string Name { get; set; }

            public List<Room> Rooms { get; set; } = new List<Room>();
        }

        public class RoomDto
        {
            public string Name { get; set; }

            public FloorDto Floor { get; set; }
        }

        public class FloorDto
        {
            public string Name { get; set; }

            public List<RoomDto> Rooms { get; set; }
        }
    }
}
