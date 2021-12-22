using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Infrastructure.VacationBot
{
    public class VacationBotService : IVacationBotService
    {
        private readonly IApplicationSettings _appSettings;
        private readonly ILogger _logger;

        public VacationBotService(IApplicationSettings appSettings, ILogger logger)
        {
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task<VacationInfo[]> GetVacationHistory(string email)
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage response;

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic", _appSettings.VacationsBotAuthToken);

                var url = string.Format(_appSettings.VacationsBotHistoryUrl, email);

                try
                {
                    response = await client.PostAsync(url, null);
                }
                catch (HttpRequestException e)
                {
                    _logger.Error(e);
                    throw new ValidationException(PremiumErrorCodes.VacationBotError, "Vacation bot error");
                }

                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error(new Exception(json));
                    throw new ValidationException(PremiumErrorCodes.VacationBotError, "Vacation bot error");
                }

                var serializer = new JavaScriptSerializer();
                var result = serializer.Deserialize<VacationInfo[]>(json);

                return result;
            }
        }
    }
}