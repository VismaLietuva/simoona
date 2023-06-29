using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RazorEngine;
using RazorEngine.Templating;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Infrastructure.Email.Attributes;
using Shrooms.Infrastructure.Email.Templating;

namespace Shrooms.Tests.Infrastructure
{
    public class EmailViewModelWithoutTimeZoneableProperties : BaseEmailTemplateViewModel
    {
        public EmailViewModelWithoutTimeZoneableProperties(DateTime date, string userNotificationSettingsUrl = null)
            :
            base(userNotificationSettingsUrl)
        {
            Date = date;
        }

        public DateTime Date { get; set; }
    }

    public class EmailViewModelWithTimeZoneableProperties : BaseEmailTemplateViewModel
    {
        public EmailViewModelWithTimeZoneableProperties(DateTime date, string userNotificationSettingsUrl = null)
            :
            base(userNotificationSettingsUrl)
        {
            Date = date;
        }

        [ApplyTimeZoneChanges]
        public DateTime Date { get; set; }
    }

    [TestFixture]
    public class MailTemplateTests
    {
        private Stopwatch _stopWatch;

        private MailTemplate _sut;

        [OneTimeSetUp]
        public void TestOneTimeInitializer()
        {
            _stopWatch = Stopwatch.StartNew();
            TestContext.Progress.WriteLine("Started templates compilation");

            Assembly.Load("Shrooms.Contracts");
            Assembly.Load("Shrooms.Contracts.DataTransferObjects");
            Assembly.Load("Shrooms.Domain");

            var emailTemplatesConfig = new EmailTemplatesCompiler();
            emailTemplatesConfig.Register(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\Shrooms.Presentation.Api");

            _stopWatch.Stop();
            TestContext.Progress.WriteLine("Finished templates compilation.");
            TestContext.Progress.WriteLine($"Duration: {_stopWatch.Elapsed}");
        }

        [SetUp]
        public void TestInitializer()
        {
            _stopWatch = Stopwatch.StartNew();
            _sut = new MailTemplate();
        }

        [TearDown]
        public void TearDown()
        {
            _stopWatch.Stop();
            TestContext.Progress.WriteLine($"Duration: {_stopWatch.Elapsed}");
        }

        [Test]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(10)]
        public void Should_Generate_NewPost_EmailContent(int retries)
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

            for (var i = 0; i < retries; i++)
            {
                TestContext.Progress.WriteLine($"Generating {i + 1}/{retries}");

                _sut.Generate(newWallPostEmailTemplateViewModel, EmailTemplateCacheKeys.NewWallPost);
                _sut.Generate(kudosSentEmailTemplateViewModel, EmailTemplateCacheKeys.KudosSent);
            }
        }

