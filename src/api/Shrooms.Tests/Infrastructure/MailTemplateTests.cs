using System;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using RazorEngineCore;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Templates;
using Shrooms.Infrastructure.Extensions;

namespace Shrooms.Tests.Infrastructure
{
    internal class MailTemplateTests
    {
        private IRazorEngine _razorEngine;
        private IMailTemplateCache _mailTemplateCache;
        private IEmailTemplateConfiguration _emailTemplateConfiguration;
        private IRazorEngineCompiledTemplate<EmailTemplateBase<LayoutEmailTemplateViewModel>> _compiledLayoutTemplate;

        private MailTemplate _sut;

        [SetUp]
        public void TestInitializer()
        {
            _razorEngine = new RazorEngine();
            _emailTemplateConfiguration = Substitute.ForPartsOf<EmailTemplateConfiguration>();

            _emailTemplateConfiguration.BaseDirectory
                .Returns(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\Shrooms.Presentation.Api");

            _mailTemplateCache = Substitute.For<IMailTemplateCache>();

            _compiledLayoutTemplate = SetUpCompiledLayoutTemplate();

            _sut = new MailTemplate(_mailTemplateCache);
        }

        [Test]
        public void Run_NewWallPostEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<NewWallPostEmailTemplateViewModel>(_emailTemplateConfiguration.NewWallPostEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<NewWallPostEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new NewWallPostEmailTemplateViewModel(
                "Title",
                "PictureUrl",
                "FullName",
                "PostDeepLink",
                "MessageBody",
                "Title",
                "SettingsUrl");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        private ICompiledEmailTemplate SetUpCompiledTemplateFor<T>(string aboslutePath, Action<IRazorEngineCompilationOptionsBuilder> builder = null)
            where T : BaseEmailTemplateViewModel
        {
            var emailTemplate = File.ReadAllText(aboslutePath);

            return _razorEngine.Compile<T>(
                emailTemplate,
                _compiledLayoutTemplate,
                builder);
        }

        private IRazorEngineCompiledTemplate<EmailTemplateBase<LayoutEmailTemplateViewModel>> SetUpCompiledLayoutTemplate()
        {
            return _razorEngine.Compile<EmailTemplateBase<LayoutEmailTemplateViewModel>>(
                File.ReadAllText(_emailTemplateConfiguration.LayoutEmailTemplateAbsolutePath));
        }
    }
}
