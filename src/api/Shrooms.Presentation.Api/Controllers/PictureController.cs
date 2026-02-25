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
    public class PictureController : BaseController
    {
        private readonly IPictureService _pictureService;

        public PictureController(IPictureService pictureService)
        {
            _pictureService = pictureService;
        }

        [HttpPost]
        [Route("Picture/Upload")]
        [PermissionAuthorize(Permission = BasicPermissions.Picture)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            if (file.Length >= WebApiConstants.MaximumPictureSizeInBytes)
            {
                return BadRequest("File is too large");
            }

            var allowedTypes = new[] { "image/png", "image/gif", "image/jpeg", "image/bmp" };
            if (!Array.Exists(allowedTypes, t => t.Equals(file.ContentType, StringComparison.OrdinalIgnoreCase)))
            {
                return StatusCode(415, "Unsupported media type");
            }

            await using var stream = file.OpenReadStream();
            var pictureName = await _pictureService.UploadFromStreamAsync(
                stream,
                file.ContentType,
                file.FileName,
                GetUserAndOrganization().OrganizationId);

            return Ok(pictureName);
        }
    }
}
