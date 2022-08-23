using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.ExcelGenerator;
using Shrooms.Infrastructure.ExcelGenerator;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public class LotteryExportService : ILotteryExportService
    {
        private readonly IExcelBuilderFactory _excelBuilderFactory;
        private readonly ILotteryParticipantService _lotteryParticipantService;

        public LotteryExportService(IExcelBuilderFactory excelBuilderFactory, ILotteryParticipantService lotteryParticipantService)
        {
            _excelBuilderFactory = excelBuilderFactory;
            _lotteryParticipantService = lotteryParticipantService;
        }

        public async Task<ByteArrayContent> ExportParticipantsAsync(int lotteryId, UserAndOrganizationDto userAndOrg)
        {
            var participants = await _lotteryParticipantService.GetParticipantsCountedAsync(lotteryId);

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
                .AddRowPadding(20);

            return new ByteArrayContent(excelBuilder.Build());
        }

        private static Func<string, IExcelColumn> MapLotteryParticipantDtoToExcelCell()
        {
            return participantFullName => new ExcelColumn
            {
                Value = participantFullName,
                SetHorizontalTextCenter = true,
                SetVerticalTextCenter = true,
                BorderTop = ExcelBorderStylePicker.Thin,
                BorderBottom = ExcelBorderStylePicker.Thin,
                BorderLeft = ExcelBorderStylePicker.Thin,
                BorderRight = ExcelBorderStylePicker.Thin
            };
        }
    }
}
