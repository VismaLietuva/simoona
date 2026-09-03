using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;

namespace Shrooms.Premium.Domain.Services.Events
{
    public class EventQuestionWriter : IEventQuestionWriter
    {
        private readonly DbSet<EventQuestion> _questionsDbSet;
        private readonly DbSet<EventOption> _optionsDbSet;
        private readonly IEventQuestionStructureValidator _structureValidator;
        private readonly ISystemClock _systemClock;

        public EventQuestionWriter(
            IUnitOfWork2 uow,
            IEventQuestionStructureValidator structureValidator,
            ISystemClock systemClock)
        {
            _questionsDbSet = uow.GetDbSet<EventQuestion>();
            _optionsDbSet = uow.GetDbSet<EventOption>();
            _structureValidator = structureValidator;
            _systemClock = systemClock;
        }

        public async Task ValidateAsync(Guid? eventId, IList<EventQuestionStructureDto> questions)
        {
            var desired = questions ?? new List<EventQuestionStructureDto>();

            _structureValidator.ValidatePayload(desired);

            // No event yet, so nothing can be referenced by id. Skipping the load also keeps
            // create from querying for rows that cannot exist.
            var existing = eventId == null
                ? new List<EventQuestion>()
                : await LoadExistingAsync(eventId.Value);

            CheckSuppliedIdsBelongToEvent(existing, desired);

            _structureValidator.ValidateResolved(BuildResolvedFromPayload(desired));
        }

        public Task WriteAsync(Guid eventId, IList<EventQuestionStructureDto> questions, string userId)
        {
            return WriteAsync(eventId, null, questions, userId);
        }

        public Task WriteForNewEventAsync(Event @event, IList<EventQuestionStructureDto> questions, string userId)
        {
            return WriteAsync(@event.Id, @event, questions, userId);
        }

        private async Task WriteAsync(Guid eventId, Event eventEntity, IList<EventQuestionStructureDto> questions, string userId)
        {
            var desired = questions ?? new List<EventQuestionStructureDto>();

            _structureValidator.ValidatePayload(desired);

            var existing = eventEntity == null
                ? await LoadExistingAsync(eventId)
                : new List<EventQuestion>();

            CheckSuppliedIdsBelongToEvent(existing, desired);

            // Validate the entire tree before touching the database. A payload that fails here must
            // leave no trace: persisting questions whose conditions were silently dropped would turn
            // hidden questions into always-visible ones, which the spec explicitly forbids.
            _structureValidator.ValidateResolved(BuildResolvedFromPayload(desired));

            SoftDeleteAbsent(existing, desired, userId);

            var entities = new List<(EventQuestionStructureDto Dto, EventQuestion Entity)>();

            foreach (var dto in desired)
            {
                var entity = dto.Id == null
                    ? InsertQuestion(eventId, eventEntity, dto, userId)
                    : UpdateQuestion(existing, dto, userId);

                entities.Add((dto, entity));
            }

            var optionByClientId = new Dictionary<string, EventOption>();

            foreach (var (dto, entity) in entities)
            {
                WriteOptions(eventId, eventEntity, dto, entity, existing, optionByClientId, userId);
            }

            foreach (var (dto, entity) in entities)
            {
                ApplyCondition(dto, entity, optionByClientId);
            }
        }

        private async Task<List<EventQuestion>> LoadExistingAsync(Guid eventId)
        {
            return await _questionsDbSet
                .Include(q => q.Options)
                .ThenInclude(o => o.EventParticipants)
                .Where(q => q.EventId == eventId)
                .ToListAsync();
        }

        /// <summary>
        /// Every id the client supplies has to name a live row of this event, and an option id has
        /// to sit under the question that claims it. Without this the lookups below throw
        /// InvalidOperationException, which the controllers do not catch — a 500 for a stale form,
        /// an option dragged between questions, or an id borrowed from another event.
        /// </summary>
        private static void CheckSuppliedIdsBelongToEvent(
            List<EventQuestion> existing,
            IList<EventQuestionStructureDto> desired)
        {
            var existingById = existing.ToDictionary(question => question.Id);

            foreach (var dto in desired.Where(question => question.Id != null))
            {
                if (!existingById.ContainsKey(dto.Id.Value))
                {
                    throw new EventException(PremiumErrorCodes.EventQuestionNotFound);
                }
            }

            foreach (var dto in desired)
            {
                var ownedOptionIds = dto.Id != null
                    ? existingById[dto.Id.Value].Options?.Select(option => option.Id).ToHashSet() ?? new HashSet<int>()
                    : new HashSet<int>();

                if (dto.Options.Any(option => option.Id != null && !ownedOptionIds.Contains(option.Id.Value)))
                {
                    throw new EventException(PremiumErrorCodes.EventQuestionOptionNotFound);
                }
            }
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

                    if (!string.IsNullOrWhiteSpace(option.ClientId))
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

            if (string.IsNullOrWhiteSpace(dto.ShowIfOptionClientId))
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

            if (string.IsNullOrWhiteSpace(dto.ShowIfOptionClientId))
            {
                entity.ShowIfOptionId = null;
                entity.ShowIfOption = null;
                return;
            }

            // The trigger row is being inserted in this same request and has no ID yet, so the link
            // rides on the navigation property and EF assigns the foreign key during SaveChanges.
            entity.ShowIfOption = optionByClientId[dto.ShowIfOptionClientId];
        }

