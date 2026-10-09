using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shrooms.Contracts.DAL;

namespace Shrooms.Tests.Infrastructure
{
    [TestFixture]
    public class SafeQueryTests
    {
        private class Org
        {
            public int Id { get; set; }
            public string ShortName { get; set; }
        }

        private class Item
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Points { get; set; }
            public DateTime Created { get; set; }
            public string PasswordHash { get; set; }
            public string SecurityStamp { get; set; }
            public Org Organization { get; set; }
            public List<Org> Children { get; set; }
        }

        private static IQueryable<Item> Items() => new[]
        {
            new Item { Id = 1, Name = "b", Points = 5, Created = new DateTime(2026, 1, 2), Organization = new Org { Id = 2, ShortName = "y" } },
            new Item { Id = 2, Name = "a", Points = 9, Created = new DateTime(2026, 1, 1), Organization = new Org { Id = 1, ShortName = "z" } },
            new Item { Id = 3, Name = "c", Points = 5, Created = new DateTime(2026, 1, 3), Organization = new Org { Id = 3, ShortName = "x" } },
        }.AsQueryable();

        [Test]
        public void OrderBy_SingleProperty_Ascending()
        {
            var ids = SafeQuery.OrderBy(Items(), "Name asc").Select(i => i.Id).ToArray();

            Assert.That(ids, Is.EqualTo(new[] { 2, 1, 3 }));
        }

        [Test]
        public void OrderBy_MultipleClauses_WithDirections()
        {
            var ids = SafeQuery.OrderBy(Items(), "Points desc, Id desc").Select(i => i.Id).ToArray();

            Assert.That(ids, Is.EqualTo(new[] { 2, 3, 1 }));
        }

        [Test]
        public void OrderBy_NestedPath_AndCaseInsensitiveNames()
        {
            var ids = SafeQuery.OrderBy(Items(), "organization.shortname").Select(i => i.Id).ToArray();

            Assert.That(ids, Is.EqualTo(new[] { 3, 1, 2 }));
        }

        [TestCase("Name.Length desc")]
        [TestCase("Points > 5 ? 0 : 1")]
        [TestCase("np(Organization.ShortName)")]
        [TestCase("Name.Contains(\"a\") desc")]
        [TestCase("DoesNotExist asc")]
        [TestCase("Organization asc")]
        [TestCase("Name sideways")]
        [TestCase("")]
        [TestCase(null)]
        public void OrderBy_RejectsExpressionsAndUnknownMembers_LeavingOrderUnchanged(string orderBy)
        {
            var ids = SafeQuery.OrderBy(Items(), orderBy).Select(i => i.Id).ToArray();

            Assert.That(ids, Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(SafeQuery.IsValidOrderBy<Item>(orderBy), Is.False);
        }

        [TestCase("PasswordHash")]
        [TestCase("SecurityStamp desc")]
        [TestCase("Name asc, PasswordHash desc")]
        public void OrderBy_NeverSortsBySensitiveMembers(string orderBy)
        {
            var ordered = SafeQuery.OrderBy(Items(), orderBy);

            Assert.That(ordered.Expression.ToString(), Does.Not.Contain("PasswordHash").And.Not.Contain("SecurityStamp"));
        }

        [Test]
        public void OrderBy_SkipsInvalidClausesButKeepsValidOnes()
        {
            var ids = SafeQuery.OrderBy(Items(), "Bogus asc, Name desc").Select(i => i.Id).ToArray();

            Assert.That(ids, Is.EqualTo(new[] { 3, 1, 2 }));
        }

        private class Room
        {
            public int Id { get; set; }
            public List<Item> ApplicationUsers { get; set; }
        }

        private class Floor
        {
            public int Id { get; set; }
            public List<Room> Rooms { get; set; }
        }

        private class Office
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<Floor> Floors { get; set; }
        }

        [Test]
        public void ValidIncludePaths_KeepsNavigationsOnly_InCanonicalCasing()
        {
            var includes = SafeQuery.ValidIncludePaths<Item>("organization, Children, Name, Nope, Organization.ShortName, PasswordHash");

            Assert.That(includes, Is.EqualTo(new[] { "Organization", "Children" }));
        }

