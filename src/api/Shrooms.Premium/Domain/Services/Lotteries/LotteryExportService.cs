using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.ExcelGenerator;
using Shrooms.Infrastructure.ExcelGenerator;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public class LotteryExportService : ILotteryExportService
    {
        private readonly IExcelBuilderFactory _excelBuilderFactory;
        private readonly ILotteryParticipantService _lotteryParticipantService;
        private readonly ILotteryService _lotteryService;

        public LotteryExportService(IExcelBuilderFactory excelBuilderFactory, ILotteryParticipantService lotteryParticipantService, ILotteryService lotteryService)
        {
            _excelBuilderFactory = excelBuilderFactory;
            _lotteryParticipantService = lotteryParticipantService;
            _lotteryService = lotteryService;
        }

        public async Task<FileExportDto> ExportParticipantsAsync(int lotteryId, UserAndOrganizationDto userAndOrg)
        {
            var lottery = await _lotteryService.GetLotteryByIdAsync(lotteryId, userAndOrg);
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
                .AddColumnsPadding(30)
                .AddRowPadding(30);

            var fileName = FileExportName.Sanitize($"{lottery?.Title} - participants", "lottery - participants", ".xlsx");
            return new FileExportDto(excelBuilder.Build(), fileName);
        }

        private static Func<string, IExcelColumn> MapLotteryParticipantDtoToExcelCell()
        {
            return participantFullName => new ExcelColumn
            {
                Value = participantFullName,
                SetHorizontalTextCenter = true,
                SetVerticalTextCenter = true,
                WrapText = true,
                BorderTop = ExcelBorderStylePicker.Thin,
                BorderBottom = ExcelBorderStylePicker.Thin,
                BorderLeft = ExcelBorderStylePicker.Thin,
                BorderRight = ExcelBorderStylePicker.Thin
            };
        }
    }
}
