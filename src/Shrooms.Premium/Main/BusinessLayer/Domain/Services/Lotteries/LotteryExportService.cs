using System.Collections.Generic;
using System.Linq;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Host.Contracts.Infrastructure;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Lotteries
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

            int numberOfTicketsAdded = 0;
            var participantTickets = new List<string>();
            var tickets = new List<List<string>>();

            foreach (var participant in participantsDTO)
            {
                for(int i = 0; i < participant.Tickets; i++)
                {
                    participantTickets.Add(participant.FullName);

                    numberOfTicketsAdded++;

                    if(numberOfTicketsAdded % BusinessLayerConstants.LotteryParticipantsInRow == 0)
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