        private EventQuestion InsertQuestion(Guid eventId, Event eventEntity, EventQuestionStructureDto dto, string userId)
        {
            var entity = new EventQuestion
            {
                EventId = eventId,
                Event = eventEntity,
                Title = dto.Title,
                Order = dto.Order,
                SelectType = dto.SelectType,
                IsRequired = dto.IsRequired,
                Options = new List<EventOption>()
            };

            StampCreated(entity, userId);

            _questionsDbSet.Add(entity);
            return entity;
        }

        private EventQuestion UpdateQuestion(List<EventQuestion> existing, EventQuestionStructureDto dto, string userId)
        {
            var entity = existing.Single(q => q.Id == dto.Id);

            entity.Title = dto.Title;
            entity.Order = dto.Order;
            entity.SelectType = dto.SelectType;
            entity.IsRequired = dto.IsRequired;

            StampModified(entity, userId);

            return entity;
        }

        private void WriteOptions(
            Guid eventId,
            Event eventEntity,
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
                SoftDeleteOption(removed, userId);
            }

            foreach (var optionDto in dto.Options)
            {
                if (optionDto.Id == null)
                {
                    var option = new EventOption
                    {
                        EventId = eventId,
                        Event = eventEntity,
                        Option = optionDto.Name,
                        Order = optionDto.Order,
                        Rule = optionDto.Rule ?? OptionRules.Default,
                        Question = entity
                    };

                    StampCreated(option, userId);

                    _optionsDbSet.Add(option);

                    if (!string.IsNullOrWhiteSpace(optionDto.ClientId))
                    {
                        optionByClientId[optionDto.ClientId] = option;
                    }
                }
                else
                {
                    var option = existingOptions.Single(o => o.Id == optionDto.Id.Value);
                    option.Option = optionDto.Name;
                    option.Order = optionDto.Order;

                    // A client that omits the rule keeps the stored one: the read shapes did not
                    // always carry it, so treating "absent" as Default silently cleared it.
                    if (optionDto.Rule != null)
                    {
                        option.Rule = optionDto.Rule.Value;
                    }

                    StampModified(option, userId);

                    if (!string.IsNullOrWhiteSpace(optionDto.ClientId))
                    {
                        optionByClientId[optionDto.ClientId] = option;
                    }
                }
            }
        }

        private void SoftDeleteAbsent(
            List<EventQuestion> existing,
            IList<EventQuestionStructureDto> desired,
            string userId)
        {
            var keptIds = desired.Where(q => q.Id != null).Select(q => q.Id.Value).ToHashSet();

            foreach (var question in existing.Where(q => !keptIds.Contains(q.Id)))
            {
                SoftDelete(question, userId);

                foreach (var option in question.Options ?? new List<EventOption>())
                {
                    SoftDeleteOption(option, userId);
                }
            }
        }

        private void SoftDelete(SoftDeletableModel entity, string userId)
        {
            entity.IsDeleted = true;
            StampModified(entity, userId);
        }

        private void SoftDeleteOption(EventOption option, string userId)
        {
            option.EventParticipants?.Clear();
            SoftDelete(option, userId);
        }

        // Both callers of this writer finish with SaveChangesAsync(false), which skips
        // ShroomsDbContext.UpdateEntityMetadata, so nothing else fills these in. Without the stamp
        // an inserted row lands with Created = 0001-01-01 and CreatedBy = NULL. The legacy option
        // path in EventService.UpdateEventOptions sets them by hand for the same reason.
        private void StampCreated(BaseModel entity, string userId)
        {
            entity.Created = _systemClock.UtcNow;
            entity.CreatedBy = userId;
            StampModified(entity, userId);
        }

        private void StampModified(BaseModel entity, string userId)
        {
            entity.Modified = _systemClock.UtcNow;
            entity.ModifiedBy = userId;
        }
    }
}
