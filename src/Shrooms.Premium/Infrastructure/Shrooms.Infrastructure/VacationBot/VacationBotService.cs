using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.Infrastructure.Configuration;
using System;
using Shrooms.Premium.Other.Shrooms.Constants.ErrorCodes;

namespace Shrooms.Infrastructure.VacationBot
{
    public class VacationBotService : IVacationBotService
    {
        private readonly IApplicationSettings _appSettings;
        private readonly Logger.ILogger _logger;

        public VacationBotService(IApplicationSettings appSettings, Logger.ILogger logger)
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
                    throw new ValidationException(ErrorCodes.VacationBotError, "Vacation bot error");
                }

                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error(new Exception(json));
                    throw new ValidationException(ErrorCodes.VacationBotError, "Vacation bot error");
                }

                var serializer = new JavaScriptSerializer();
                var result = serializer.Deserialize<VacationInfo[]>(json);

                return result;
            }
        }
    }
}