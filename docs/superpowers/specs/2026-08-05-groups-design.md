# Groups — Design

**Date:** 2026-08-05
**Status:** Approved

## Summary

A generic **Groups** feature modelled on the existing Committees feature, where behaviour is driven by an
admin-configurable **GroupType** rather than hard-coded. Five types ship seeded: Committee, TaskForce,
FoodMaster, GroupOfInterest, Other.

Groups is a **premium** feature living in `Shrooms.Premium`, exactly like Committees and Event Types.

## Scope

### In scope

- `Group`, `GroupType` and `GroupSuggestion` entities, org-scoped and soft-deletable.
- Admin CRUD for group types under Admin → Customization → Group Types.
- Admin CRUD for groups, plus a client-facing Groups list page below Committees.
- Type-gated suggestions, mirroring the committee suggestion flow.
- A monthly kudos award endpoint consumed by an external Logic App.
- New `GROUPS_BASIC` / `GROUPS_ADMINISTRATION` permissions.

### Out of scope

- **Committees are not touched.** No data migration, no shared tables, no changes to the committee
  controller, service, kudos-committee logic, wall widgets or suggestion emails. Groups is a parallel
  feature. Folding Committees into Groups is a possible future change, not part of this work.
- **Tagging groups in posts.** The `Tag` field is stored and validated now. Wiring it up as an @mention
  target in posts (today only people can be tagged) is a later feature.
- The Logic App itself. This spec only defines the endpoint it calls.

## Data model

New entities in `Shrooms.DataLayer.EntityModels/Models/Group/`, all deriving from
`SoftDeletableModelWithOrg`, configured through `IEntityTypeConfiguration` implementations in
`Shrooms.DataLayer/DAL/EntityTypeConfigurations/` following `CommitteeEntityConfig`.

### GroupType

The admin-managed classifier. Modelled on `EventType`.

| Field | Type | Meaning |
|---|---|---|
| `Name` | string, required | Unique per organization |
| `HasLeader` | bool | Enables the group's `LeaderId` field |
| `HasDelegates` | bool | Enables the `Delegates` collection |
| `IsTemporary` | bool | Time-bound: enables and requires start/end dates, expired groups move to "Past groups", and the type may not receive kudos |
| `HasGroupTag` | bool | Enables the `Tag` handle |
| `ReceivesKudos` | bool | Enables `MonthlyKudosAmount` and `KudosTypeId` |
| `HasSuggestions` | bool | Enables the suggestions panel and endpoints |

`IsTemporary` deliberately covers both "is time-bound" and "shows start/end dates" — a single flag,
because a temporary group with hidden dates is a contradictory state. The trade-off is that a permanent
group cannot display a "founded on" date; nothing in the requirements needs that.

### Group

| Field | Type | Notes |
|---|---|---|
| `Name` | string, required | Unique per organization |
| `Description` | string, required | |
| `PictureId` | string, nullable | Card thumbnail, falls back to `group-default.png` |
| `Website` | string, nullable | |
| `GroupTypeId` | int, required | FK → `GroupType` |
| `Tag` | string, nullable | Unique per organization, case-insensitive, when set |
| `LeaderId` | string, nullable | FK → `ApplicationUser`. A single leader, like `Project.OwnerId` |
| `StartDate` | DateTime, nullable | |
| `EndDate` | DateTime, nullable | |
| `MonthlyKudosAmount` | decimal, nullable | Matches `KudosType.Value` precision |
| `KudosTypeId` | int, nullable | FK → `KudosType` |
| `Members` | m2m → `ApplicationUser` | |
| `Delegates` | m2m → `ApplicationUser` | |
| `Suggestions` | collection of `GroupSuggestion` | |

### GroupSuggestion

Mirrors `CommitteeSuggestion`: `Title`, `Description`, `Date`, `User`.

### GroupMonthlyKudosAward

Idempotency guard and audit trail for the monthly award. Derives from `BaseModelWithOrg` — these rows
are an audit record and are never soft-deleted.

| Field | Notes |
|---|---|
| `OrganizationId`, `Year`, `Month`, `UserId` | Unique index across all four |
| `GroupId` | Which group won this user's allocation |
| `Amount` | |
| `KudosLogId` | FK to the written `KudosLog` |

## Migration

One EF Core migration in `Shrooms.DataLayer/EFCoreMigrations/`:

1. Creates the four tables and their join tables.
2. Seeds the five group types **per organization**. All flags remain editable afterwards; no type is
   marked as protected.
3. Seeds `GROUPS_BASIC` and `GROUPS_ADMINISTRATION` into `Permissions`, and grants them to whichever
   roles currently hold `COMMITTEES_BASIC` / `COMMITTEES_ADMINISTRATION` respectively — following the
   `IF NOT EXISTS` + mirror-from-existing-permission pattern in
   `20260421000005_AddEventUsersPermission.cs`.

### Seeded type flags

