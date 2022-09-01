using System;
using System.Collections.Generic;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using RazorEngineCore;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateDtos;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Templates;
using Shrooms.Infrastructure.Extensions;

namespace Shrooms.Tests.Infrastructure
{
    [TestFixture]
    public class MailTemplateTests
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

            var template = new NewWallPostEmailTemplateViewModel("Value", "Value", "Value", "Value", "Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_NewMentionTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<NewMentionTemplateViewModel>(_emailTemplateConfiguration.NewMentionTemplateAbsolutePath);

            _mailTemplateCache.Get<NewMentionTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new NewMentionTemplateViewModel("Value", "Value", "Value", "Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }


        [Test]
        public void Run_KudosRejectedEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<KudosRejectedEmailTemplateViewModel>(_emailTemplateConfiguration.KudosRejectedEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<KudosRejectedEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new KudosRejectedEmailTemplateViewModel("Value", "Value", 10, "Value", "Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_KudosSentEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<KudosSentEmailTemplateViewModel>(_emailTemplateConfiguration.KudosSentEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<KudosSentEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new KudosSentEmailTemplateViewModel("Value", "Value", 10, "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_KudosReceivedEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<KudosReceivedEmailTemplateViewModel>(_emailTemplateConfiguration.KudosReceivedEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<KudosReceivedEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new KudosReceivedEmailTemplateViewModel("Value", 10, "Value", "Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_KudosDecreasedEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<KudosDecreasedEmailTemplateViewModel>(_emailTemplateConfiguration.KudosDecreasedEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<KudosDecreasedEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new KudosDecreasedEmailTemplateViewModel("Value", 10, "Value", "Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_BirthdaysNotificationTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<BirthdaysNotificationTemplateViewModel>(_emailTemplateConfiguration.BirthdaysNotificationAbsolutePath);

            _mailTemplateCache.Get<BirthdaysNotificationTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new BirthdaysNotificationTemplateViewModel(
                new List<BirthdaysNotificationEmployeeViewModel>(),
                "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_UserConfirmationEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<UserConfirmationEmailTemplateViewModel>(_emailTemplateConfiguration.UserConfirmationEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<UserConfirmationEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new UserConfirmationEmailTemplateViewModel("Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_NotificationAboutNewUserEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<NotificationAboutNewUserEmailTemplateViewModel>(_emailTemplateConfiguration.NotificationAboutNewUserEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<NotificationAboutNewUserEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new NotificationAboutNewUserEmailTemplateViewModel("Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_NewCommentEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<NewCommentEmailTemplateViewModel>(_emailTemplateConfiguration.NewCommentEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<NewCommentEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new NewCommentEmailTemplateViewModel("Value", "Value", "Value", "Value", "Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_ResetPasswordTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<ResetPasswordTemplateViewModel>(_emailTemplateConfiguration.ResetPasswordTemplateAbsolutePath);

            _mailTemplateCache.Get<ResetPasswordTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new ResetPasswordTemplateViewModel("Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_VerifyEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<VerifyEmailTemplateViewModel>(_emailTemplateConfiguration.VerifyEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<VerifyEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new VerifyEmailTemplateViewModel("Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_BookReminderEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<BookReminderEmailTemplateViewModel>(_emailTemplateConfiguration.BookReminderEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<BookReminderEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new BookReminderEmailTemplateViewModel("Value", "Value", "Value", "Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_BookReportEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<BookReportEmailTemplateViewModel>(_emailTemplateConfiguration.BookReportEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<BookReportEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new BookReportEmailTemplateViewModel("Value", "Value", "Value", "Value", "Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_EventParticipantExpelledEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<EventParticipantExpelledEmailTemplateViewModel>(_emailTemplateConfiguration.EventParticipantExpelledEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<EventParticipantExpelledEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new EventParticipantExpelledEmailTemplateViewModel("Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_EventJoinRemindEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<EventJoinRemindEmailTemplateViewModel>(_emailTemplateConfiguration.EventJoinRemindEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<EventJoinRemindEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new EventJoinRemindEmailTemplateViewModel("Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_CoacheeJoinedEventEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<CoacheeJoinedEventEmailTemplateViewModel>(
                _emailTemplateConfiguration.CoacheeJoinedEventEmailTemplateAbsolutePath,
                builder => builder.AddUsing(nameof(System)));

            _mailTemplateCache.Get<CoacheeJoinedEventEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var eventStatusDto = new UserEventAttendStatusChangeEmailDto
            {
                FirstName = "FirstName",
                LastName = "LastName",
                OrganizationId = 1,
                ManagerId = "ManagerId",
                ManagerEmail = "ManagerEmail",
                EventName = "EventName",
                EventId = Guid.NewGuid(),
                EventStartDate = DateTime.UtcNow,
                EventEndDate = DateTime.UtcNow
            };

            var template = new CoacheeJoinedEventEmailTemplateViewModel("Value", eventStatusDto, "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_CoacheeLeftEventEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<CoacheeLeftEventEmailTemplateViewModel>(_emailTemplateConfiguration.CoacheeLeftEventEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<CoacheeLeftEventEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var eventStatusDto = new UserEventAttendStatusChangeEmailDto
            {
                FirstName = "FirstName",
                LastName = "LastName",
                OrganizationId = 1,
                ManagerId = "ManagerId",
                ManagerEmail = "ManagerEmail",
                EventName = "EventName",
                EventId = Guid.NewGuid(),
                EventStartDate = DateTime.UtcNow,
                EventEndDate = DateTime.UtcNow
            };

            var template = new CoacheeLeftEventEmailTemplateViewModel("Value", eventStatusDto, "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_BookTakenEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<BookTakenEmailTemplateViewModel>(_emailTemplateConfiguration.BookTakenEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<BookTakenEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new BookTakenEmailTemplateViewModel("Value", "Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_LoyaltyKudosReceivedEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<LoyaltyKudosReceivedEmailTemplateViewModel>(_emailTemplateConfiguration.LoyaltyKudosReceivedEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<LoyaltyKudosReceivedEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new LoyaltyKudosReceivedEmailTemplateViewModel("Value", 10, "Value", "Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_LoyaltyKudosDecreasedEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<LoyaltyKudosDecreasedEmailTemplateViewModel>(_emailTemplateConfiguration.LoyaltyKudosDecreasedEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<LoyaltyKudosDecreasedEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new LoyaltyKudosDecreasedEmailTemplateViewModel("Value", 10, "Value", "Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Run_CommitteeSuggestionEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<CommitteeSuggestionEmailTemplateViewModel>(_emailTemplateConfiguration.CommitteeSuggestionEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<CommitteeSuggestionEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new CommitteeSuggestionEmailTemplateViewModel("Value", "Value", "Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }


        [Test]
        public void Run_ServiceRequestCommentEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<ServiceRequestCommentEmailTemplateViewModel>(_emailTemplateConfiguration.ServiceRequestCommentEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<ServiceRequestCommentEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new CommitteeSuggestionEmailTemplateViewModel("Value", "Value", "Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }


        [Test]
        public void Run_ServiceRequestEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<ServiceRequestEmailTemplateViewModel>(_emailTemplateConfiguration.ServiceRequestEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<ServiceRequestEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new ServiceRequestEmailTemplateViewModel("Value", "Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }


        [Test]
        public void Run_ServiceRequestUpdateEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<ServiceRequestUpdateEmailTemplateViewModel>(_emailTemplateConfiguration.ServiceRequestUpdateEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<ServiceRequestUpdateEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var template = new ServiceRequestUpdateEmailTemplateViewModel("Value", "Value", "Value", "Value", "Value");

            // Act
            var result = _sut.Generate(template);

            // Assert
            Assert.IsNotNull(result);
        }


        [Test]
        public void Run_StartedLotteryEmailTemplateViewModel_ReturnsCompiledTemplate()
        {
            // Arrange
            var compiledTemplate = SetUpCompiledTemplateFor<StartedLotteryEmailTemplateViewModel>(_emailTemplateConfiguration.StartedLotteryEmailTemplateAbsolutePath);

            _mailTemplateCache.Get<StartedLotteryEmailTemplateViewModel>()
                .Returns(compiledTemplate);

            var lotteryStartedDto = new LotteryStartedEmailDto
            {
                Id = 1,
                Title = "Title",
                Description = "Description",
                EntryFee = 10,
                EndDate = DateTime.UtcNow,
            };

            var template = new StartedLotteryEmailTemplateViewModel(lotteryStartedDto, "Value", DateTime.UtcNow, "Value");

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
