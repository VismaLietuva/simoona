using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.ExcelGenerator;
using Shrooms.Infrastructure.ExcelGenerator;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public class LotteryExportService : ILotteryExportService
    {
        private readonly IExcelBuilderFactory _excelBuilderFactory;
        private readonly IParticipantService _participantService;

        public LotteryExportService(IExcelBuilderFactory excelBuilderFactory, IParticipantService participantService)
        {
            _excelBuilderFactory = excelBuilderFactory;
            _participantService = participantService;
        }

        public async Task<ByteArrayContent> ExportParticipantsAsync(int lotteryId, UserAndOrganizationDto userAndOrg)
        {
            var participants = await _participantService.GetParticipantsCountedAsync(lotteryId);

            var tickets = participants
                .SelectMany(participant => Enumerable.Repeat(participant.FullName, participant.Tickets));

            var excelBuilder = _excelBuilderFactory.GetBuilder();

            excelBuilder
                .AddWorksheet(BusinessLayerConstants.LotteryParticipantsExcelTableName)
                .AddColumnSequence(
                    tickets,
                    MapLotteryParticipantDtoToExcelCell(),
                    BusinessLayerConstants.LotteryParticipantsInRow)
                .AddColumnsPadding(20)
                .AddRowsPadding(20);

            return new ByteArrayContent(excelBuilder.Build());
        }

        private static Func<string, IExcelColumn> MapLotteryParticipantDtoToExcelCell()
        {
            return participantFullName => new ExcelColumn
            {
                Value = participantFullName,
                SetHorizontalTextCenter = true,
                SetVerticalTextCenter = true
            };
        }
    }
}