| Type | Leader | Delegates | Temporary | Tag | Kudos | Suggestions |
|---|---|---|---|---|---|---|
| Committee | – | – | – | ✓ | ✓ | ✓ |
| TaskForce | – | – | ✓ | ✓ | – | ✓ |
| FoodMaster | – | – | – | ✓ | ✓ | – |
| GroupOfInterest | – | – | – | ✓ | – | ✓ |
| Other | – | – | – | – | – | – |

`HasLeader` and `HasDelegates` ship off for every type; admins enable them per type as needed.

## API

All new code sits in `Shrooms.Premium`, mirroring the Committees layout:

```
Premium/DataTransferObjects/Models/Groups/
Premium/Domain/Services/Groups/            IGroupsService, GroupsService,
                                           IGroupTypesService, GroupTypesService
Premium/Domain/Services/Email/Group/       IGroupNotificationService, GroupNotificationService
Premium/Presentation/Api/Controllers/      GroupsController, GroupTypesController
Premium/Presentation/WebViewModels/Groups/
Premium/Presentation/ModelMappings/Profiles/GroupProfile.cs
Premium/IoC/Modules/GroupModule.cs
```

`GroupModule` registers `IGroupsService`, `IGroupTypesService` and `IGroupNotificationService` as
scoped, following `CommitteeModule`, and is called from the premium IoC registration.

### GroupTypesController

Shaped like `EventTypeController` — `[Route("GroupTypes")]`, every action on `GROUPS_ADMINISTRATION`.

| Verb | Route | Purpose |
|---|---|---|
| GET | `Types` | All group types for the organization |
| GET | `Get?id=` | Single type |
| POST | `Create` | |
| PUT | `Update` | |
| DELETE | `Delete?id=` | |

### GroupsController

`[Route("Groups")]`.

| Verb | Route | Permission | Purpose |
|---|---|---|---|
| GET | `GetAll` | `GROUPS_BASIC` | All groups with their type's flags embedded |
| GET | `Get?id=` | `GROUPS_BASIC` | Single group |
| POST | `Post` | `GROUPS_ADMINISTRATION` | |
| PUT | `Put` | `GROUPS_ADMINISTRATION` | |
| DELETE | `Delete?id=` | `GROUPS_ADMINISTRATION` | |
| GET | `GetSuggestions?id=` | `GROUPS_BASIC` | |
| POST | `PostSuggestion` | `GROUPS_BASIC` | |
| DELETE | `DeleteSuggestion` | `GROUPS_ADMINISTRATION` | |
| POST | `AwardMonthlyKudos` | `KUDOS_ADMINISTRATION` | Logic App entry point |

`GetAll` embeds each group's type flags in the response so the UI can render conditionally without a
second request. The three suggestion endpoints return a validation error when the group's type has
`HasSuggestions = false`.

### Validation

Every rule throws `ValidationException` with a new `ErrorCodes` entry, and each code is registered in
`src/webapp/src/client/app/common/services/error-handler.service.js` — following the KudosType
uniqueness work in commit `606b87b0`.

**GroupsService**

1. A field disabled by the group's type is populated → reject. Covers `LeaderId`, `Delegates`,
   `StartDate`/`EndDate`, `Tag`, `MonthlyKudosAmount`/`KudosTypeId`. The UI hides these fields, so a
   rejection means a malformed API call, not a normal user path.
2. `IsTemporary` → `EndDate` is required, and `EndDate >= StartDate` when both are set.
3. `ReceivesKudos` → `MonthlyKudosAmount` is required and greater than zero, `KudosTypeId` is required
   and must resolve to an existing kudos type.
4. `Tag` is unique per organization, case-insensitive, when set.
5. `Name` is unique per organization.

**GroupTypesService**

1. `Name` is unique per organization.
2. `IsTemporary && ReceivesKudos` → reject. The two are mutually exclusive by definition.
3. Deleting a type that still has groups → reject.

### Turning a type flag off

Because groups reject writes to fields their type disables, existing rows would otherwise keep orphaned
data the UI no longer shows. When an admin clears a flag on a type that already has groups, the update
clears the corresponding field on every group of that type, in the same transaction:

| Flag cleared | Fields cleared on the type's groups |
|---|---|
| `HasLeader` | `LeaderId` |
| `HasDelegates` | `Delegates` |
| `IsTemporary` | `StartDate`, `EndDate` |
| `HasGroupTag` | `Tag` |
| `ReceivesKudos` | `MonthlyKudosAmount`, `KudosTypeId` |
| `HasSuggestions` | Suggestions are soft-deleted |

This is destructive, so the admin form warns before saving — naming the flag, the number of affected
groups, and what will be cleared — and requires confirmation. Already-awarded
`GroupMonthlyKudosAward` rows and their `KudosLog` entries are never touched.

## Monthly kudos award

An external Logic App calls `POST Groups/AwardMonthlyKudos` once a month. It authenticates as a
dedicated service account holding `KUDOS_ADMINISTRATION` — Simoona has no API-key mechanism, so this
goes through the normal OAuth token flow.

