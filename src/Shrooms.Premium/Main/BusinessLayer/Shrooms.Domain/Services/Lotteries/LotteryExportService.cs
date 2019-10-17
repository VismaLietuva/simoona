using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Infrastructure.ExcelGenerator;

namespace Shrooms.Domain.Services.Lotteries
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

                    if(numberOfTicketsAdded % ConstBusinessLayer.ParticipantsInRow == 0)
                    {
                        tickets.Add(participantTickets);
                        participantTickets = new List<string>();
                    }
                }
            }
            tickets.Add(participantTickets);

            _excelBuilder.AddNewWorksheet(ConstBusinessLayer.LotteryParticipantsExcelTableName, new List<string>(), tickets);

            return _excelBuilder.GenerateByteArray();
        }
    }
}
