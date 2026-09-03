using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Razor.Templating.Core;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Attributes;
using Shrooms.Infrastructure.Email.Templating;

namespace Shrooms.Tests.Infrastructure
{
    public class EmailViewModelWithoutTimeZoneableProperties : BaseEmailTemplateViewModel
    {
        public EmailViewModelWithoutTimeZoneableProperties(DateTime date, string userNotificationSettingsUrl = null)
            : base(userNotificationSettingsUrl)
        {
            Date = date;
        }

        public DateTime Date { get; set; }
    }

    public class EmailViewModelWithTimeZoneableProperties : BaseEmailTemplateViewModel
    {
        public EmailViewModelWithTimeZoneableProperties(DateTime date, string userNotificationSettingsUrl = null)
            : base(userNotificationSettingsUrl)
        {
            Date = date;
        }

        [ApplyTimeZoneChanges]
        public DateTime Date { get; set; }
    }

    [TestFixture]
    public class MailTemplateTests
    {
        private IRazorTemplateEngine _razorTemplateEngine;
        private MailTemplate _sut;

        [SetUp]
        public void TestInitializer()
        {
            _razorTemplateEngine = Substitute.For<IRazorTemplateEngine>();
            _sut = new MailTemplate(_razorTemplateEngine, Substitute.For<IApplicationSettings>());
        }

        [Test]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(10)]
        public async Task Should_Generate_NewPost_EmailContent(int retries)
        {
            var newWallPostEmailTemplateViewModel = new NewWallPostEmailTemplateViewModel(
                "WallTitle",
                "http://picture.example.com",
                "Iam Creator",
                "http://post.example.com/1",
                "body",
                "http://settings.example.com/1",
                "Read it");

            var kudosSentEmailTemplateViewModel = new KudosSentEmailTemplateViewModel(
                "http://settings.example.com/1",
                "Iam Creator",
                10,
                "New kudos for you!",
                "http://profile.example.com/1");

            _razorTemplateEngine
                .RenderAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<Dictionary<string, object>>())
                .Returns(Task.FromResult("<html>rendered</html>"));

