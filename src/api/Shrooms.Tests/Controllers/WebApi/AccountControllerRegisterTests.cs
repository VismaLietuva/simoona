using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Administration;
using Shrooms.Domain.Services.Jwt;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.RefreshTokens;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.Api.Helpers;
using Shrooms.Presentation.WebViewModels.Models;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class AccountControllerRegisterTests
    {
        private const string Tenant = "test";
        private const string Email = "jane@company.com";

        private ShroomsUserManager _userManager;
        private IOrganizationService _organizationService;
        private IAdministrationUsersService _administrationService;
        private AccountController _controller;

        [SetUp]
        public void SetUp()
        {
            _userManager = Substitute.For<ShroomsUserManager>(
                Substitute.For<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null,
                Substitute.For<ILogger<UserManager<ApplicationUser>>>(),
                Substitute.For<ICustomCache<string, IEnumerable<string>>>());

            _organizationService = Substitute.For<IOrganizationService>();
            _organizationService.GetOrganizationByNameAsync(Tenant)
                .Returns(Task.FromResult(new Organization { Id = 1, ShortName = Tenant, AuthenticationProviders = "Internal,Google" }));
            _organizationService.IsOrganizationHostValidAsync(Arg.Any<string>(), Tenant).Returns(Task.FromResult(true));

            _administrationService = Substitute.For<IAdministrationUsersService>();
            _administrationService.UserIsSoftDeletedAsync(Arg.Any<string>()).Returns(Task.FromResult(false));
            _administrationService.CreateNewUserAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Tenant)
                .Returns(Task.FromResult(IdentityResult.Success));

            var mapper = Substitute.For<IMapper>();
            mapper.Map<ApplicationUser>(Arg.Any<RegisterViewModel>()).Returns(new ApplicationUser { Email = Email, UserName = Email });

            _controller = new AccountController(
                mapper,
                _userManager,
                Substitute.For<IPermissionService>(),
                _organizationService,
                Substitute.For<IRefreshTokenService>(),
                _administrationService,
                Substitute.For<IApplicationSettings>(),
                Substitute.For<IJwtTokenService>(),
                Substitute.For<IReturnUrlValidator>(),
                Substitute.For<IAuthenticationSchemeProvider>(),
                Substitute.For<ILogger<AccountController>>());

            var httpContext = new DefaultHttpContext();
            httpContext.Items["tenantName"] = Tenant;
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        [Test]
        public async Task NewAddress_CreatesUser()
        {
            _userManager.FindByEmailAsync(Email).Returns(Task.FromResult<ApplicationUser>(null));

            var result = await _controller.RegisterUser(Model());

            Assert.That(result, Is.InstanceOf<OkResult>());
            await _administrationService.Received(1).CreateNewUserAsync(Arg.Any<ApplicationUser>(), "Str0ngPassw0rd", Tenant);
        }

        [Test]
        public async Task ExistingUnconfirmedInternalAccount_TakesNewPasswordAndResendsVerification()
        {
            // Setting the password rotates the security stamp, which invalidates every earlier verification
            // link; whoever reads the mailbox then confirms only the latest registrant's password.
            var existing = new ApplicationUser { Id = "u1", Email = Email, EmailConfirmed = false };
            _userManager.FindByEmailAsync(Email).Returns(Task.FromResult(existing));
            _userManager.HasPasswordAsync(existing).Returns(Task.FromResult(true));
            _userManager.RemovePasswordAsync(existing).Returns(Task.FromResult(IdentityResult.Success));
            _userManager.AddPasswordAsync(existing, "Str0ngPassw0rd").Returns(Task.FromResult(IdentityResult.Success));
            _administrationService.HasExistingExternalLoginAsync(Email, AuthenticationConstants.InternalLoginProvider).Returns(Task.FromResult(true));

            var result = await _controller.RegisterUser(Model());

            Assert.That(result, Is.InstanceOf<OkResult>());
            await _userManager.Received(1).RemovePasswordAsync(existing);
            await _userManager.Received(1).AddPasswordAsync(existing, "Str0ngPassw0rd");
            await _administrationService.Received(1).SendUserVerificationEmailAsync(existing, Tenant);
            await _administrationService.DidNotReceiveWithAnyArgs().CreateNewUserAsync(default, default, default);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task WeakPassword_GetsTheSameResponse_WhetherOrNotTheAddressExists(bool addressExists)
        {
            var validator = Substitute.For<IPasswordValidator<ApplicationUser>>();
            validator.ValidateAsync(_userManager, Arg.Any<ApplicationUser>(), "weak")
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityErrorDescriber().PasswordRequiresDigit())));
            _userManager.PasswordValidators.Add(validator);
            _userManager.FindByEmailAsync(Email).Returns(Task.FromResult(addressExists ? new ApplicationUser { Id = "u1", Email = Email, EmailConfirmed = true } : null));

            var model = Model();
            model.Password = "weak";
            model.ConfirmPassword = "weak";

            var result = await _controller.RegisterUser(model);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            await _userManager.DidNotReceiveWithAnyArgs().FindByEmailAsync(default);
            await _administrationService.DidNotReceiveWithAnyArgs().CreateNewUserAsync(default, default, default);
            await _administrationService.DidNotReceiveWithAnyArgs().SendUserVerificationEmailAsync(default, default);
        }

        [Test]
        public async Task ExistingConfirmedAccount_AnswersOkWithoutAnyChange()
        {
            var existing = new ApplicationUser { Id = "u1", Email = Email, EmailConfirmed = true };
            _userManager.FindByEmailAsync(Email).Returns(Task.FromResult(existing));

            var result = await _controller.RegisterUser(Model());

            Assert.That(result, Is.InstanceOf<OkResult>(), "must not reveal that the address is registered");
            await _administrationService.DidNotReceiveWithAnyArgs().SendUserVerificationEmailAsync(default, default);
            await _userManager.DidNotReceiveWithAnyArgs().RemovePasswordAsync(default);
            await _administrationService.DidNotReceiveWithAnyArgs().CreateNewUserAsync(default, default, default);
        }

        [Test]
        public async Task SoftDeletedAccount_IsNotRestored()
        {
            _userManager.FindByEmailAsync(Email).Returns(Task.FromResult<ApplicationUser>(null));
            _administrationService.UserIsSoftDeletedAsync(Email).Returns(Task.FromResult(true));

            var result = await _controller.RegisterUser(Model());

            Assert.That(result, Is.InstanceOf<OkResult>());
            await _administrationService.DidNotReceiveWithAnyArgs().RestoreUserAsync(default);
            await _administrationService.DidNotReceiveWithAnyArgs().CreateNewUserAsync(default, default, default);
        }

        [Test]
        public async Task EmailHostNotAllowed_IsRejected()
        {
            _organizationService.IsOrganizationHostValidAsync(Email, Tenant).Returns(Task.FromResult(false));

            var result = await _controller.RegisterUser(Model());

            AssertError(result, "email_host_not_allowed");
            await _administrationService.DidNotReceiveWithAnyArgs().CreateNewUserAsync(default, default, default);
        }

        [Test]
        public async Task InternalProviderDisabled_IsRejected()
        {
            _organizationService.GetOrganizationByNameAsync(Tenant)
                .Returns(Task.FromResult(new Organization { Id = 1, ShortName = Tenant, AuthenticationProviders = "Google" }));

            var result = await _controller.RegisterUser(Model());

            AssertError(result, "internal_registration_disabled");
            await _userManager.DidNotReceiveWithAnyArgs().FindByEmailAsync(default);
        }

        private static RegisterViewModel Model()
        {
            return new RegisterViewModel
            {
                FirstName = "Jane",
                LastName = "Doe",
                UserName = Email,
                Email = Email,
                Password = "Str0ngPassw0rd",
                ConfirmPassword = "Str0ngPassw0rd"
            };
        }

        private static void AssertError(IActionResult result, string expectedError)
        {
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var value = ((BadRequestObjectResult)result).Value;
            var error = value.GetType().GetProperty("error")?.GetValue(value) as string;
            Assert.That(error, Is.EqualTo(expectedError));
        }
    }
}
