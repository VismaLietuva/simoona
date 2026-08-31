using System.Collections.Generic;
using NUnit.Framework;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.Domain.Services.Vacations;

namespace Shrooms.Premium.Tests.DomainService.VacationService
{
    [TestFixture]
    public class VacationNameIndexTests
    {
        private static ApplicationUser User(string id, string first, string last)
        {
            return new ApplicationUser { Id = id, FirstName = first, LastName = last };
        }

        private static string Find(IReadOnlyDictionary<string, ApplicationUser> index, string name)
        {
            return VacationNameIndex.TryFind(index, name, out var user) ? user.Id : null;
        }

        [Test]
        public void Build_MatchesEitherNameOrder()
        {
            var index = VacationNameIndex.Build(new[] { User("a", "Rūta", "Trumpauskaitė") });

            Assert.That(Find(index, "Rūta Trumpauskaitė"), Is.EqualTo("a"));
            Assert.That(Find(index, "Trumpauskaitė Rūta"), Is.EqualTo("a"));
        }

        /// <summary>Payroll writes plain ASCII where the directory has diacritics.</summary>
        [Test]
        public void Build_IgnoresDiacriticsAndCase()
        {
            var index = VacationNameIndex.Build(new[] { User("a", "Rūta", "Trumpauskaitė") });

            Assert.That(Find(index, "ruta trumpauskaite"), Is.EqualTo("a"));
        }

        /// <summary>
        /// Two people whose names mirror each other: guessing would charge one
        /// person's leave to the other, so neither claims the ambiguous form.
        /// </summary>
        [Test]
        public void Build_RefusesToGuessBetweenMirroredNames()
        {
            var index = VacationNameIndex.Build(new[]
            {
                User("a", "Jonas", "Petras"),
                User("b", "Petras", "Jonas")
            });

            Assert.That(Find(index, "Jonas Petras"), Is.Null);
            Assert.That(Find(index, "Petras Jonas"), Is.Null);
        }

        [Test]
        public void Build_RefusesToGuessBetweenTwoPeopleOfTheSameName()
        {
            var index = VacationNameIndex.Build(new[]
            {
                User("a", "Jonas", "Jonaitis"),
                User("b", "Jonas", "Jonaitis")
            });

            Assert.That(Find(index, "Jonas Jonaitis"), Is.Null);
        }

        /// <summary>One person, so their own reversed form is not a clash.</summary>
        [Test]
        public void Build_KeepsAPalindromicNameForItsOwnerOnly()
        {
            var index = VacationNameIndex.Build(new[] { User("a", "Jonas", "Jonas") });

            Assert.That(Find(index, "Jonas Jonas"), Is.EqualTo("a"));
        }

        [Test]
        public void Build_SkipsAUserWithNoNameAtAll()
        {
            var index = VacationNameIndex.Build(new[] { User("a", null, null), User("b", "Ona", "Onaitė") });

            Assert.That(Find(index, "Ona Onaitė"), Is.EqualTo("b"));
            Assert.That(Find(index, ""), Is.Null);
        }
    }
}
