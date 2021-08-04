using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public class LotteryExportService : ILotteryExportService
    {
        private readonly IExcelBuilder _excelBuilder;
        private readonly IParticipantService _participantService;
        public LotteryExportService(IExcelBuilder excelBuilder, IParticipantService participantService)
        {
            _excelBuilder = excelBuilder;
            _participantService = participantService;
        }
        public async Task<byte[]> ExportParticipantsAsync(int lotteryId, UserAndOrganizationDto userAndOrg)
        {
            var participants = await _participantService.GetParticipantsCountedAsync(lotteryId);

            var numberOfTicketsAdded = 0;
            var participantTickets = new List<string>();
            var tickets = new List<List<string>>();

            foreach (var participant in participants)
            {
                for (var i = 0; i < participant.Tickets; i++)
                {
                    participantTickets.Add(participant.FullName);

                    numberOfTicketsAdded++;

                    if (numberOfTicketsAdded % BusinessLayerConstants.LotteryParticipantsInRow == 0)
                    {
                        tickets.Add(participantTickets);
                        participantTickets = new List<string>();
                    }
                }
            }

            tickets.Add(participantTickets);
            _excelBuilder.AddNewWorksheet(BusinessLayerConstants.LotteryParticipantsExcelTableName, tickets);

            return _excelBuilder.GenerateByteArray();
        }
    }
}