        [Test]
        public void ValidIncludePaths_FollowsCollectionsIntoTheirElementType()
        {
            var includes = SafeQuery.ValidIncludePaths<Office>("Floors,Floors.Rooms,Floors.Rooms.ApplicationUsers,Floors.Rooms.ApplicationUsers.Organization,Floors.Rooms.ApplicationUsers.Name,Floors.Nope");

            Assert.That(includes, Is.EqualTo(new[] { "Floors", "Floors.Rooms", "Floors.Rooms.ApplicationUsers", "Floors.Rooms.ApplicationUsers.Organization" }));
        }

        [Test]
        public void OrderBy_StillRejectsPathsThroughCollections()
        {
            var offices = new[] { new Office { Id = 2, Name = "b" }, new Office { Id = 1, Name = "a" } }.AsQueryable();

            Assert.That(SafeQuery.IsValidOrderBy<Office>("Floors.Rooms.Id"), Is.False);
            Assert.That(SafeQuery.OrderBy(offices, "Floors.Id desc").Select(o => o.Id).ToArray(), Is.EqualTo(new[] { 2, 1 }));
        }

        private class Mixed
        {
            public int Id { get; set; }
            public string Name { get; set; }
            [System.ComponentModel.DataAnnotations.Schema.NotMapped]
            public string FullName { get; set; }
            public string Ignored { get; set; }
        }

        [Test]
        public void NotMappedProperties_AreNeverSortedOrIncluded()
        {
            Assert.That(SafeQuery.IsValidOrderBy<Mixed>("FullName"), Is.False);
            Assert.That(SafeQuery.IsValidOrderBy<Mixed>("Name"), Is.True);
        }

        [Test]
        public void QueryablePredicate_ExcludesFluentIgnoredMembers()
        {
            bool IsQueryable(System.Reflection.PropertyInfo p) => p.Name != "Ignored";

            Assert.That(SafeQuery.IsValidOrderBy<Mixed>("Ignored", IsQueryable), Is.False);
            Assert.That(SafeQuery.IsValidOrderBy<Mixed>("Ignored"), Is.True, "without the predicate only [NotMapped] is known");
            Assert.That(SafeQuery.IsValidOrderBy<Mixed>("Name", IsQueryable), Is.True);
        }

        [Test]
        public void OrderBy_SupportsCountOfACollection_AsTheAdminUserListSends()
        {
            var items = new[]
            {
                new Item { Id = 1, Children = new List<Org> { new(), new() } },
                new Item { Id = 2, Children = new List<Org>() },
                new Item { Id = 3, Children = new List<Org> { new() } },
            }.AsQueryable();

            Assert.That(SafeQuery.OrderBy(items, "Children.Count() desc").Select(i => i.Id).ToArray(), Is.EqualTo(new[] { 1, 3, 2 }));
            Assert.That(SafeQuery.OrderBy(items, "Children.Count asc").Select(i => i.Id).ToArray(), Is.EqualTo(new[] { 2, 3, 1 }));
            Assert.That(SafeQuery.IsValidOrderBy<Item>("Name.Count()"), Is.False, "Count is only for collections");
            Assert.That(SafeQuery.IsValidOrderBy<Item>("Children.Count().Foo"), Is.False);
        }

        [TestCase("BookAppAuthorizationGuid")]
        [TestCase("SecurityStamp")]
        [TestCase("PhoneNumber")]
        public void SensitiveMembers_AreNeverIncludedOrSorted(string member)
        {
            Assert.That(SafeQuery.IsValidOrderBy<Item>(member), Is.False);
            Assert.That(SafeQuery.ValidIncludePaths<Item>(member), Is.Empty);
        }

        [Test]
        public void ValidIncludePaths_EmptyInput_ReturnsEmpty()
        {
            Assert.That(SafeQuery.ValidIncludePaths<Item>(null), Is.Empty);
            Assert.That(SafeQuery.ValidIncludePaths<Item>("  "), Is.Empty);
        }
    }
}
