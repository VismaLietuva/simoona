using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    /// <summary>
    /// Hand-written rather than AutoMapper: almost every field crosses a
    /// representation boundary, and a convention mapper would get those wrong.
    /// </summary>
    internal static class VacationMapper
    {
        public static VacationPersonDto ToPerson(ApplicationUser user)
        {
            return user == null
                ? null
                : new VacationPersonDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PictureId = user.PictureId
                };
        }

        public static VacationRequestDto ToRequest(VacationRequest request, DateTime today)
        {
            return new VacationRequestDto
            {
                Id = request.Id,
                Type = VacationWireFormat.TypeToWire(request.Type),
                Status = VacationWireFormat.StatusToWire(request.Status),
                Employee = ToPerson(request.Employee),
                DateFrom = VacationWireFormat.ToDay(request.DateFrom),
                DateTo = VacationWireFormat.ToDay(request.DateTo),
                WorkingDays = request.WorkingDays,
                Note = request.Note,
                CreatedAt = AsUtc(request.Created),
                ReviewedAt = request.ReviewedAt.HasValue ? AsUtc(request.ReviewedAt.Value) : null,
                ReviewedBy = ToPerson(request.ReviewedBy),
                ReviewComment = request.ReviewComment,
                CanEdit = VacationRequestValidator.CanEdit(request, today),
                CanCancel = VacationRequestValidator.CanCancel(request, today)
            };
        }

        public static VacationEventDto ToEvent(VacationRequestEvent entity)
        {
            return new VacationEventDto
            {
                Id = entity.Id,
                RequestId = entity.VacationRequestId,
                Kind = VacationWireFormat.KindToWire(entity.Kind),
                At = AsUtc(entity.OccurredAt),
                Actor = ToPerson(entity.Actor),
                Employee = ToPerson(entity.Employee),
                Type = VacationWireFormat.TypeToWire(entity.Type),
                DateFrom = VacationWireFormat.ToDay(entity.DateFrom),
                DateTo = VacationWireFormat.ToDay(entity.DateTo),
                WorkingDays = entity.WorkingDays,
                Comment = entity.Comment,
                Changes = DeserializeChanges(entity.ChangesJson)
            };
        }

        public static VacationOrderDto ToOrder(VacationOrder order)
        {
            return new VacationOrderDto
            {
                Id = order.Id,
                Reference = order.Reference,
                Number = order.Number,
                IssuedOn = VacationWireFormat.ToDay(order.IssuedOn),
                Type = order.Type.HasValue ? VacationWireFormat.TypeToWire(order.Type.Value) : null,
                PeriodStart = VacationWireFormat.ToDay(order.PeriodStart),
                IssuedBy = ToPerson(order.IssuedBy),
                CreatedAt = AsUtc(order.Created),
                Items = (order.Items ?? new List<VacationOrderItem>())
                    .OrderBy(item => item.EmployeeName)
                    .Select(item => new VacationOrderItemDto
                    {
                        RequestId = item.VacationRequestId,
                        EmployeeName = item.EmployeeName,
                        Type = VacationWireFormat.TypeToWire(item.Type),
                        DateFrom = VacationWireFormat.ToDay(item.DateFrom),
                        DateTo = VacationWireFormat.ToDay(item.DateTo)
                    })
                    .ToList()
            };
        }

        public static string SerializeChanges(IList<VacationFieldChangeDto> changes)
        {
            return changes == null || changes.Count == 0 ? null : JsonConvert.SerializeObject(changes);
        }

        public static IList<VacationFieldChangeDto> DeserializeChanges(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<VacationFieldChangeDto>();
            }

            try
            {
                return JsonConvert.DeserializeObject<List<VacationFieldChangeDto>>(json)
                       ?? new List<VacationFieldChangeDto>();
            }
            catch (JsonException)
            {
                // Not worth failing a log page over: the row still has its snapshot.
                return new List<VacationFieldChangeDto>();
            }
        }

        public static IList<VacationFieldChangeDto> Diff(VacationRequest before, VacationRequest after)
        {
            var changes = new List<VacationFieldChangeDto>();

            void Push(string field, string from, string to)
            {
                if (from != to)
                {
                    changes.Add(new VacationFieldChangeDto { Field = field, From = from, To = to });
                }
            }

            Push("type", VacationWireFormat.TypeToWire(before.Type), VacationWireFormat.TypeToWire(after.Type));
            Push("status", VacationWireFormat.StatusToWire(before.Status), VacationWireFormat.StatusToWire(after.Status));
            Push("dateFrom", VacationWireFormat.ToDay(before.DateFrom), VacationWireFormat.ToDay(after.DateFrom));
            Push("dateTo", VacationWireFormat.ToDay(before.DateTo), VacationWireFormat.ToDay(after.DateTo));
            Push("note", before.Note, after.Note);

            return changes;
        }

        /// <summary>
        /// EF returns Unspecified for datetime2, and an Unspecified DateTime
        /// serialises without the Z — the client then renders it in its own zone.
        /// </summary>
        private static DateTime AsUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
    }
}
