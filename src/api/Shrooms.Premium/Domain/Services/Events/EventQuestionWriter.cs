using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;

namespace Shrooms.Premium.Domain.Services.Events
{
    public class EventQuestionWriter : IEventQuestionWriter
    {
        private readonly IUnitOfWork2 _uow;
        private readonly DbSet<EventQuestion> _questionsDbSet;
        private readonly DbSet<EventOption> _optionsDbSet;
        private readonly IEventQuestionStructureValidator _structureValidator;

        public EventQuestionWriter(IUnitOfWork2 uow, IEventQuestionStructureValidator structureValidator)
        {
            _uow = uow;
            _questionsDbSet = uow.GetDbSet<EventQuestion>();
            _optionsDbSet = uow.GetDbSet<EventOption>();
            _structureValidator = structureValidator;
        }

        public async Task WriteAsync(Guid eventId, IList<EventQuestionStructureDto> questions, string userId)
        {
            var desired = questions ?? new List<EventQuestionStructureDto>();

            _structureValidator.ValidatePayload(desired);

            var existing = await _questionsDbSet
                .Include(q => q.Options)
                .Where(q => q.EventId == eventId)
                .ToListAsync();

            // Validate the entire tree before touching the database. A payload that fails here must
            // leave no trace: persisting questions whose conditions were silently dropped would turn
            // hidden questions into always-visible ones, which the spec explicitly forbids.
            _structureValidator.ValidateResolved(BuildResolvedFromPayload(desired));

            SoftDeleteAbsent(existing, desired, userId);

            var entities = new List<(EventQuestionStructureDto Dto, EventQuestion Entity)>();

            foreach (var dto in desired)
            {
                var entity = dto.Id == null
                    ? InsertQuestion(eventId, dto)
                    : UpdateQuestion(existing, dto);

                entities.Add((dto, entity));
            }

            var optionByClientId = new Dictionary<string, EventOption>();

            foreach (var (dto, entity) in entities)
            {
                WriteOptions(eventId, dto, entity, existing, optionByClientId, userId);
            }

            foreach (var (dto, entity) in entities)
            {
                ApplyCondition(dto, entity, optionByClientId);
            }

            await _uow.SaveChangesAsync(userId);
        }

        /// <summary>
        /// Projects the desired payload onto <see cref="ResolvedEventQuestionDto"/> so the
        /// structural rules can be checked before anything is inserted. Rows that do not exist yet
        /// get a negative synthetic ID; real database identities are always positive, so the two
        /// spaces cannot collide and the validator cannot tell them apart.
        /// </summary>
        private static IReadOnlyList<ResolvedEventQuestionDto> BuildResolvedFromPayload(
            IList<EventQuestionStructureDto> desired)
        {
            var syntheticOptionId = new Dictionary<EventQuestionOptionStructureDto, int>();
            var syntheticOptionIdByClientId = new Dictionary<string, int>();
            var syntheticQuestionId = new Dictionary<EventQuestionStructureDto, int>();
            var next = -1;

            foreach (var dto in desired)
            {
                syntheticQuestionId[dto] = dto.Id ?? next--;

                foreach (var option in dto.Options)
                {
                    var id = option.Id ?? next--;
                    syntheticOptionId[option] = id;

                    if (option.ClientId != null)
                    {
                        syntheticOptionIdByClientId[option.ClientId] = id;
                    }
                }
            }

            return desired.Select(dto => new ResolvedEventQuestionDto
            {
                QuestionId = syntheticQuestionId[dto],
                Order = dto.Order,
                SelectType = dto.SelectType,
                IsRequired = dto.IsRequired,
                ShowIfOptionId = ResolveTriggerId(dto, syntheticOptionIdByClientId),
                OptionIds = dto.Options.Select(o => syntheticOptionId[o]).ToList()
            }).ToList();
        }

