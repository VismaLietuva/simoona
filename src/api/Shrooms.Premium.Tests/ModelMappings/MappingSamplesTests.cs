using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Shrooms.Premium.Tests.ModelMappings
{
    [TestFixture]
    public class MappingSamplesTests
    {
        [Test]
        public void SaltedSample_DiffersFromTheUnsaltedOneAndFromDefaults()
        {
            // Mapping onto the salted sample must show both a member wrongly kept and one wrongly reset. Bools can only do the
            // second, so they stay true.
            var plain = (Values)MappingSamples.Fill(typeof(Values));
            var salted = (Values)MappingSamples.Fill(typeof(Values), "existing-");

            foreach (var property in typeof(Values).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var saltedValue = property.GetValue(salted);
                var defaultValue = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;
                if (property.PropertyType != typeof(bool))
                {
                    Assert.That(saltedValue, Is.Not.EqualTo(property.GetValue(plain)), property.Name);
                }

                Assert.That(saltedValue, Is.Not.EqualTo(defaultValue), property.Name);
            }
        }

        [Test]
        public void HasCycle_FollowsDictionaryValuesAndPublicFields()
        {
            var throughDictionary = new Holder();
            throughDictionary.Children["self"] = throughDictionary;
            var throughField = new Holder();
            throughField.Next = throughField;

            Assert.That(MappingSamples.HasCycle(throughDictionary), Is.True);
            Assert.That(MappingSamples.HasCycle(throughField), Is.True);
            Assert.That(MappingSamples.HasCycle(new Holder { Next = new Holder() }), Is.False);
        }

        [Test]
        public void DifferingPaths_QuotesNamesThatAreNotIdentifiers_SoBlankCanFindThem()
        {
            var expected = JObject.Parse("{ \"Floor\": { \"display.name\": \"a\", \"it's\": 1 } }");
            var actual = JObject.Parse("{ \"Floor\": { \"display.name\": \"b\", \"it's\": 2 } }");

            var paths = MappingSamples.DifferingPaths(expected, actual);
            MappingSamples.Blank(expected, paths);
            MappingSamples.Blank(actual, paths);

            Assert.That(paths, Is.EqualTo(new[] { "Floor['display.name']", "Floor['it\\'s']" }));
            Assert.That(MappingSamples.DifferingPaths(expected, actual), Is.Empty);
        }

        public enum Status
        {
            None,
            Open,
            Closed
        }

        public class Values
        {
            public int Number { get; set; }

            public byte Small { get; set; }

            public long Large { get; set; }

            public decimal Amount { get; set; }

            public bool Flag { get; set; }

            public Status State { get; set; }

            public Status? MaybeState { get; set; }

            public string Name { get; set; }

            public DateTime When { get; set; }

            public Guid Key { get; set; }
        }

        public class Holder
        {
            public Holder Next;

            public Dictionary<string, Holder> Children { get; } = new();
        }
    }
}
