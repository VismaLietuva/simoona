using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Shrooms.API.Filters;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.WebApi;
using Shrooms.Domain.Services.Picture;

namespace Shrooms.API.Controllers.WebApi
{
    [Authorize]
    public class PictureController : BaseController
    {
        private readonly IPictureService _pictureService;

        public PictureController(IPictureService pictureService)
        {
            _pictureService = pictureService;
        }

        [PermissionAuthorize(Permission = BasicPermissions.Picture)]
        public async Task<IHttpActionResult> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
            var imageContent = provider.Contents[0];

            if (imageContent.Headers.ContentLength >= ConstWebApi.MaximumPictureSizeInBytes)
            {
                return BadRequest("File is too large");
            }

            Image image;
            var imageStream = await provider.Contents[0].ReadAsStreamAsync();

            try
            {
                image = Image.FromStream(imageStream);
            }
            catch (ArgumentException)
            {
                return UnsupportedMediaType();
            }

            if (image.RawFormat.Guid != ImageFormat.Png.Guid && image.RawFormat.Guid != ImageFormat.Gif.Guid
                    && image.RawFormat.Guid != ImageFormat.Jpeg.Guid && image.RawFormat.Guid != ImageFormat.Bmp.Guid)
            {
                return UnsupportedMediaType();
            }

            imageStream.Position = 0;

            var pictureName = await _pictureService.UploadFromImage(image,
                    imageContent.Headers.ContentType.ToString(),
                    imageContent.Headers.ContentDisposition.FileName.Replace("\"", string.Empty),
                    GetUserAndOrganization().OrganizationId);

            return Ok(pictureName);
        }
    }
}