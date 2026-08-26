using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    /// <summary>
    /// What a leave order says, independent of the file it is written into. The
    /// .docx and the .pdf builder both read from here, so the two renderings of
    /// one numbered document cannot drift apart.
    ///
    /// The Lithuanian wording is literal, not translated: this is a legal
    /// document that goes to payroll, so it is a constant here for the same
    /// reason the Vacation Bot letters are constants on the client.
    /// </summary>
    internal static class VacationOrderContent
    {
        /// <summary>The trailing space is in the signed original.</summary>
        public const string Heading = "ĮSAKYMAS ";

        public const string Preamble = "Atsižvelgdamas į prašymus,";

        public const string Payout = "Išmokant priskaičiuotus atostoginius su mėnesio atlyginimu.";

        private static readonly string[] MonthsGenitive =
        {
            "sausio", "vasario", "kovo", "balandžio", "gegužės", "birželio",
            "liepos", "rugpjūčio", "rugsėjo", "spalio", "lapkričio", "gruodžio"
        };

        public static IEnumerable<string> LetterheadLines(VacationSettingsDto settings)
        {
            return (settings.OrderLetterhead ?? string.Empty)
                .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0);
        }

        public static string ReferenceLine(VacationOrder order)
        {
            return $"{VacationWireFormat.ToDay(order.IssuedOn)} d. Nr.{order.Reference}";
        }

        public static string TitleFor(VacationOrder order)
        {
            var types = order.Items.Select(item => item.Type).Distinct().ToList();
            if (types.Count != 1)
            {
                return "DĖL ATOSTOGŲ SUTEIKIMO";
            }

            return types[0] switch
            {
                VacationRequestType.Annual => "DĖL KASMETINIŲ ATOSTOGŲ SUTEIKIMO",
                VacationRequestType.Unpaid => "DĖL NEMOKAMŲ ATOSTOGŲ SUTEIKIMO",
                VacationRequestType.Parental => "DĖL PAPILDOMOS POILSIO DIENOS SUTEIKIMO",
                _ => "DĖL ATOSTOGŲ SUTEIKIMO"
            };
        }

        /// <summary>
        /// Each leave type gets its own decree sentence: one order can grant
        /// annual and unpaid leave, and the wording differs.
        /// </summary>
        public static IEnumerable<IGrouping<VacationRequestType, VacationOrderItem>> Groups(VacationOrder order)
        {
            return order.Items.GroupBy(item => item.Type).OrderBy(group => group.Key);
        }

        public static IEnumerable<VacationOrderItem> Lines(IEnumerable<VacationOrderItem> group)
        {
            return group.OrderBy(item => item.EmployeeName, StringComparer.CurrentCulture);
        }

        /// <summary>
        /// Only paid leave is paid out: the unpaid and parental orders end at the
        /// list, as the signed ones do.
        /// </summary>
        public static bool HasPayout(VacationOrder order)
        {
            return order.Items.Any(item => item.Type == VacationRequestType.Annual);
        }

        public static string DecreeFor(VacationRequestType type)
        {
            // The spaced "Į s a k a u" is how the original document sets the
            // operative verb; reproduced literally so the two look alike.
            var what = type switch
            {
                VacationRequestType.Annual => "kasmetines apmokamas atostogas",
                VacationRequestType.Unpaid => "nemokamas atostogas",
                VacationRequestType.Parental => "papildomas poilsio dienas (tėvadienius)",
                _ => "atostogas"
            };

            return $"Į s a k a u  suteikti {what} šiems darbuotojams:";
        }

        public static string ItemLine(VacationOrderItem item)
        {
            // A single day is written out long-hand and a period as a range, which
            // is the distinction the original document draws.
            return item.DateFrom.Date == item.DateTo.Date
                ? $"{item.EmployeeName} : {LongDate(item.DateFrom)} imtinai;"
                : $"{item.EmployeeName} : nuo {VacationWireFormat.ToDay(item.DateFrom)} d. iki {VacationWireFormat.ToDay(item.DateTo)} d. imtinai;";
        }

        private static string LongDate(DateTime date)
        {
            return $"{date.Year} m. {MonthsGenitive[date.Month - 1]} {date.Day} d.";
        }
    }
}
