using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.Emoji;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Domain.Services.Emoji;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.WebViewModels.Models.Emoji;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [Route("Emoji")]
    public class EmojiController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ICustomEmojiService _customEmojiService;

        public EmojiController(IMapper mapper, ICustomEmojiService customEmojiService)
        {
            _mapper = mapper;
            _customEmojiService = customEmojiService;
        }

        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> List()
        {
            var result = await _customEmojiService.GetAllAsync(GetUserAndOrganization(), GetOrganizationName());

            var etag = $"\"{result.ETag}\"";
            Response.Headers.ETag = etag;
            Response.Headers.CacheControl = "private";

            if (Request.Headers.IfNoneMatch.Contains(etag))
            {
                return StatusCode(304);
            }

            return Ok(_mapper.Map<IEnumerable<CustomEmojiDto>, IEnumerable<CustomEmojiViewModel>>(result.Emojis));
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = BasicPermissions.CustomEmoji)]
        public async Task<IActionResult> Create([FromForm] string name, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            if (file.Length > WebApiConstants.MaximumCustomEmojiSizeInBytes)
            {
                return BadRequest($"File is too large. Maximum size is {WebApiConstants.MaximumCustomEmojiSizeInBytes / 1024} KB");
            }

            if (!Array.Exists(WebApiConstants.AllowedCustomEmojiContentTypes, t => t.Equals(file.ContentType, StringComparison.OrdinalIgnoreCase)))
            {
                return StatusCode(415, $"Unsupported media type. Allowed types: {string.Join(", ", WebApiConstants.AllowedCustomEmojiContentTypes)}");
            }

            await using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            var newEmojiDto = new NewCustomEmojiDto
            {
                Name = name,
                Content = stream,
                MimeType = file.ContentType,
                FileName = file.FileName
            };

            try
            {
                var emoji = await _customEmojiService.CreateAsync(newEmojiDto, GetUserAndOrganization(), GetOrganizationName());

                return Ok(_mapper.Map<CustomEmojiDto, CustomEmojiViewModel>(emoji));
            }
            catch (ValidationException e)
            {
                if (e.ErrorCode == ErrorCodes.DuplicatesIntolerable)
                {
                    return Conflict(new { e.ErrorCode, e.ErrorMessage });
                }

                return BadRequestWithError(e);
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        [PermissionAuthorize(Permission = BasicPermissions.CustomEmoji)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _customEmojiService.DeleteAsync(id, GetUserAndOrganization());

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
