using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.Constants;
using Shrooms.Domain.Services.Picture;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [Route("Picture")]
    public class PictureController : BaseController
    {
        private static readonly string[] AllowedMimeTypes =
            { "image/png", "image/gif", "image/jpeg", "image/bmp", "image/webp" };

        private readonly IPictureService _pictureService;

        public PictureController(IPictureService pictureService)
        {
            _pictureService = pictureService;
        }

        // Legacy upload: stores the uploaded file as-is. Kept until the Next.js UI fully
        // replaces the AngularJS app.
        // Remove after new UI release.
        [HttpPost]
        [Route("Upload")]
        [PermissionAuthorize(Permission = BasicPermissions.Picture)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var validationResult = ValidateUpload(file);
            if (validationResult != null)
            {
                return validationResult;
            }

            await using var stream = file.OpenReadStream();
            var pictureName = await _pictureService.UploadFromStreamAsync(
                stream,
                file.ContentType,
                file.FileName,
                GetUserAndOrganization().OrganizationId);

            return Ok(pictureName);
        }

        // Stores the upload byte-for-byte after lightweight validation. For consumers
        // that handle their own responsive sizing (e.g. Next.js with next/image),
        // where any server-side re-encode is a visible quality loss.
        [HttpPost]
        [Route("UploadOriginal")]
        [PermissionAuthorize(Permission = BasicPermissions.Picture)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadOriginal(IFormFile file)
        {
            var validationResult = ValidateUpload(file);
            if (validationResult != null)
            {
                return validationResult;
            }

            await using var stream = file.OpenReadStream();

            try
            {
                var pictureName = await _pictureService.UploadOriginalAsync(
                    stream,
                    file.ContentType,
                    file.FileName,
                    GetUserAndOrganization().OrganizationId);

                return Ok(pictureName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static IActionResult ValidateUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("No file uploaded");
            }

            if (file.Length >= WebApiConstants.MaximumPictureSizeInBytes)
            {
                return new BadRequestObjectResult("File is too large");
            }

            if (!Array.Exists(AllowedMimeTypes, t => t.Equals(file.ContentType, StringComparison.OrdinalIgnoreCase)))
            {
                return new ObjectResult("Unsupported media type") { StatusCode = StatusCodes.Status415UnsupportedMediaType };
            }

            return null;
        }
    }
}
