using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Infrastructure.Email;
using System;

namespace Shrooms.Tests.Infrastructure
{
    [TestFixture]
    public class EmailTemplateCompilerTests
    {
        private IMailTemplateCache _mailTemplateCache;
        private IEmailTemplateConfiguration _emailTemplateConfiguration;

        private EmailTemplateCompiler _sut;

        [SetUp]
        public void TestInitializer()
        {
            _mailTemplateCache = Substitute.For<IMailTemplateCache>();
            _emailTemplateConfiguration = Substitute.ForPartsOf<EmailTemplateConfiguration>();

            _emailTemplateConfiguration.BaseDirectory
                .Returns(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\Shrooms.Presentation.Api");

            _sut = new EmailTemplateCompiler(_mailTemplateCache, _emailTemplateConfiguration);
        }

        [Test]
        public void Register_WhenCompiling_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _sut.Register());
        }
    }
}
