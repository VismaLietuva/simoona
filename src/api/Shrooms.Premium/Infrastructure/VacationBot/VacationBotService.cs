using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Infrastructure.VacationBot
{
    public class VacationBotService : IVacationBotService
    {
        private readonly IApplicationSettings _appSettings;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public VacationBotService(HttpClient httpClient, IApplicationSettings appSettings, ILogger logger)
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

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException e)
            {
                _logger.Error(e);
                throw new ValidationException(PremiumErrorCodes.VacationBotError, "Vacation bot error");
            }

            using (response)
            {
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error(new Exception(json));
                    throw new ValidationException(PremiumErrorCodes.VacationBotError, "Vacation bot error");
                }

                return JsonSerializer.Deserialize<VacationInfo[]>(json);
            }
        }
    }
}