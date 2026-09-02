using System.Linq;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models.Events;

namespace Shrooms.Premium.Tests.DataLayer
{
    public class EventQuestionModelTests
    {
        private ShroomsDbContext _context;

        [SetUp]
        public void TestInitializer()
        {
            var options = new DbContextOptionsBuilder<ShroomsDbContext>()
                .UseInMemoryDatabase(databaseName: "EventQuestionModelTests")
                .Options;

            _context = new ShroomsDbContext(options);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public void Should_Register_EventQuestion_Entity()
        {
            var entityType = _context.Model.FindEntityType(typeof(EventQuestion));

            Assert.That(entityType, Is.Not.Null);
        }

        [Test]
        public void Should_Limit_EventQuestion_Title_To_100_Characters()
        {
            var entityType = _context.Model.FindEntityType(typeof(EventQuestion));

            var title = entityType.FindProperty(nameof(EventQuestion.Title));

            Assert.That(title.GetMaxLength(), Is.EqualTo(100));
            Assert.That(title.IsNullable, Is.False);
        }

        [Test]
        public void Should_Make_EventOption_QuestionId_Nullable_For_Legacy_Options()
        {
            var entityType = _context.Model.FindEntityType(typeof(EventOption));

            var questionId = entityType.FindProperty(nameof(EventOption.QuestionId));

            Assert.That(questionId, Is.Not.Null);
            Assert.That(questionId.IsNullable, Is.True);
        }

        [Test]
        public void Should_Restrict_Delete_On_ShowIfOption_To_Protect_The_Question_Tree()
        {
            var entityType = _context.Model.FindEntityType(typeof(EventQuestion));

            var showIf = entityType.FindNavigation(nameof(EventQuestion.ShowIfOption));

            Assert.That(showIf.ForeignKey.DeleteBehavior, Is.EqualTo(DeleteBehavior.Restrict));
        }

        [Test]
        public void Should_Expose_EventQuestions_From_Event()
        {
            var entityType = _context.Model.FindEntityType(typeof(Event));

            var navigation = entityType.FindNavigation(nameof(Event.EventQuestions));

            Assert.That(navigation, Is.Not.Null);
            Assert.That(navigation.TargetEntityType.ClrType, Is.EqualTo(typeof(EventQuestion)));
        }

        [Test]
        public void Should_Not_Create_A_Shadow_Foreign_Key_For_EventQuestions()
        {
            var entityType = _context.Model.FindEntityType(typeof(EventQuestion));

            var foreignKeys = entityType.GetForeignKeys()
                .Where(fk => fk.PrincipalEntityType.ClrType == typeof(Event))
                .ToList();

            Assert.That(foreignKeys, Has.Count.EqualTo(1));
            Assert.That(foreignKeys[0].Properties[0].Name, Is.EqualTo(nameof(EventQuestion.EventId)));
        }
    }
}
