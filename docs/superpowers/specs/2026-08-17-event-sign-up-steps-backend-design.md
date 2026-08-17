# Event sign-up steps — backend design

**Date:** 2026-08-17
**Status:** Approved, not yet implemented
**Source contract:** `simoona-nextjs/docs/backend/event-sign-up-steps.md` (frontend request, 2026-08-10)

## Goal

Event hosts collect structured answers from attendees at sign-up time. The driving case is food
ordering: an ordered list of questions, each with its own options, where a question can be
configured to appear only if a specific option in an earlier question was chosen.

Today hosts collate this by hand from the event's comment wall.

The frontend is complete and waiting behind a mock. This design covers the backend only.

## What does not change

- **The join payload keeps its shape.** `chosenOptions: int[]` still carries flat leaf option IDs.
  All new structure lives in how the server validates that flat set.
- **Legacy flat-option events keep working untouched.** Options with `QuestionId == null` behave
  exactly as today and `MaxOptions` still applies to them. No data migration.

## Corrections to the frontend request

The request's §7 listed four open questions with assumed answers. Reading the code resolves all
four, and one of them removes work rather than adding it.

| # | Assumption in the request | Resolution |
| - | ------------------------- | ---------- |
| 4 | Recurring events: questions copied to each occurrence, fresh copy per occurrence | **Wrong model — nothing to build.** `Event.EventRecurring` is an `EventRecurrenceOptions` enum column (`None`, `EveryDay`, `EveryWeek`, `EveryTwoWeeks`, `EveryMonth`) on a single `Event` row. Simoona does not materialise per-occurrence rows, so there is nothing to copy to. Questions hang off the one event and are shared across occurrences automatically. |
| 1 | `AddColleague` needs the same validation as Join | **Free.** `EventParticipationService.AddColleagueAsync` delegates to `JoinAsync(joinDto, true)`. Validating Join covers it. The request's "three endpoints" are two code paths. |
| 3 | `ResetAttendees` presumably clears answers | **Free.** Answers are rows in the `EventParticipant`↔`EventOption` join table. Removing participants removes their answers. |
| 2 | `OptionRules.IgnoreSingleJoin` stays a per-option concern | **Confirmed.** `CheckIfSingleChoiceSelectedWithRule` evaluates it against the flat selected-option list, orthogonal to question grouping. Unchanged. |

Correction 4 should be fed back to the frontend so its spec stops describing occurrence copying.

## Placement

Events live in `Shrooms.Premium`; the entity models and EF configuration live in core. The split
follows the existing `EventOption` precedent:

| Artifact | Project |
| -------- | ------- |
| `EventQuestion` entity | `Shrooms.DataLayer.EntityModels/Models/Events/` |
| `EventQuestionEntityConfig` | `Shrooms.DataLayer/DAL/EntityTypeConfigurations/` |
| Migration | `Shrooms.DataLayer/EFCoreMigrations/` |
| DTOs | `Shrooms.Premium/DataTransferObjects/Models/Events/` |
| ViewModels | `Shrooms.Premium/Presentation/WebViewModels/Events/` |
| AutoMapper config | `Shrooms.Premium/Presentation/ModelMappings/Profiles/EventsProfile.cs` |
| Validators | `Shrooms.Premium/Domain/DomainServiceValidators/Events/` |

### Why two new validator classes

`EventValidationService` is already 371 lines and holds unrelated concerns. The new rules are
cohesive and pure — functions over a question tree with no database access — so they get their own
units:

- **`EventQuestionStructureValidator`** — the §1 invariants and the five limits. Runs on write.
- **`EventAnswerValidator`** — the §3 reachability walk. Runs on join and option change in Phase 1,
  and additionally on attend-status change in Phase 2.

Both are testable without a database or a UoW, which is the main reason to keep them separate.

## Schema

```
EventQuestion : SoftDeletableModel
  Id              int PK
  EventId         Guid FK -> Event
  Title           string(100)  NOT NULL
  Order           int          NOT NULL
  SelectType      enum { Single = 0, Multi = 1 }
  IsRequired      bool
  ShowIfOptionId  int? FK -> EventOption      -- null = always shown
  IsDeleted, Created, CreatedBy, Modified, ModifiedBy   -- from SoftDeletableModel/BaseModel

EventOption   (existing table, additive)
  + QuestionId    int? FK -> EventQuestion    -- null = legacy flat option
  + Order         int NOT NULL DEFAULT 0
```

