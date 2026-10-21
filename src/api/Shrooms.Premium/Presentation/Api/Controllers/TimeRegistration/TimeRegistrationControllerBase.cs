using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Presentation.Common.Controllers;

namespace Shrooms.Premium.Presentation.Api.Controllers.TimeRegistration
{
    public abstract class TimeRegistrationControllerBase : BaseController
    {
        protected async Task<IActionResult> GuardedAsync<T>(Func<Task<T>> action)
        {
            try
            {
                return Ok(await action());
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
            catch (UnauthorizedException)
            {
                return Forbidden();
            }
        }

        protected async Task<IActionResult> GuardedAsync(Func<Task> action)
        {
            try
            {
                await action();
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
            catch (UnauthorizedException)
            {
                return Forbidden();
            }
        }
    }
}