        [Test]
        public void Generate_WithoutTimeZoneKey_GeneratesEmailBody()
        {
            // Arrange
            var viewModel = new EmailViewModelWithTimeZoneableProperties(DateTime.UtcNow);
            var template = CreateTemplate<EmailViewModelWithTimeZoneableProperties>($"@Model.{nameof(EmailViewModelWithTimeZoneableProperties.Date)}");
            var templateKey = CompileMockEmailTemplate<EmailViewModelWithTimeZoneableProperties>(template);

            // Act
            var result = _sut.Generate(viewModel, templateKey);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        [Test]
        public void Generate_WithTimeZoneKey_GeneratesEmailBodyWithTransformedDate()
        {
            // Arrange
            var date = DateTime.UtcNow;
            var timeZoneKey = GetLastAvailableTimeZoneKey();
            var viewModel = new EmailViewModelWithTimeZoneableProperties(date);
            var template = CreateTemplate<EmailViewModelWithTimeZoneableProperties>($"@Model.{nameof(EmailViewModelWithTimeZoneableProperties.Date)}");
            var templateKey = CompileMockEmailTemplate<EmailViewModelWithTimeZoneableProperties>(template);
            var expectedDate = ConvertUtcToTimeZoneWithoutMilliseconds(date, timeZoneKey);

            // Act
            var result = _sut.Generate(viewModel, templateKey, timeZoneKey);

            // Assert
            Assert.AreEqual(expectedDate, DateTime.Parse(result));
        }

        [Test]
        public void Generate_WithTimeZoneKeyAndWithoutMarkedProperties_GeneratesDefaultEmailBody()
        {
            // Arrange
            var date = RemoveMilliseconds(DateTime.UtcNow);
            var timeZoneKey = GetLastAvailableTimeZoneKey();
            var viewModel = new EmailViewModelWithoutTimeZoneableProperties(date);
            var template = CreateTemplate<EmailViewModelWithoutTimeZoneableProperties>($"@Model.{nameof(EmailViewModelWithoutTimeZoneableProperties.Date)}");
            var templateKey = CompileMockEmailTemplate<EmailViewModelWithoutTimeZoneableProperties>(template);

            // Act
            var result = _sut.Generate(viewModel, templateKey, timeZoneKey);

            // Assert
            Assert.AreEqual(date, DateTime.Parse(result));
        }

        [Test]
        public void Generate_WithoutTimeZoneKeys_Throws()
        {
            // Arrange
            var viewModel = new EmailViewModelWithTimeZoneableProperties(DateTime.UtcNow);
            var template = CreateTemplate<EmailViewModelWithTimeZoneableProperties>("");
            var templateKey = CompileMockEmailTemplate<EmailViewModelWithTimeZoneableProperties>(template);

            // ReSharper disable once CollectionNeverUpdated.Local
            var timeZoneKeys = new List<string>();

            // Assert
            Assert.Throws<ArgumentException>(() => _sut.Generate(viewModel, templateKey, timeZoneKeys));
        }

        [Test]
        public void Generate_WithTimeZoneKeysAndWithoutMarkedProperties_Throws()
        {
            // Arrange
            var viewModel = new EmailViewModelWithoutTimeZoneableProperties(DateTime.UtcNow);
            var template = CreateTemplate<EmailViewModelWithoutTimeZoneableProperties>("");
            var templateKey = CompileMockEmailTemplate<EmailViewModelWithoutTimeZoneableProperties>(template);
            var timeZoneKeys = new List<string> { GetLastAvailableTimeZoneKey() };

            // Assert
            Assert.Throws<ArgumentException>(() => _sut.Generate(viewModel, templateKey, timeZoneKeys));
        }

        [Test]
        public void Generate_WithTimeZoneKeys_GeneratesEmailBodyWithTransformedDate()
        {
            // Arrange
            var date = DateTime.UtcNow;
            var timeZoneKeys = new List<string> { GetLastAvailableTimeZoneKey() };
            var viewModel = new EmailViewModelWithTimeZoneableProperties(date);
            var template = CreateTemplate<EmailViewModelWithTimeZoneableProperties>($"@Model.{nameof(EmailViewModelWithTimeZoneableProperties.Date)}");
            var templateKey = CompileMockEmailTemplate<EmailViewModelWithTimeZoneableProperties>(template);
            var expectedDate = ConvertUtcToTimeZoneWithoutMilliseconds(date, timeZoneKeys[0]);

            // Act
            var result = _sut.Generate(viewModel, templateKey, timeZoneKeys);

            // Assert
            Assert.AreEqual(expectedDate, DateTime.Parse(result.Values[timeZoneKeys[0]]));
        }

        private static string CompileMockEmailTemplate<TEmailTemplate>(string template) where TEmailTemplate : BaseEmailTemplateViewModel
        {
            var templateKey = Guid.NewGuid().ToString();
            Engine.Razor.AddTemplate(templateKey, template);
            Engine.Razor.Compile(templateKey, typeof(TEmailTemplate));
            return templateKey;
        }

        private static string CreateTemplate<TEmailTemplate>(string body) where TEmailTemplate : BaseEmailTemplateViewModel
        {
            var model = $"@model {typeof(TEmailTemplate).FullName}";
            return $"{model}\n{body}";
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
