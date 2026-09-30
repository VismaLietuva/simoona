using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Jwt;
using Shrooms.Presentation.Api.Controllers;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class TokenControllerLockoutTests
    {
        private const string Password = "Correct-Horse-1";

        private ShroomsUserManager _userManager;
        private IJwtTokenService _jwtTokenService;
        private TokenController _controller;
        private ApplicationUser _user;

        [SetUp]
        public void SetUp()
        {
            _user = new ApplicationUser { Id = "user-1", UserName = "jane", Email = "jane@test.lt", EmailConfirmed = true };

            _userManager = Substitute.For<ShroomsUserManager>(
                Substitute.For<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null,
                Substitute.For<ILogger<UserManager<ApplicationUser>>>(),
                Substitute.For<ICustomCache<string, IEnumerable<string>>>());
            _userManager.SupportsUserLockout.Returns(true);
            _userManager.FindByNameAsync("jane").Returns(Task.FromResult(_user));
            _userManager.FindByNameAsync("nobody").Returns(Task.FromResult<ApplicationUser>(null));
            _userManager.FindByEmailAsync("nobody").Returns(Task.FromResult<ApplicationUser>(null));
            _userManager.IsLockedOutAsync(_user).Returns(Task.FromResult(false));
            _userManager.CheckPasswordAsync(_user, Password).Returns(Task.FromResult(true));
            _userManager.CheckPasswordAsync(_user, Arg.Is<string>(p => p != Password)).Returns(Task.FromResult(false));
            _userManager.AccessFailedAsync(_user).Returns(Task.FromResult(IdentityResult.Success));
            _userManager.ResetAccessFailedCountAsync(_user).Returns(Task.FromResult(IdentityResult.Success));

            _jwtTokenService = Substitute.For<IJwtTokenService>();
            _jwtTokenService.GenerateTokenAsync(_user).Returns(Task.FromResult(new JwtTokenResult("jwt", 3600)));

            _controller = new TokenController(_userManager, _jwtTokenService, Substitute.For<ILogger<TokenController>>());
        }

        [Test]
        public async Task WrongPassword_RecordsFailedAttempt_AndReturnsInvalidGrant()
        {
            var result = await Post("jane", "wrong");

            await _userManager.Received(1).AccessFailedAsync(_user);
            await _userManager.DidNotReceive().ResetAccessFailedCountAsync(_user);
            AssertError(result, "invalid_grant");
        }

        [Test]
        public async Task LockedOutUser_IsRefusedBeforePasswordCheck()
        {
            _userManager.IsLockedOutAsync(_user).Returns(Task.FromResult(true));

            var result = await Post("jane", Password);

            await _userManager.DidNotReceive().CheckPasswordAsync(_user, Arg.Any<string>());
            await _jwtTokenService.DidNotReceiveWithAnyArgs().GenerateTokenAsync(default);
            AssertError(result, "account_locked");
        }

        [Test]
        public async Task CorrectPassword_ResetsFailureCounter_AndIssuesToken()
        {
            var result = await Post("jane", Password);

            await _userManager.Received(1).ResetAccessFailedCountAsync(_user);
            await _userManager.DidNotReceive().AccessFailedAsync(_user);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UnknownUser_ReturnsSameErrorAsWrongPassword()
        {
            var result = await Post("nobody", "whatever");

            AssertError(result, "invalid_grant");
            await _userManager.DidNotReceiveWithAnyArgs().AccessFailedAsync(default);
        }

        [Test]
        public async Task WrongPassword_WhenStoreHasNoLockout_StillReturnsInvalidGrant()
        {
            _userManager.SupportsUserLockout.Returns(false);

            var result = await Post("jane", "wrong");

            await _userManager.DidNotReceiveWithAnyArgs().AccessFailedAsync(default);
            await _userManager.DidNotReceiveWithAnyArgs().IsLockedOutAsync(default);
            AssertError(result, "invalid_grant");
        }

        private async Task<IActionResult> Post(string userName, string password)
        {
            var body = $"grant_type=password&username={userName}&password={System.Net.WebUtility.UrlEncode(password)}";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "POST";
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            return await _controller.Token();
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
