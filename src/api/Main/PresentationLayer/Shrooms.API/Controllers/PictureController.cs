using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Shrooms.API.Filters;
using Shrooms.Constants.WebApi;
using Shrooms.Domain.Services.Picture;
using Shrooms.Host.Contracts.Constants;

namespace Shrooms.API.Controllers
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

            if (imageContent.Headers.ContentLength >= WebApiConstants.MaximumPictureSizeInBytes)
            {
                return BadRequest("File is too large");
            }

            Image imgObj;
            var imageStream = await provider.Contents[0].ReadAsStreamAsync();

            try
            {
                imgObj = Image.FromStream(imageStream);
            }
            catch (ArgumentException)
            {
                return UnsupportedMediaType();
            }

            if (imgObj.RawFormat.Guid != ImageFormat.Png.Guid &&
                imgObj.RawFormat.Guid != ImageFormat.Gif.Guid &&
                imgObj.RawFormat.Guid != ImageFormat.Jpeg.Guid &&
                imgObj.RawFormat.Guid != ImageFormat.Bmp.Guid)
            {
                return UnsupportedMediaType();
            }

            imageStream.Position = 0;

            var pictureName = await _pictureService
                .UploadFromStream(
                    imageStream,
                    imageContent.Headers.ContentType.ToString(),
                    imageContent.Headers.ContentDisposition.FileName.Replace("\"", string.Empty),
                    GetUserAndOrganization().OrganizationId);

            return Ok(pictureName);
        }
    }
}