        private static int? ResolveTriggerId(
            EventQuestionStructureDto dto,
            IReadOnlyDictionary<string, int> syntheticOptionIdByClientId)
        {
            if (dto.ShowIfOptionId != null)
            {
                return dto.ShowIfOptionId;
            }

            if (dto.ShowIfOptionClientId == null)
            {
                return null;
            }

            // A clientId naming nothing in this payload is left unresolved on purpose: the
            // validator then fails to find an owning question and rejects the tree, which is the
            // required behaviour for a condition pointing at a row removed in the same request.
            return syntheticOptionIdByClientId.TryGetValue(dto.ShowIfOptionClientId, out var id)
                ? id
                : int.MinValue;
        }

        private static void ApplyCondition(
            EventQuestionStructureDto dto,
            EventQuestion entity,
            IReadOnlyDictionary<string, EventOption> optionByClientId)
        {
            if (dto.ShowIfOptionId != null)
            {
                entity.ShowIfOptionId = dto.ShowIfOptionId;
                return;
            }

            if (dto.ShowIfOptionClientId == null)
            {
                entity.ShowIfOptionId = null;
                entity.ShowIfOption = null;
                return;
            }

            // The trigger row is being inserted in this same request and has no ID yet, so the link
            // rides on the navigation property and EF assigns the foreign key during SaveChanges.
            entity.ShowIfOption = optionByClientId[dto.ShowIfOptionClientId];
        }

        private EventQuestion InsertQuestion(Guid eventId, EventQuestionStructureDto dto)
        {
            var entity = new EventQuestion
            {
                EventId = eventId,
                Title = dto.Title,
                Order = dto.Order,
                SelectType = dto.SelectType,
                IsRequired = dto.IsRequired,
                Options = new List<EventOption>()
            };

            _questionsDbSet.Add(entity);
            return entity;
        }

        private static EventQuestion UpdateQuestion(List<EventQuestion> existing, EventQuestionStructureDto dto)
        {
            var entity = existing.Single(q => q.Id == dto.Id);

            entity.Title = dto.Title;
            entity.Order = dto.Order;
            entity.SelectType = dto.SelectType;
            entity.IsRequired = dto.IsRequired;

            return entity;
        }

        private void WriteOptions(
            Guid eventId,
            EventQuestionStructureDto dto,
            EventQuestion entity,
            List<EventQuestion> existing,
            Dictionary<string, EventOption> optionByClientId,
            string userId)
        {
            var existingOptions = existing
                .Where(q => q.Id == dto.Id)
                .SelectMany(q => q.Options ?? new List<EventOption>())
                .ToList();

            var keptIds = dto.Options.Where(o => o.Id != null).Select(o => o.Id.Value).ToHashSet();

            foreach (var removed in existingOptions.Where(o => !keptIds.Contains(o.Id)))
            {
                removed.IsDeleted = true;
                removed.Modified = DateTime.UtcNow;
                removed.ModifiedBy = userId;
            }

            foreach (var optionDto in dto.Options)
            {
                if (optionDto.Id == null)
                {
                    var option = new EventOption
                    {
                        EventId = eventId,
                        Option = optionDto.Name,
                        Order = optionDto.Order,
                        Rule = optionDto.Rule,
                        Question = entity
                    };

                    _optionsDbSet.Add(option);

                    if (optionDto.ClientId != null)
                    {
                        optionByClientId[optionDto.ClientId] = option;
                    }
                }
                else
                {
                    var option = existingOptions.Single(o => o.Id == optionDto.Id.Value);
                    option.Option = optionDto.Name;
                    option.Order = optionDto.Order;
                    option.Rule = optionDto.Rule;

                    if (optionDto.ClientId != null)
                    {
                        optionByClientId[optionDto.ClientId] = option;
                    }
                }
            }
        }

        private static void SoftDeleteAbsent(
            List<EventQuestion> existing,
            IList<EventQuestionStructureDto> desired,
            string userId)
        {
            var keptIds = desired.Where(q => q.Id != null).Select(q => q.Id.Value).ToHashSet();

            foreach (var question in existing.Where(q => !keptIds.Contains(q.Id)))
            {
                question.IsDeleted = true;
                question.Modified = DateTime.UtcNow;
                question.ModifiedBy = userId;

                foreach (var option in question.Options ?? new List<EventOption>())
                {
                    option.IsDeleted = true;
                    option.Modified = DateTime.UtcNow;
                    option.ModifiedBy = userId;
                }
            }
        }
    }
}