            for (var i = 0; i < retries; i++)
            {
                await _sut.GenerateAsync(newWallPostEmailTemplateViewModel, "/EmailTemplates/Wall/NewPost.cshtml");
                await _sut.GenerateAsync(kudosSentEmailTemplateViewModel, "/EmailTemplates/Kudos/KudosSent.cshtml");
            }
        }

        [Test]
        public async Task GenerateAsync_WithoutTimeZoneKey_GeneratesEmailBody()
        {
            // Arrange
            var viewModel = new EmailViewModelWithTimeZoneableProperties(DateTime.UtcNow);
            var templateKey = Guid.NewGuid().ToString();
            const string expectedBody = "2024-01-15";

            _razorTemplateEngine
                .RenderAsync(templateKey, Arg.Any<object>(), Arg.Any<Dictionary<string, object>>())
                .Returns(Task.FromResult(expectedBody));

            // Act
            var result = await _sut.GenerateAsync(viewModel, templateKey);

            // Assert
            Assert.That(string.IsNullOrEmpty(result), Is.False);
        }

        [Test]
        public async Task GenerateAsync_WithTimeZoneKey_GeneratesEmailBodyWithTransformedDate()
        {
            // Arrange
            var date = RemoveMilliseconds(DateTime.UtcNow);
            var timeZoneKey = GetLastAvailableTimeZoneKey();
            var viewModel = new EmailViewModelWithTimeZoneableProperties(date);
            var templateKey = Guid.NewGuid().ToString();
            var expectedDate = ConvertUtcToTimeZoneWithoutMilliseconds(date, timeZoneKey);

            _razorTemplateEngine
                .RenderAsync(templateKey, Arg.Any<object>(), Arg.Any<Dictionary<string, object>>())
                .Returns(callInfo =>
                {
                    // Capture the date that was on the model when render was called
                    var model = (EmailViewModelWithTimeZoneableProperties)callInfo.ArgAt<object>(1);
                    return Task.FromResult(model.Date.ToString("o"));
                });

            // Act
            var result = await _sut.GenerateAsync(viewModel, templateKey, timeZoneKey);

            // Assert
            Assert.That(DateTime.Parse(result), Is.EqualTo(expectedDate));
        }

        [Test]
        public async Task GenerateAsync_WithTimeZoneKeyAndWithoutMarkedProperties_GeneratesDefaultEmailBody()
        {
            // Arrange
            var date = RemoveMilliseconds(DateTime.UtcNow);
            var timeZoneKey = GetLastAvailableTimeZoneKey();
            var viewModel = new EmailViewModelWithoutTimeZoneableProperties(date);
            var templateKey = Guid.NewGuid().ToString();

            _razorTemplateEngine
                .RenderAsync(templateKey, Arg.Any<object>(), Arg.Any<Dictionary<string, object>>())
                .Returns(callInfo =>
                {
                    var model = (EmailViewModelWithoutTimeZoneableProperties)callInfo.ArgAt<object>(1);
                    return Task.FromResult(model.Date.ToString("o"));
                });

            // Act
            var result = await _sut.GenerateAsync(viewModel, templateKey, timeZoneKey);

            // Assert
            Assert.That(DateTime.Parse(result), Is.EqualTo(date));
        }

        [Test]
        public void GenerateAsync_WithoutTimeZoneKeys_Throws()
        {
            // Arrange
            var viewModel = new EmailViewModelWithTimeZoneableProperties(DateTime.UtcNow);
            var templateKey = Guid.NewGuid().ToString();
            var timeZoneKeys = new List<string>();

            // Assert
            Assert.ThrowsAsync<ArgumentException>(() => _sut.GenerateAsync(viewModel, templateKey, timeZoneKeys));
        }

        [Test]
        public void GenerateAsync_WithTimeZoneKeysAndWithoutMarkedProperties_Throws()
        {
            // Arrange
            var viewModel = new EmailViewModelWithoutTimeZoneableProperties(DateTime.UtcNow);
            var templateKey = Guid.NewGuid().ToString();
            var timeZoneKeys = new List<string> { GetLastAvailableTimeZoneKey() };

            // Assert
            Assert.ThrowsAsync<ArgumentException>(() => _sut.GenerateAsync(viewModel, templateKey, timeZoneKeys));
        }

        [Test]
        public async Task GenerateAsync_WithTimeZoneKeys_GeneratesEmailBodyWithTransformedDate()
        {
            // Arrange
            var date = DateTime.UtcNow;
            var timeZoneKeys = new List<string> { GetLastAvailableTimeZoneKey() };
            var viewModel = new EmailViewModelWithTimeZoneableProperties(date);
            var templateKey = Guid.NewGuid().ToString();
            var expectedDate = ConvertUtcToTimeZoneWithoutMilliseconds(date, timeZoneKeys[0]);

            _razorTemplateEngine
                .RenderAsync(templateKey, Arg.Any<object>(), Arg.Any<Dictionary<string, object>>())
                .Returns(callInfo =>
                {
                    var model = (EmailViewModelWithTimeZoneableProperties)callInfo.ArgAt<object>(1);
                    return Task.FromResult(model.Date.ToString("o"));
                });

            // Act
            var result = await _sut.GenerateAsync(viewModel, templateKey, timeZoneKeys);

            // Assert
            Assert.That(DateTime.Parse(result.Values[timeZoneKeys[0]]), Is.EqualTo(expectedDate).Within(TimeSpan.FromSeconds(1)));
        }

        private static string GetLastAvailableTimeZoneKey()
        {
            return TimeZoneInfo.GetSystemTimeZones().Last().Id;
        }

        private static DateTime ConvertUtcToTimeZoneWithoutMilliseconds(DateTime date, string timeZoneKey)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneKey);
            var zonedDate = TimeZoneInfo.ConvertTimeFromUtc(date, timeZone);
            return RemoveMilliseconds(zonedDate);
        }

        private static DateTime RemoveMilliseconds(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
        }
    }
}