The request takes optional `year` and `month`, defaulting to the current month, so a missed run can be
replayed.

### Allocation rule

A person can belong to several kudos-receiving groups — someone may be foodmaster for three food teams.
**They receive the highest single amount among their groups, once**, not the sum.

The computation:

1. Select every group whose type has `ReceivesKudos`. Such a group can never carry dates — `IsTemporary`
   and `ReceivesKudos` are mutually exclusive, and clearing `IsTemporary` wipes the dates — so there is
   no expiry filter to apply.
2. Expand each group's members.
3. Group by user, keep the row with the highest `MonthlyKudosAmount`. That group's `KudosTypeId` is the
   type awarded.

This rule lives in a single service method so it can be swapped for sum-or-cap later without touching
the Logic App.

### Awarding and idempotency

The award runs in one transaction:

- For each computed allocation, insert a `GroupMonthlyKudosAward` row. The unique index on
  `(OrganizationId, Year, Month, UserId)` means rows already present for the period are skipped.
- For each newly inserted row, write a `KudosLog` with `Status = KudosStatus.Approved`,
  `KudosTypeName` / `KudosTypeValue` copied from the group's `KudosType`, `Points` set to the amount,
  and a generated comment naming the group. These are system grants, so there is no approval step.
- The response reports awarded and skipped counts.

A re-run for the same period is therefore a no-op, and `GroupMonthlyKudosAward` records which group won
each user's allocation.

## UI

### Admin → Customization → Group Types

Mirrors `app/customization/event-types/`:

```
app/customization/group-types/
  group-types.module.js                          premium-gated states,
                                                 customizationNavigationFactory entry
  group-types.repository.js
  list/list.html, list.controller.js             table: Name + a Yes/No column per flag
  create-edit/create-edit.html, .controller.js   name + six flag checkboxes
```

`customization.module.js` gains `simoonaApp.Customization.GroupTypes` in its module list and
`GROUPS_ADMINISTRATION` in both `authorizeOneOfPermissions` arrays.

The create/edit form disables `ReceivesKudos` while `IsTemporary` is checked and vice versa, so the
mutually-exclusive pair cannot be submitted. The server rejects it independently.

### Client → Groups

A new left-menu item at `order: 7` in `leftMenuGroups.company`, directly below Committees, gated on
`GROUPS_BASIC` and `isPremium`:

```
app/group/
  group.module.js
  group.repository.js
  group.controller.js
  group-list.html
  group-new-edit-modal.html, group-new-edit-modal.controller.js
  group-suggestion.controller.js
```

The list reuses the card grid plus right-hand detail panel from `committee-list.html`, with:

- one heading per group type, and
- a collapsed **Past groups** section at the bottom containing expired temporary groups
  (`IsTemporary` and `EndDate` in the past). Expired groups stay viewable and editable.

The detail panel renders only what the group's type enables: leader, delegates, dates, tag badge, and
the suggestions panel each appear conditionally.

### Create/edit modal

Gated on `GROUPS_ADMINISTRATION`. The type picker comes first; the rest of the form reacts to the
selected type's flags. Leader autocomplete, delegates autocomplete, date pickers, tag input, and the
kudos amount + KudosType dropdown each appear only when their flag is on.

Changing the type clears the fields the new type does not support, so the submitted payload always
matches what the server will accept.

### Wiring

- Six new `<script>` tags in `src/webapp/src/client/index.html`, next to the committee ones, plus the
  group-types customization scripts.
- New `resources/en_US/group.json` and `resources/lt_LT/group.json`.
- Additions to `customization.json` (menu entry name and description) and `navbar.json` (`navbar.groups`).
- New error codes in `error-handler.service.js`.
- A `group-default.png` / `group-default-th.png` thumbnail pair alongside the committee defaults.
- Server-side validation messages in a new `Shrooms.Resources/Models/Group/Group.resx` and its `lt-Lt`
  counterpart, following the committee resources.

## Testing

`Shrooms.Premium.Tests` gains `GroupsServiceTests` and `GroupTypesServiceTests`, following the shape of
the `KudosServiceTests` added in `606b87b0`:

- Each validation rule in both services, including the disabled-field rejections and the
  `IsTemporary` + `ReceivesKudos` conflict.
- Group name and tag uniqueness within an organization, and that the same name is allowed in a
  different organization.
- Deleting a group type that still has groups is rejected.
- The allocation rule with a user in three kudos-receiving groups of differing amounts — expects one
  award at the highest amount, carrying that group's kudos type.
- Clearing a type flag clears the matching field on that type's groups, and leaves already-awarded
  `GroupMonthlyKudosAward` rows intact.
- Award idempotency: running twice for the same period writes one `KudosLog` per user and reports the
  second run as fully skipped.

## Open questions

None. The multi-group allocation rule (highest single amount) is a business decision that may be
revisited; it is isolated in one service method for that reason.
