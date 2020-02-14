using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Infrastructure.Email.Templating;

namespace Shrooms.Tests.Infrastructure
{
    internal class MailTemplateTests
    {
        private Stopwatch _stopWatch;

        [OneTimeSetUp]
        public void TestOneTimeInitializer()
        {
            _stopWatch = Stopwatch.StartNew();
            TestContext.Progress.WriteLine("Started templates compilation");

            Assembly.Load("Shrooms.Contracts");
            Assembly.Load("Shrooms.Contracts.DataTransferObjects");
            Assembly.Load("Shrooms.Domain");

            EmailTemplatesConfig.Register(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\Shrooms.Presentation.Api");

            _stopWatch.Stop();
            TestContext.Progress.WriteLine("Finished templates compilation.");
            TestContext.Progress.WriteLine($"Duration: {_stopWatch.Elapsed}");
        }

        [SetUp]
        public void TestInitializer()
        {
            _stopWatch = Stopwatch.StartNew();
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
            var mailTemplate = new MailTemplate();

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

                mailTemplate.Generate(newWallPostEmailTemplateViewModel, EmailTemplateCacheKeys.NewWallPost);
                mailTemplate.Generate(kudosSentEmailTemplateViewModel, EmailTemplateCacheKeys.KudosSent);
            }
        }
    }
}
