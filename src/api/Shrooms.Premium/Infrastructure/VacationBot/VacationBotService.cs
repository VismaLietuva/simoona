using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Infrastructure.VacationBot
{
    public class VacationBotService : IVacationBotService
    {
        private readonly IApplicationSettings _appSettings;
        private readonly ILogger<VacationBotService> _logger;
        private readonly HttpClient _httpClient;

        public VacationBotService(HttpClient httpClient, IApplicationSettings appSettings, ILogger<VacationBotService> logger)
        {
            _httpClient = httpClient;
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task<VacationInfo[]> GetVacationHistory(string email)
        {
            var url = string.Format(_appSettings.VacationsBotHistoryUrl, email);

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic", _appSettings.VacationsBotAuthToken);

            try
            {
                using var response = await _httpClient.SendAsync(request);

                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Vacation bot returned error response: {ResponseBody}", json);
                    throw new ValidationException(PremiumErrorCodes.VacationBotError, "Vacation bot error");
                }

                return JsonSerializer.Deserialize<VacationInfo[]>(json);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, e.Message);
                throw new ValidationException(PremiumErrorCodes.VacationBotError, "Vacation bot error");
            }
        }
    }
}
