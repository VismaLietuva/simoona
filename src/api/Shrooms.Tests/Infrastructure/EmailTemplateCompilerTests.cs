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
        private string _baseDirectory;

        private EmailTemplateCompiler _sut;

        [SetUp]
        public void TestInitializer()
        {
            _mailTemplateCache = Substitute.For<IMailTemplateCache>();
            _baseDirectory = AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\Shrooms.Presentation.Api";

            _sut = new EmailTemplateCompiler(_mailTemplateCache, _baseDirectory);
        }

        [Test]
        public void Register_WhenCompiling_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _sut.Register());
        }
    }
}
