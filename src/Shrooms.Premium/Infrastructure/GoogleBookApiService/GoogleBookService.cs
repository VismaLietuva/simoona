using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Books.v1;
using Google.Apis.Services;
using Shrooms.Premium.DataTransferObjects;

namespace Shrooms.Premium.Infrastructure.GoogleBookApiService
{
    public class GoogleBookService : IBookInfoService
    {
        private BooksService _service;

        public GoogleBookService()
        {
            _service = new BooksService(new BaseClientService.Initializer
            {
                ApiKey = ConfigurationManager.AppSettings["GoogleAccountApiKey"]
            });
        }

        public async Task<ExternalBookInfo> FindBookByIsbnAsync(string isbn)
        {
            var query = $"isbn:{isbn}";

            var result = await _service.Volumes.List(query).ExecuteAsync();
            if (result.Items == null)
            {
                return null;
            }

            var volume = result.Items.First().VolumeInfo;
            var bookInfo = new ExternalBookInfo
            {
                Author = volume.Authors == null ? "Authors not set" : string.Join(", ", volume.Authors),
                Title = volume.Title,
                Url = volume.InfoLink,
                CoverImageUrl = volume.ImageLinks?.Thumbnail
            };

            return bookInfo;
        }
    }
}
