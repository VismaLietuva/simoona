using System.Collections.Generic;
using System.Linq;
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
        public byte[] ExportParticipants(int lotteryId, UserAndOrganizationDTO userAndOrg)
        {
            var participantsDTO = _participantService.GetParticipantsCounted(lotteryId).ToList();

            var numberOfTicketsAdded = 0;
            var participantTickets = new List<string>();
            var tickets = new List<List<string>>();

            foreach (var participant in participantsDTO)
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
