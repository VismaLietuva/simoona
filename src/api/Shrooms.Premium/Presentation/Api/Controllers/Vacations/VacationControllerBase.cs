using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Domain.DomainExceptions.Vacation;
using Shrooms.Presentation.Common.Controllers;

namespace Shrooms.Premium.Presentation.Api.Controllers.Vacations
{
    /// <summary>
    /// Wider than <see cref="BaseController.BadRequestWithError"/>: alongside the
    /// translated sentence it carries the stable machine code and the values that
    /// sentence interpolates, so the client can render its own copy of the same
    /// message. The two original fields are still there, so a caller reading only
    /// ErrorMessage is unaffected.
    /// </summary>
    public abstract class VacationControllerBase : BaseController
    {
        protected async Task<IActionResult> GuardedAsync<T>(Func<Task<T>> action)
        {
            try
            {
                return Ok(await action());
            }
            catch (VacationValidationException e)
            {
                return Refused(e);
            }
        }

        protected async Task<IActionResult> GuardedFileAsync(Func<Task<VacationDocumentDto>> action)
        {
            try
            {
                var document = await action();
                return File(document.Content, document.ContentType, document.FileName);
            }
            catch (VacationValidationException e)
            {
                return Refused(e);
            }
        }

        private BadRequestObjectResult Refused(VacationValidationException e)
        {
            return BadRequest(new
            {
                ErrorCode = e.ErrorCode,
                ErrorMessage = e.Message,
                Code = e.Code,
                Params = e.Parameters
            });
        }
    }
}