`EventQuestionEntityConfig` mirrors `EventOptionEntityConfig`: `HasQueryFilter(e => !e.IsDeleted)`
and `Property(e => e.Title).IsRequired()`.

**Soft-delete interaction.** Both tables carry a global `!IsDeleted` query filter. A question whose
`ShowIfOptionId` points at a soft-deleted option would read back as a question with a dangling
condition. The structure validator rejects that state at write time (see invariant 4), so it cannot
be persisted. The `ShowIfOptionId` FK uses `DeleteBehavior.Restrict` to keep a cascade from
silently rewriting the tree.

### Structural invariants

1. `ShowIfOptionId` must reference an option owned by a question with a strictly lower `Order` in
   the same event. This single rule makes cycles and forward references impossible — no graph
   traversal is needed to prove termination.
2. A question's options must all belong to that question's event.
3. Reordering may not move a conditional question above the question owning its trigger option.
4. Create/Update receives the full desired state of the tree. If that state is internally
   inconsistent — e.g. a condition points at an option removed in the same payload — reject with
   `400` rather than silently nulling the condition. Silently turning a hidden question into an
   always-visible one is a data-quality surprise for the host.

### Limits

The frontend already enforces these; the server enforces the same numbers.

| Limit | Value |
| ----- | ----- |
| Questions per event | 20 |
| Options per question | 30 |
| Question title length | 100 |
| Option label length | 100 |
| Conditional chain depth | 5 |

**Depth counting.** An always-shown question (`ShowIfOptionId == null`) is depth 0. A question whose
condition points at an option owned by a depth-*N* question is depth *N*+1. Depth greater than 5 is
rejected — at most five conditional levels off an always-shown root. Both sides must count it this
way or the two validators disagree at the boundary.

## Write path

`questions[]` is added to `CreateEventViewModel` and `UpdateEventViewModel`. `newOptions[]`,
`editedOptions[]` and `maxOptions` are untouched and continue serving legacy flat-option events.

### The temp-ID problem

When a host builds a new event, neither questions nor options have database IDs yet, but question
2's condition already needs to point at an option of question 1. Every node carries a
client-generated `clientId`, and a condition references **either** a real `optionId` (row already
saved) **or** an `optionClientId` (row being inserted in this same request). Exactly one of the two
is set; both set, or neither, is a `400`.

Server order:

1. Insert questions (without `ShowIfOptionId`).
2. Insert options, recording the `clientId` → generated-ID map.
3. Resolve `optionClientId` references through that map and set `ShowIfOptionId`.
4. Run `EventQuestionStructureValidator` over the resulting tree.
5. Commit — or roll back the whole unit of work on failure.

Validation deliberately runs after resolution, so invariant 1 is checked against real IDs and
orders rather than against a half-resolved payload.

Rows present in the database but absent from the payload are soft-deleted, matching
`EventOption.IsDeleted`.

### Conventions

- `order` is 0-based and contiguous within its parent. The frontend always sends a dense sequence;
  the server may renumber defensively but must not reorder.
- `rule` is always `Default` from this frontend. `IgnoreSingleJoin` is not surfaced in the builder;
  the field rides along only to keep the option shape identical to `NewEventOptionViewModel`.

## Answer validation

Applies to `POST /Events/Join` and `POST /Events/Options` — and therefore to
`POST /Events/AddColleague`, which delegates to Join. `chosenOptions: int[]` stays a flat list of
leaf option IDs; the server groups it by `QuestionId` and walks the questions in `Order`.

**Reachability.** A question is reachable iff `ShowIfOptionId == null`, or that option is present in
`chosenOptions` and the question owning that option is itself reachable. Because triggers always
live at a lower `Order`, one forward pass resolves this.

| Rule | Error reason |
| ---- | ------------ |
| Every option ID belongs to this event | `UnknownOption` |
| `SelectType == Single` → at most 1 chosen for that question | `TooManyAnswers` |
| Reachable and `IsRequired` → at least 1 chosen | `RequiredAnswerMissing` |
| Not reachable → exactly 0 chosen | `AnswerForHiddenQuestion` |
| Legacy options (`QuestionId == null`) → still capped by `MaxOptions` | unchanged behaviour |

## Error contract

The existing pattern in `EventController` is `catch (EventException e) => BadRequest(e.Message)`,
which produces a bare string body. The frontend needs a machine-readable body so its wizard can jump
to the offending step instead of showing a generic error:

