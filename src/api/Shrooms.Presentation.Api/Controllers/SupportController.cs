using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.Support;
using Shrooms.Domain.Services.Support;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.WebViewModels.Models.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    public class SupportController : BaseController
    {
        // Mirrors PictureController's allow-list; the screenshot is emailed, not stored.
        private static readonly string[] AllowedImageMimeTypes =
            { "image/png", "image/gif", "image/jpeg", "image/bmp", "image/webp" };

        private readonly IMapper _mapper;
        private readonly ISupportService _supportService;

        public SupportController(IMapper mapper, ISupportService supportService)
        {
            _mapper = mapper;
            _supportService = supportService;
        }

        [PermissionAuthorize(Permission = BasicPermissions.Support)]
        [HttpPost]
        public async Task<IActionResult> SubmitTicket([FromForm] SupportPostViewModel support)
        {
            var maxSupportTypeIndex = Enum.GetValues(typeof(SupportType)).Cast<int>().Max();

            if (!ModelState.IsValid || maxSupportTypeIndex < support.Type)
            {
                return BadRequest();
            }

            var supportDto = _mapper.Map<SupportPostViewModel, SupportDto>(support);

            if (support.Image != null && support.Image.Length > 0)
            {
                var imageValidationResult = ValidateImage(support.Image);
                if (imageValidationResult != null)
                {
                    return imageValidationResult;
                }

                supportDto.Attachment = await ReadAttachmentAsync(support.Image);
            }

            await _supportService.SubmitTicketAsync(GetUserAndOrganization(), supportDto);

            return StatusCode(201);
        }

        private static IActionResult ValidateImage(IFormFile image)
        {
            if (image.Length >= WebApiConstants.MaximumPictureSizeInBytes)
            {
                return new BadRequestObjectResult("Image is too large");
            }

            if (!Array.Exists(AllowedImageMimeTypes, type => type.Equals(image.ContentType, StringComparison.OrdinalIgnoreCase)))
            {
                return new ObjectResult("Unsupported media type") { StatusCode = StatusCodes.Status415UnsupportedMediaType };
            }

            return null;
        }

        // Buffered rather than streamed: the mail attachment is built after this
        // action returns, by which point ASP.NET Core has disposed the form file.
        private static async Task<SupportAttachmentDto> ReadAttachmentAsync(IFormFile image)
        {
            using var buffer = new MemoryStream();
            await image.CopyToAsync(buffer);

            return new SupportAttachmentDto
            {
                Content = buffer.ToArray(),
                FileName = Path.GetFileName(image.FileName),
                ContentType = image.ContentType
            };
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Support)]
        public IEnumerable<SupportType> GetSupportTypes()
        {
            return Enum.GetValues(typeof(SupportType)).Cast<SupportType>();
        }
    }
}