```json
{
  "code": "EventAnswersInvalid",
  "errors": [
    { "questionId": 12, "reason": "RequiredAnswerMissing" },
    { "questionId": 14, "reason": "AnswerForHiddenQuestion" }
  ]
}
```

This is delivered additively. A new `EventAnswersInvalidException : EventException` carries the
structured error list, and the affected actions catch it *before* the generic `EventException`
handler, returning the object body. Every other error in the controller keeps its current string
body, so no existing client breaks.

**Scope of the structured body: answer validation only.** Write-time structural failures — a bad
`clientId` reference, a breached limit, a violated invariant — are host-facing builder errors that
the frontend already prevents, so they throw plain `EventException` and keep the existing string
body. Only the four answer-validation reasons need machine-readable routing, because only those can
be reached by an attendee mid-wizard.

## Read path

| Endpoint | Change |
| -------- | ------ |
| `GET /Events/Options?eventId` | Add `questions[]` (full tree: `selectType`, `isRequired`, `showIfOptionId`, ordered options). Add `myChosenOptions: int[]` — the calling user's own current answers. |
| `GET /Events/Update?eventId` | Add `questions[]` so the host's builder hydrates on edit. |
| `GET /Events/Details` | Add `questions[]`, each option keeping the existing `participants[]` shape, so the host sees who ordered what grouped by question. Keep the flat `options[]` for legacy. |
| `GET /Events/Export?eventId` | One column per question in the participants sheet. Hosts use this to place the actual food order. |

`myChosenOptions` is derivable from `Details.options[].participants[]`, but only by scanning every
participant, and `Details` does not carry the question grouping. Returning it directly on `Options`
is what lets the wizard prefill when a user reopens it.

## Attend-status transitions

`UpdateAttendStatusViewModel` is `{ eventId, attendStatus, attendComment }` — no `chosenOptions`. A
user switching Maybe → Going has no way to supply answers, so an event with required questions would
either reject the transition or accept an incomplete participant.

Add `chosenOptions: int[]` (nullable) with this lifecycle:

| Transition | Answers |
| ---------- | ------- |
| → Attending / AttendingVirtually | required; validated per the answer-validation rules |
| → MaybeAttending | preserved as-is, not re-validated |
| → NotAttending, Leave, Expel | cleared |

`AttendingVirtually` answers the same question set — there is no separate one.

## Editing after attendees have answered

| Change | Behaviour |
| ------ | --------- |
| Add question, add option, rename, reorder | Always allowed, no participant impact |
| Delete question/option, `Multi` → `Single`, optional → required | Allowed. Orphaned or invalidated answers are deleted, and the response reports how many participants were affected so the host can be warned first |

This reuses the spirit of the existing `ResetParticipantList` flag rather than introducing a second
mechanism. Optional → required does **not** retroactively invalidate participants who already joined
without answering — they stay joined with a gap. Re-prompting existing attendees is out of scope.

## Phasing

**Phase 1 — unblocks frontend integration end-to-end**

Entity, EF config and migration; both validators; `questions[]` on Create/Update write with
`clientId` resolution; `GET /Events/Options` (`questions[]` + `myChosenOptions`);
`GET /Events/Update` hydration; Join and Options answer validation; the `EventAnswersInvalid` body.

**Phase 2**

`GET /Events/Details` question grouping for the host responses panel; `GET /Events/Export` columns;
`UpdateAttendStatusViewModel.chosenOptions` and the transition lifecycle; the affected-participant
count reported on destructive edits.

## Testing

Unit tests, no database:

- `EventQuestionStructureValidator` — each of the five limits at its boundary; depth counting at 5
  and 6; each of the four structural invariants; the `optionId`/`optionClientId` exactly-one rule.
- `EventAnswerValidator` — reachability through a multi-level branch; each of the four error
  reasons; legacy flat options still capped by `MaxOptions`; mixed legacy-and-question events.

Service tests:

- Create and Update round-trip a full tree, including a condition resolved from `optionClientId`.
- Rows absent from the payload are soft-deleted, not hard-deleted.
- A payload whose condition points at an option removed in the same request is rejected, and nothing
  is committed.
- Destructive edits report the correct affected-participant count.

## Out of scope

- Re-prompting existing attendees after a question becomes required.
- Surfacing `IgnoreSingleJoin` in the builder UI.
- Any change to the AngularJS webapp — the client for this feature is the Next.js frontend.
