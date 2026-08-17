# Event Sign-Up Steps — Phase 1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let event hosts define an ordered, conditionally-branching set of sign-up questions, and validate attendee answers against that structure on join.

**Architecture:** A new `EventQuestion` table owns ordered questions; the existing `EventOption` table gains a nullable `QuestionId` so legacy flat options keep working untouched. Two new pure validator classes — one for write-time structure, one for answer-time reachability — hold all new rules and are testable without a database. The join payload keeps its existing `chosenOptions: int[]` shape; only server-side validation changes.

**Tech Stack:** .NET 10, EF Core (SQL Server), AutoMapper, NUnit + NSubstitute + EF Core InMemory, Aspire AppHost for local orchestration.

**Spec:** `docs/superpowers/specs/2026-08-17-event-sign-up-steps-backend-design.md`

## Global Constraints

- Questions per event: **20**. Options per question: **30**. Question title length: **100**. Option label length: **100**. Conditional chain depth: **5**.
- Depth counting: `ShowIfOptionId == null` is depth 0; a question whose condition points at an option owned by a depth-*N* question is depth *N*+1. Depth > 5 is rejected.
- `Order` is 0-based and contiguous within its parent. The server may renumber defensively but must **not** reorder.
- Legacy options (`QuestionId == null`) keep their current behaviour and stay capped by `Event.MaxChoices`.
- The join payload shape does **not** change: `chosenOptions: int[]` remains a flat list of leaf option IDs.
- New error codes continue the numeric sequence in `PremiumErrorCodes` — the last used is `232`, so new codes start at **233**.
- New tests use `Assert.That` (NUnit constraint model), which is already used in ~104 places in the test project.
- Branch: `feat/event-sign-up-steps`, already created off `origin/master`.

---

## Environment (already done — verify only)

These were completed while writing this plan. Verify before Task 1:

```bash
dotnet --list-sdks          # expect 10.0.400
dotnet ef --version         # expect Entity Framework Core tools 10.x
docker info --format "{{.ServerVersion}}"   # expect a version, not an error
pwsh -NoProfile -Command '$PSVersionTable.PSVersion.ToString()'   # expect 7.6.5
```

**Test baseline, measured 2026-08-17 before Task 1:** `Shrooms.Premium.Tests` is
**488 passed / 0 failed / 0 skipped** in ~8s. `dotnet build` on `Shrooms.Presentation.Api` is
**0 errors / 3975 warnings** — the warnings are a pre-existing StyleCop baseline, not something
this plan introduces or needs to fix.

To bring the stack up (needed from Task 1 onward, for applying migrations):

```bash
cd src/api && dotnet run --project Simoona.AppHost
```

`ShroomsDbContextFactory` hardcodes the design-time connection string to
`127.0.0.1,1434` / `sa` / `Password!123`, which matches the Aspire SQL Server container. So
`dotnet ef` commands need no `--connection` argument as long as the AppHost is running.

---

## File Structure

| File | Responsibility |
| ---- | -------------- |
| `Shrooms.Contracts/Enums/EventQuestionSelectType.cs` | **Create.** `Single`/`Multi` enum. |
| `Shrooms.DataLayer.EntityModels/Models/Events/EventQuestion.cs` | **Create.** The entity. |
| `Shrooms.DataLayer.EntityModels/Models/Events/EventOption.cs` | **Modify.** Add `QuestionId`, `Question`, `Order`. |
| `Shrooms.DataLayer/DAL/EntityTypeConfigurations/EventQuestionEntityConfig.cs` | **Create.** Query filter, lengths, FK delete behaviour. |
| `Shrooms.DataLayer/DAL/ShroomsDbContext.cs` | **Modify.** `DbSet<EventQuestion>` + `ApplyConfiguration`. |
| `Shrooms.DataLayer/EFCoreMigrations/*_AddEventQuestions.cs` | **Generated.** Schema change. |
| `Shrooms.Premium/DataTransferObjects/Models/Events/EventQuestionStructureDto.cs` | **Create.** Unresolved write payload (carries `ClientId`). |
| `Shrooms.Premium/DataTransferObjects/Models/Events/ResolvedEventQuestionDto.cs` | **Create.** Post-resolution shape both validators consume. |
| `Shrooms.Premium/Domain/DomainServiceValidators/Events/EventQuestionStructureValidator.cs` | **Create.** Limits + invariants. Pure. |
| `Shrooms.Premium/Domain/DomainServiceValidators/Events/EventAnswerValidator.cs` | **Create.** Reachability + 4 answer reasons. Pure. |
| `Shrooms.Premium/Domain/DomainExceptions/Event/EventAnswersInvalidException.cs` | **Create.** Carries structured errors. |
| `Shrooms.Premium/Constants/PremiumErrorCodes.cs` | **Modify.** Codes 233–239. |

Validators are separate classes rather than additions to `EventValidationService` (already 371
lines, unrelated concerns) specifically because they are pure functions over a question tree —
no `IUnitOfWork2`, no DbSet, no mocking needed to test them.

---

### Task 1: Schema — entity, configuration, migration

**Files:**
- Create: `src/api/Shrooms.Contracts/Enums/EventQuestionSelectType.cs`
- Create: `src/api/Shrooms.DataLayer.EntityModels/Models/Events/EventQuestion.cs`
- Modify: `src/api/Shrooms.DataLayer.EntityModels/Models/Events/EventOption.cs`
- Create: `src/api/Shrooms.DataLayer/DAL/EntityTypeConfigurations/EventQuestionEntityConfig.cs`
- Modify: `src/api/Shrooms.DataLayer/DAL/ShroomsDbContext.cs` (DbSet near line 91, `ApplyConfiguration` near line 232)
- Test: `src/api/Shrooms.Premium.Tests/DataLayer/EventQuestionModelTests.cs`

**Interfaces:**
- Consumes: nothing.
- Produces: `EventQuestion` entity with `Id:int`, `EventId:Guid`, `Title:string`, `Order:int`, `SelectType:EventQuestionSelectType`, `IsRequired:bool`, `ShowIfOptionId:int?`; `EventOption.QuestionId:int?` and `EventOption.Order:int`; enum `EventQuestionSelectType { Single = 0, Multi = 1 }`.

- [ ] **Step 1: Write the failing test**

Create `src/api/Shrooms.Premium.Tests/DataLayer/EventQuestionModelTests.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models.Events;

namespace Shrooms.Premium.Tests.DataLayer
{
    public class EventQuestionModelTests
    {
        private ShroomsDbContext _context;

        [SetUp]
        public void TestInitializer()
        {
            var options = new DbContextOptionsBuilder<ShroomsDbContext>()
                .UseInMemoryDatabase(databaseName: "EventQuestionModelTests")
                .Options;

            _context = new ShroomsDbContext(options);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public void Should_Register_EventQuestion_Entity()
        {
            var entityType = _context.Model.FindEntityType(typeof(EventQuestion));

            Assert.That(entityType, Is.Not.Null);
        }

        [Test]
        public void Should_Limit_EventQuestion_Title_To_100_Characters()
        {
            var entityType = _context.Model.FindEntityType(typeof(EventQuestion));

            var title = entityType.FindProperty(nameof(EventQuestion.Title));

            Assert.That(title.GetMaxLength(), Is.EqualTo(100));
            Assert.That(title.IsNullable, Is.False);
        }

        [Test]
        public void Should_Make_EventOption_QuestionId_Nullable_For_Legacy_Options()
        {
            var entityType = _context.Model.FindEntityType(typeof(EventOption));

            var questionId = entityType.FindProperty(nameof(EventOption.QuestionId));

            Assert.That(questionId, Is.Not.Null);
            Assert.That(questionId.IsNullable, Is.True);
        }

        [Test]
        public void Should_Restrict_Delete_On_ShowIfOption_To_Protect_The_Question_Tree()
        {
            var entityType = _context.Model.FindEntityType(typeof(EventQuestion));

            var showIf = entityType.FindNavigation(nameof(EventQuestion.ShowIfOption));

            Assert.That(showIf.ForeignKey.DeleteBehavior, Is.EqualTo(DeleteBehavior.Restrict));
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test src/api/Shrooms.Premium.Tests/Shrooms.Premium.Tests.csproj --filter "FullyQualifiedName~EventQuestionModelTests" --nologo`

Expected: FAIL — compile error, `EventQuestion` does not exist.

- [ ] **Step 3: Create the enum**

Create `src/api/Shrooms.Contracts/Enums/EventQuestionSelectType.cs`:

```csharp
namespace Shrooms.Contracts.Enums
{
    public enum EventQuestionSelectType
    {
        Single = 0,
        Multi = 1
    }
}
```

- [ ] **Step 4: Create the entity**

Create `src/api/Shrooms.DataLayer.EntityModels/Models/Events/EventQuestion.cs`:

```csharp
using System;
using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Events
{
    public class EventQuestion : SoftDeletableModel
    {
        public Guid EventId { get; set; }
        public virtual Event Event { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public EventQuestionSelectType SelectType { get; set; }
        public bool IsRequired { get; set; }

        /// <summary>
        /// Null means the question is always shown. Otherwise the question is shown only when
        /// this option is chosen. The referenced option always belongs to a question with a
        /// strictly lower <see cref="Order"/>, which makes cycles impossible.
        /// </summary>
        public int? ShowIfOptionId { get; set; }
        public virtual EventOption ShowIfOption { get; set; }

        public virtual ICollection<EventOption> Options { get; set; }
    }
}
```

- [ ] **Step 5: Add the two columns to EventOption**

Modify `src/api/Shrooms.DataLayer.EntityModels/Models/Events/EventOption.cs` to read:

```csharp
using System;
using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Events
{
    public class EventOption : SoftDeletableModel
    {
        public Guid EventId { get; set; }
        public virtual Event Event { get; set; }
        public string Option { get; set; }
        public OptionRules Rule { get; set; }

        /// <summary>
        /// Null means this is a legacy flat option, capped by <c>Event.MaxChoices</c>.
        /// </summary>
        public int? QuestionId { get; set; }
        public virtual EventQuestion Question { get; set; }

        public int Order { get; set; }

        public virtual ICollection<EventParticipant> EventParticipants { get; set; }
    }
}
```

- [ ] **Step 6: Create the EF configuration**

Create `src/api/Shrooms.DataLayer/DAL/EntityTypeConfigurations/EventQuestionEntityConfig.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Events;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    public class EventQuestionEntityConfig : IEntityTypeConfiguration<EventQuestion>
    {
        public void Configure(EntityTypeBuilder<EventQuestion> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(e => e.Event)
                .WithMany()
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict, not Cascade: deleting a trigger option must not silently rewrite the
            // question tree by nulling conditions. The structure validator rejects that state.
            builder.HasOne(e => e.ShowIfOption)
                .WithMany()
                .HasForeignKey(e => e.ShowIfOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
```

- [ ] **Step 7: Register in the DbContext**

In `src/api/Shrooms.DataLayer/DAL/ShroomsDbContext.cs`, add after the `EventOptions` DbSet (near line 91):

```csharp
        public virtual DbSet<EventQuestion> EventQuestions { get; set; }
```

and after `modelBuilder.ApplyConfiguration(new EventOptionEntityConfig());` (near line 232):

```csharp
            modelBuilder.ApplyConfiguration(new EventQuestionEntityConfig());
```

- [ ] **Step 8: Run tests to verify they pass**

Run: `dotnet test src/api/Shrooms.Premium.Tests/Shrooms.Premium.Tests.csproj --filter "FullyQualifiedName~EventQuestionModelTests" --nologo`

Expected: PASS, 4 tests.

- [ ] **Step 9: Generate the migration**

Make sure the Aspire stack is running (`dotnet run --project src/api/Simoona.AppHost`), then:

```bash
dotnet ef migrations add AddEventQuestions \
  --project src/api/Shrooms.DataLayer \
  --context ShroomsDbContext \
  --output-dir EFCoreMigrations
```

Expected: two new files in `src/api/Shrooms.DataLayer/EFCoreMigrations/`.

Open the generated `Up()` and confirm it contains `CreateTable(name: "EventQuestions"…)` plus
`AddColumn` for `QuestionId` and `Order` on `EventOptions`. If it contains anything else — drops,
renames, unrelated tables — the model has drifted; stop and investigate rather than applying it.

- [ ] **Step 10: Apply the migration**

```bash
dotnet ef database update --project src/api/Shrooms.DataLayer --context ShroomsDbContext
```

Expected: `Done.` and no error.

- [ ] **Step 11: Commit**

```bash
git add src/api/Shrooms.Contracts/Enums/EventQuestionSelectType.cs \
        src/api/Shrooms.DataLayer.EntityModels/Models/Events/EventQuestion.cs \
        src/api/Shrooms.DataLayer.EntityModels/Models/Events/EventOption.cs \
        src/api/Shrooms.DataLayer/DAL/EntityTypeConfigurations/EventQuestionEntityConfig.cs \
        src/api/Shrooms.DataLayer/DAL/ShroomsDbContext.cs \
        src/api/Shrooms.DataLayer/EFCoreMigrations/ \
        src/api/Shrooms.Premium.Tests/DataLayer/EventQuestionModelTests.cs
git commit -m "feat(events): EventQuestion schema for sign-up steps"
```

---

### Task 2: Structure validator — limits and invariants

**Files:**
- Create: `src/api/Shrooms.Premium/DataTransferObjects/Models/Events/EventQuestionStructureDto.cs`
- Create: `src/api/Shrooms.Premium/DataTransferObjects/Models/Events/ResolvedEventQuestionDto.cs`
- Create: `src/api/Shrooms.Premium/Domain/DomainServiceValidators/Events/EventQuestionStructureValidator.cs`
- Modify: `src/api/Shrooms.Premium/Constants/PremiumErrorCodes.cs`
- Test: `src/api/Shrooms.Premium.Tests/DomainService/EventServices/EventQuestionStructureValidatorTests.cs`

**Interfaces:**
- Consumes: `EventQuestionSelectType` (Task 1).
- Produces:
  - `EventQuestionStructureDto { int? Id, string ClientId, string Title, int Order, EventQuestionSelectType SelectType, bool IsRequired, int? ShowIfOptionId, string ShowIfOptionClientId, IList<EventQuestionOptionStructureDto> Options }`
  - `EventQuestionOptionStructureDto { int? Id, string ClientId, string Name, int Order, OptionRules Rule }`
  - `ResolvedEventQuestionDto { int QuestionId, int Order, EventQuestionSelectType SelectType, bool IsRequired, int? ShowIfOptionId, IReadOnlyCollection<int> OptionIds }`
  - `EventQuestionStructureValidator.ValidatePayload(IList<EventQuestionStructureDto>)` — throws `EventException`
  - `EventQuestionStructureValidator.ValidateResolved(IReadOnlyList<ResolvedEventQuestionDto>)` — throws `EventException`

Validation is split in two because `ClientId` references cannot be checked against real IDs until
after insert. `ValidatePayload` runs on the raw request; `ValidateResolved` runs after IDs exist.

- [ ] **Step 1: Add the error codes**

In `src/api/Shrooms.Premium/Constants/PremiumErrorCodes.cs`, after
`EventReminderCannotBeAdded = "232";` add:

```csharp
        public const string EventQuestionLimitExceeded = "233";
        public const string EventQuestionOptionLimitExceeded = "234";
        public const string EventQuestionTitleInvalid = "235";
        public const string EventQuestionOptionNameInvalid = "236";
        public const string EventQuestionConditionAmbiguous = "237";
        public const string EventQuestionConditionInvalid = "238";
        public const string EventQuestionDepthExceeded = "239";
```

- [ ] **Step 2: Create the DTOs**

Create `src/api/Shrooms.Premium/DataTransferObjects/Models/Events/EventQuestionStructureDto.cs`:

```csharp
using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventQuestionStructureDto
    {
        public int? Id { get; set; }

        /// <summary>Client-generated, required when <see cref="Id"/> is null.</summary>
        public string ClientId { get; set; }

        public string Title { get; set; }
        public int Order { get; set; }
        public EventQuestionSelectType SelectType { get; set; }
        public bool IsRequired { get; set; }

        /// <summary>Set when the trigger option already exists in the database.</summary>
        public int? ShowIfOptionId { get; set; }

        /// <summary>Set when the trigger option is being inserted in this same request.</summary>
        public string ShowIfOptionClientId { get; set; }

        public IList<EventQuestionOptionStructureDto> Options { get; set; } = new List<EventQuestionOptionStructureDto>();
    }

    public class EventQuestionOptionStructureDto
    {
        public int? Id { get; set; }
        public string ClientId { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public OptionRules Rule { get; set; }
    }
}
```

Create `src/api/Shrooms.Premium/DataTransferObjects/Models/Events/ResolvedEventQuestionDto.cs`:

```csharp
using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    /// <summary>
    /// A question whose IDs all exist. Both validators consume this shape, which is why it holds
    /// plain IDs rather than entities — it keeps them free of any database dependency.
    /// </summary>
    public class ResolvedEventQuestionDto
    {
        public int QuestionId { get; set; }
        public int Order { get; set; }
        public EventQuestionSelectType SelectType { get; set; }
        public bool IsRequired { get; set; }
        public int? ShowIfOptionId { get; set; }
        public IReadOnlyCollection<int> OptionIds { get; set; } = new List<int>();
    }
}
```

- [ ] **Step 3: Write the failing tests**

Create `src/api/Shrooms.Premium.Tests/DomainService/EventServices/EventQuestionStructureValidatorTests.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    public class EventQuestionStructureValidatorTests
    {
        private EventQuestionStructureValidator _validator;

        [SetUp]
        public void TestInitializer()
        {
            _validator = new EventQuestionStructureValidator();
        }

        private static EventQuestionStructureDto Question(string clientId, int order, string title = "Pick your dish")
        {
            return new EventQuestionStructureDto
            {
                Id = null,
                ClientId = clientId,
                Title = title,
                Order = order,
                SelectType = EventQuestionSelectType.Single,
                IsRequired = true,
                Options = new List<EventQuestionOptionStructureDto>
                {
                    new EventQuestionOptionStructureDto { ClientId = clientId + "-o1", Name = "Pizza", Order = 0 }
                }
            };
        }

        [Test]
        public void Should_Accept_A_Valid_Flat_Payload()
        {
            var questions = new List<EventQuestionStructureDto> { Question("q1", 0) };

            Assert.DoesNotThrow(() => _validator.ValidatePayload(questions));
        }

        [Test]
        public void Should_Reject_More_Than_20_Questions()
        {
            var questions = Enumerable.Range(0, 21)
                .Select(i => Question("q" + i, i))
                .ToList();

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(questions));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionLimitExceeded));
        }

        [Test]
        public void Should_Accept_Exactly_20_Questions()
        {
            var questions = Enumerable.Range(0, 20)
                .Select(i => Question("q" + i, i))
                .ToList();

            Assert.DoesNotThrow(() => _validator.ValidatePayload(questions));
        }

        [Test]
        public void Should_Reject_More_Than_30_Options_In_One_Question()
        {
            var question = Question("q1", 0);
            question.Options = Enumerable.Range(0, 31)
                .Select(i => new EventQuestionOptionStructureDto { ClientId = "o" + i, Name = "Option", Order = i })
                .ToList();

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { question }));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionOptionLimitExceeded));
        }

        [Test]
        public void Should_Reject_A_Title_Longer_Than_100_Characters()
        {
            var question = Question("q1", 0, new string('x', 101));

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { question }));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionTitleInvalid));
        }

        [Test]
        public void Should_Reject_An_Empty_Title()
        {
            var question = Question("q1", 0, "   ");

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { question }));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionTitleInvalid));
        }

        [Test]
        public void Should_Reject_An_Option_Name_Longer_Than_100_Characters()
        {
            var question = Question("q1", 0);
            question.Options[0].Name = new string('x', 101);

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { question }));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionOptionNameInvalid));
        }

        [Test]
        public void Should_Reject_A_Condition_That_Sets_Both_OptionId_And_OptionClientId()
        {
            var question = Question("q2", 1);
            question.ShowIfOptionId = 41;
            question.ShowIfOptionClientId = "q1-o1";

            var questions = new List<EventQuestionStructureDto> { Question("q1", 0), question };

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(questions));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionConditionAmbiguous));
        }

        [Test]
        public void Should_Reject_A_ClientId_Reference_That_Matches_No_Option_In_The_Payload()
        {
            var question = Question("q2", 1);
            question.ShowIfOptionClientId = "does-not-exist";

            var questions = new List<EventQuestionStructureDto> { Question("q1", 0), question };

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(questions));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionConditionInvalid));
        }

        [Test]
        public void Should_Require_A_ClientId_When_Id_Is_Null()
        {
            var question = Question("q1", 0);
            question.ClientId = null;

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { question }));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionConditionAmbiguous));
        }

        [Test]
        public void Should_Reject_A_Condition_Pointing_At_A_Question_With_A_Higher_Order()
        {
            var resolved = new List<ResolvedEventQuestionDto>
            {
                new ResolvedEventQuestionDto { QuestionId = 1, Order = 0, ShowIfOptionId = 20, OptionIds = new[] { 10 } },
                new ResolvedEventQuestionDto { QuestionId = 2, Order = 1, ShowIfOptionId = null, OptionIds = new[] { 20 } }
            };

            var ex = Assert.Throws<EventException>(() => _validator.ValidateResolved(resolved));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionConditionInvalid));
        }

        [Test]
        public void Should_Accept_A_Condition_Pointing_At_A_Question_With_A_Lower_Order()
        {
            var resolved = new List<ResolvedEventQuestionDto>
            {
                new ResolvedEventQuestionDto { QuestionId = 1, Order = 0, ShowIfOptionId = null, OptionIds = new[] { 10 } },
                new ResolvedEventQuestionDto { QuestionId = 2, Order = 1, ShowIfOptionId = 10, OptionIds = new[] { 20 } }
            };

            Assert.DoesNotThrow(() => _validator.ValidateResolved(resolved));
        }

        [Test]
        public void Should_Accept_A_Conditional_Chain_Exactly_5_Deep()
        {
            var resolved = BuildChain(5);

            Assert.DoesNotThrow(() => _validator.ValidateResolved(resolved));
        }

        [Test]
        public void Should_Reject_A_Conditional_Chain_6_Deep()
        {
            var resolved = BuildChain(6);

            var ex = Assert.Throws<EventException>(() => _validator.ValidateResolved(resolved));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionDepthExceeded));
        }

        /// <summary>
        /// Question 0 is always shown (depth 0); each subsequent question is triggered by the
        /// previous question's option, so question N sits at depth N.
        /// </summary>
        private static List<ResolvedEventQuestionDto> BuildChain(int depth)
        {
            var questions = new List<ResolvedEventQuestionDto>
            {
                new ResolvedEventQuestionDto { QuestionId = 1, Order = 0, ShowIfOptionId = null, OptionIds = new[] { 10 } }
            };

            for (var i = 1; i <= depth; i++)
            {
                questions.Add(new ResolvedEventQuestionDto
                {
                    QuestionId = i + 1,
                    Order = i,
                    ShowIfOptionId = (i * 10),
                    OptionIds = new[] { (i + 1) * 10 }
                });
            }

            return questions;
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they fail**

Run: `dotnet test src/api/Shrooms.Premium.Tests/Shrooms.Premium.Tests.csproj --filter "FullyQualifiedName~EventQuestionStructureValidatorTests" --nologo`

Expected: FAIL — compile error, `EventQuestionStructureValidator` does not exist.

- [ ] **Step 5: Write the validator**

Create `src/api/Shrooms.Premium/Domain/DomainServiceValidators/Events/EventQuestionStructureValidator.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;

namespace Shrooms.Premium.Domain.DomainServiceValidators.Events
{
    /// <summary>
    /// Write-time structural rules for the sign-up question tree. Pure — no database access —
    /// so it can be unit tested directly.
    /// </summary>
    public class EventQuestionStructureValidator : IEventQuestionStructureValidator
    {
        public const int MaxQuestionsPerEvent = 20;
        public const int MaxOptionsPerQuestion = 30;
        public const int MaxTitleLength = 100;
        public const int MaxOptionNameLength = 100;
        public const int MaxConditionalDepth = 5;

        /// <summary>
        /// Checks everything decidable before rows are inserted: limits, lengths, and that every
        /// condition names exactly one trigger that exists somewhere in this payload.
        /// </summary>
        public void ValidatePayload(IList<EventQuestionStructureDto> questions)
        {
            if (questions == null || questions.Count == 0)
            {
                return;
            }

            if (questions.Count > MaxQuestionsPerEvent)
            {
                throw new EventException(PremiumErrorCodes.EventQuestionLimitExceeded);
            }

            foreach (var question in questions)
            {
                ValidateQuestionShape(question);
            }

            var optionClientIds = questions
                .SelectMany(q => q.Options)
                .Where(o => o.ClientId != null)
                .Select(o => o.ClientId)
                .ToHashSet();

            foreach (var question in questions)
            {
                ValidateCondition(question, optionClientIds);
            }
        }

        /// <summary>
        /// Checks the rules that need real IDs: a condition must point at an option owned by a
        /// question with a strictly lower order, and the conditional chain must not exceed
        /// <see cref="MaxConditionalDepth"/>.
        /// </summary>
        public void ValidateResolved(IReadOnlyList<ResolvedEventQuestionDto> questions)
        {
            if (questions == null || questions.Count == 0)
            {
                return;
            }

            var ordered = questions.OrderBy(q => q.Order).ToList();

            var ownerByOptionId = new Dictionary<int, ResolvedEventQuestionDto>();
            foreach (var question in ordered)
            {
                foreach (var optionId in question.OptionIds)
                {
                    ownerByOptionId[optionId] = question;
                }
            }

            var depthByQuestionId = new Dictionary<int, int>();

            foreach (var question in ordered)
            {
                if (question.ShowIfOptionId == null)
                {
                    depthByQuestionId[question.QuestionId] = 0;
                    continue;
                }

                if (!ownerByOptionId.TryGetValue(question.ShowIfOptionId.Value, out var owner) ||
                    owner.Order >= question.Order)
                {
                    throw new EventException(PremiumErrorCodes.EventQuestionConditionInvalid);
                }

                // The owner sits at a lower order, so it has already been assigned a depth.
                var depth = depthByQuestionId[owner.QuestionId] + 1;

                if (depth > MaxConditionalDepth)
                {
                    throw new EventException(PremiumErrorCodes.EventQuestionDepthExceeded);
                }

                depthByQuestionId[question.QuestionId] = depth;
            }
        }

        private static void ValidateQuestionShape(EventQuestionStructureDto question)
        {
            if (question.Id == null && string.IsNullOrWhiteSpace(question.ClientId))
            {
                throw new EventException(PremiumErrorCodes.EventQuestionConditionAmbiguous);
            }

            if (string.IsNullOrWhiteSpace(question.Title) || question.Title.Length > MaxTitleLength)
            {
                throw new EventException(PremiumErrorCodes.EventQuestionTitleInvalid);
            }

            var options = question.Options ?? new List<EventQuestionOptionStructureDto>();

            if (options.Count > MaxOptionsPerQuestion)
            {
                throw new EventException(PremiumErrorCodes.EventQuestionOptionLimitExceeded);
            }

            foreach (var option in options)
            {
                if (string.IsNullOrWhiteSpace(option.Name) || option.Name.Length > MaxOptionNameLength)
                {
                    throw new EventException(PremiumErrorCodes.EventQuestionOptionNameInvalid);
                }
            }
        }

        private static void ValidateCondition(EventQuestionStructureDto question, HashSet<string> optionClientIds)
        {
            var hasId = question.ShowIfOptionId != null;
            var hasClientId = !string.IsNullOrWhiteSpace(question.ShowIfOptionClientId);

            if (hasId && hasClientId)
            {
                throw new EventException(PremiumErrorCodes.EventQuestionConditionAmbiguous);
            }

            if (hasClientId && !optionClientIds.Contains(question.ShowIfOptionClientId))
            {
                throw new EventException(PremiumErrorCodes.EventQuestionConditionInvalid);
            }
        }
    }
}
```

- [ ] **Step 6: Write the interface**

Create `src/api/Shrooms.Premium/Domain/DomainServiceValidators/Events/IEventQuestionStructureValidator.cs`:

```csharp
using System.Collections.Generic;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.DomainServiceValidators.Events
{
    public interface IEventQuestionStructureValidator
    {
        void ValidatePayload(IList<EventQuestionStructureDto> questions);

        void ValidateResolved(IReadOnlyList<ResolvedEventQuestionDto> questions);
    }
}
```

- [ ] **Step 7: Run tests to verify they pass**

Run: `dotnet test src/api/Shrooms.Premium.Tests/Shrooms.Premium.Tests.csproj --filter "FullyQualifiedName~EventQuestionStructureValidatorTests" --nologo`

Expected: PASS, 15 tests.

- [ ] **Step 8: Commit**

```bash
git add src/api/Shrooms.Premium/Constants/PremiumErrorCodes.cs \
        src/api/Shrooms.Premium/DataTransferObjects/Models/Events/EventQuestionStructureDto.cs \
        src/api/Shrooms.Premium/DataTransferObjects/Models/Events/ResolvedEventQuestionDto.cs \
        src/api/Shrooms.Premium/Domain/DomainServiceValidators/Events/EventQuestionStructureValidator.cs \
        src/api/Shrooms.Premium/Domain/DomainServiceValidators/Events/IEventQuestionStructureValidator.cs \
        src/api/Shrooms.Premium.Tests/DomainService/EventServices/EventQuestionStructureValidatorTests.cs
git commit -m "feat(events): validate sign-up question structure"
```

---

### Task 3: Answer validator — reachability and the four reasons

**Files:**
- Create: `src/api/Shrooms.Premium/DataTransferObjects/Models/Events/EventAnswerErrorDto.cs`
- Create: `src/api/Shrooms.Premium/Domain/DomainExceptions/Event/EventAnswersInvalidException.cs`
- Create: `src/api/Shrooms.Premium/Domain/DomainServiceValidators/Events/EventAnswerValidator.cs`
- Create: `src/api/Shrooms.Premium/Domain/DomainServiceValidators/Events/IEventAnswerValidator.cs`
- Test: `src/api/Shrooms.Premium.Tests/DomainService/EventServices/EventAnswerValidatorTests.cs`

**Interfaces:**
- Consumes: `ResolvedEventQuestionDto` (Task 2).
- Produces:
  - `enum EventAnswerErrorReason { UnknownOption, TooManyAnswers, RequiredAnswerMissing, AnswerForHiddenQuestion }`
  - `EventAnswerErrorDto { int? QuestionId, EventAnswerErrorReason Reason }`
  - `EventAnswersInvalidException` with `IReadOnlyList<EventAnswerErrorDto> Errors` and `Code == "EventAnswersInvalid"`
  - `EventAnswerValidator.Validate(IReadOnlyList<ResolvedEventQuestionDto> questions, IReadOnlyCollection<int> chosenOptionIds, IReadOnlyCollection<int> legacyOptionIds)` — throws `EventAnswersInvalidException`

**Note on `QuestionId` nullability.** The frontend contract shows `questionId` as an int on every
error. `UnknownOption` has no owning question by definition, so `QuestionId` is `int?` and is
`null` for that reason only. JSON-serialises as `"questionId": null`. Flag this to the frontend.

- [ ] **Step 1: Write the failing tests**

Create `src/api/Shrooms.Premium.Tests/DomainService/EventServices/EventAnswerValidatorTests.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    public class EventAnswerValidatorTests
    {
        private EventAnswerValidator _validator;

        [SetUp]
        public void TestInitializer()
        {
            _validator = new EventAnswerValidator();
        }

        /// <summary>
        /// q1 "Pick your dish" (required, single): options 10 Pizza, 11 Pasta.
        /// q2 "Which pizza?"   (required, single, shown if 10): options 20, 21.
        /// q3 "Anything else?" (optional, multi, always shown): options 30, 31.
        /// </summary>
        private static List<ResolvedEventQuestionDto> FoodTree()
        {
            return new List<ResolvedEventQuestionDto>
            {
                new ResolvedEventQuestionDto
                {
                    QuestionId = 1, Order = 0, SelectType = EventQuestionSelectType.Single,
                    IsRequired = true, ShowIfOptionId = null, OptionIds = new[] { 10, 11 }
                },
                new ResolvedEventQuestionDto
                {
                    QuestionId = 2, Order = 1, SelectType = EventQuestionSelectType.Single,
                    IsRequired = true, ShowIfOptionId = 10, OptionIds = new[] { 20, 21 }
                },
                new ResolvedEventQuestionDto
                {
                    QuestionId = 3, Order = 2, SelectType = EventQuestionSelectType.Multi,
                    IsRequired = false, ShowIfOptionId = null, OptionIds = new[] { 30, 31 }
                }
            };
        }

        [Test]
        public void Should_Accept_A_Complete_Branch()
        {
            Assert.DoesNotThrow(() => _validator.Validate(FoodTree(), new[] { 10, 20 }, new int[0]));
        }

        [Test]
        public void Should_Accept_A_Branch_That_Skips_The_Conditional_Question()
        {
            // Pasta chosen, so "Which pizza?" is not reachable and needs no answer.
            Assert.DoesNotThrow(() => _validator.Validate(FoodTree(), new[] { 11 }, new int[0]));
        }

        [Test]
        public void Should_Reject_An_Option_That_Does_Not_Belong_To_The_Event()
        {
            var ex = Assert.Throws<EventAnswersInvalidException>(
                () => _validator.Validate(FoodTree(), new[] { 10, 20, 999 }, new int[0]));

            Assert.That(ex.Errors.Single().Reason, Is.EqualTo(EventAnswerErrorReason.UnknownOption));
            Assert.That(ex.Errors.Single().QuestionId, Is.Null);
        }

        [Test]
        public void Should_Reject_Two_Answers_To_A_Single_Select_Question()
        {
            var ex = Assert.Throws<EventAnswersInvalidException>(
                () => _validator.Validate(FoodTree(), new[] { 10, 11, 20 }, new int[0]));

            Assert.That(ex.Errors.Any(e => e.QuestionId == 1 && e.Reason == EventAnswerErrorReason.TooManyAnswers), Is.True);
        }

        [Test]
        public void Should_Accept_Two_Answers_To_A_Multi_Select_Question()
        {
            Assert.DoesNotThrow(() => _validator.Validate(FoodTree(), new[] { 11, 30, 31 }, new int[0]));
        }

        [Test]
        public void Should_Reject_A_Missing_Answer_To_A_Reachable_Required_Question()
        {
            // Pizza chosen but "Which pizza?" left unanswered.
            var ex = Assert.Throws<EventAnswersInvalidException>(
                () => _validator.Validate(FoodTree(), new[] { 10 }, new int[0]));

            Assert.That(ex.Errors.Single().QuestionId, Is.EqualTo(2));
            Assert.That(ex.Errors.Single().Reason, Is.EqualTo(EventAnswerErrorReason.RequiredAnswerMissing));
        }

        [Test]
        public void Should_Reject_An_Answer_To_A_Hidden_Question()
        {
            // Pasta chosen, yet a pizza sub-option was answered.
            var ex = Assert.Throws<EventAnswersInvalidException>(
                () => _validator.Validate(FoodTree(), new[] { 11, 20 }, new int[0]));

            Assert.That(ex.Errors.Single().QuestionId, Is.EqualTo(2));
            Assert.That(ex.Errors.Single().Reason, Is.EqualTo(EventAnswerErrorReason.AnswerForHiddenQuestion));
        }

        [Test]
        public void Should_Report_Every_Failing_Question_Not_Just_The_First()
        {
            var questions = new List<ResolvedEventQuestionDto>
            {
                new ResolvedEventQuestionDto
                {
                    QuestionId = 1, Order = 0, SelectType = EventQuestionSelectType.Single,
                    IsRequired = true, ShowIfOptionId = null, OptionIds = new[] { 10 }
                },
                new ResolvedEventQuestionDto
                {
                    QuestionId = 2, Order = 1, SelectType = EventQuestionSelectType.Single,
                    IsRequired = true, ShowIfOptionId = null, OptionIds = new[] { 20 }
                }
            };

            var ex = Assert.Throws<EventAnswersInvalidException>(
                () => _validator.Validate(questions, new int[0], new int[0]));

            Assert.That(ex.Errors.Count, Is.EqualTo(2));
        }

        [Test]
        public void Should_Treat_A_Question_As_Hidden_When_Its_Trigger_Question_Is_Itself_Hidden()
        {
            // q3 is triggered by an option of q2, but q2 is hidden because q1 chose 11.
            var questions = FoodTree();
            questions.Add(new ResolvedEventQuestionDto
            {
                QuestionId = 4, Order = 3, SelectType = EventQuestionSelectType.Single,
                IsRequired = true, ShowIfOptionId = 20, OptionIds = new[] { 40 }
            });

            var ex = Assert.Throws<EventAnswersInvalidException>(
                () => _validator.Validate(questions, new[] { 11, 40 }, new int[0]));

            Assert.That(ex.Errors.Any(e => e.QuestionId == 4 && e.Reason == EventAnswerErrorReason.AnswerForHiddenQuestion), Is.True);
        }

        [Test]
        public void Should_Accept_Legacy_Flat_Options_Alongside_Questions()
        {
            Assert.DoesNotThrow(() => _validator.Validate(FoodTree(), new[] { 11, 500 }, new[] { 500, 501 }));
        }

        [Test]
        public void Should_Accept_Legacy_Only_Events_With_No_Questions()
        {
            Assert.DoesNotThrow(() => _validator.Validate(new List<ResolvedEventQuestionDto>(), new[] { 500 }, new[] { 500 }));
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test src/api/Shrooms.Premium.Tests/Shrooms.Premium.Tests.csproj --filter "FullyQualifiedName~EventAnswerValidatorTests" --nologo`

Expected: FAIL — compile error, `EventAnswerValidator` does not exist.

- [ ] **Step 3: Create the error DTO and reason enum**

Create `src/api/Shrooms.Premium/DataTransferObjects/Models/Events/EventAnswerErrorDto.cs`:

```csharp
namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public enum EventAnswerErrorReason
    {
        UnknownOption,
        TooManyAnswers,
        RequiredAnswerMissing,
        AnswerForHiddenQuestion
    }

    public class EventAnswerErrorDto
    {
        /// <summary>
        /// Null only for <see cref="EventAnswerErrorReason.UnknownOption"/>, which by definition
        /// has no owning question.
        /// </summary>
        public int? QuestionId { get; set; }

        public EventAnswerErrorReason Reason { get; set; }
    }
}
```

- [ ] **Step 4: Create the exception**

Create `src/api/Shrooms.Premium/Domain/DomainExceptions/Event/EventAnswersInvalidException.cs`:

```csharp
using System.Collections.Generic;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.DomainExceptions.Event
{
    /// <summary>
    /// Thrown when sign-up answers do not satisfy the question tree. Carries a machine-readable
    /// error list so the attendee wizard can jump to the offending step instead of showing a
    /// generic message. Derives from <see cref="EventException"/> so existing catch blocks still
    /// work if a caller has not been updated.
    /// </summary>
    public class EventAnswersInvalidException : EventException
    {
        public const string ErrorCode = "EventAnswersInvalid";

        public EventAnswersInvalidException(IReadOnlyList<EventAnswerErrorDto> errors)
            : base(ErrorCode)
        {
            Errors = errors;
        }

        public IReadOnlyList<EventAnswerErrorDto> Errors { get; }
    }
}
```

- [ ] **Step 5: Write the validator**

Create `src/api/Shrooms.Premium/Domain/DomainServiceValidators/Events/EventAnswerValidator.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;
using Shrooms.Contracts.Enums;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;

namespace Shrooms.Premium.Domain.DomainServiceValidators.Events
{
    /// <summary>
    /// Answer-time rules for the sign-up question tree. Pure — no database access.
    /// Collects every failure rather than throwing on the first, so the wizard can show them all.
    /// </summary>
    public class EventAnswerValidator : IEventAnswerValidator
    {
        public void Validate(
            IReadOnlyList<ResolvedEventQuestionDto> questions,
            IReadOnlyCollection<int> chosenOptionIds,
            IReadOnlyCollection<int> legacyOptionIds)
        {
            var chosen = (chosenOptionIds ?? new int[0]).ToHashSet();
            var legacy = (legacyOptionIds ?? new int[0]).ToHashSet();
            var ordered = (questions ?? new List<ResolvedEventQuestionDto>()).OrderBy(q => q.Order).ToList();

            var errors = new List<EventAnswerErrorDto>();

            var knownOptionIds = ordered.SelectMany(q => q.OptionIds).Concat(legacy).ToHashSet();

            foreach (var unknown in chosen.Where(id => !knownOptionIds.Contains(id)))
            {
                errors.Add(new EventAnswerErrorDto
                {
                    QuestionId = null,
                    Reason = EventAnswerErrorReason.UnknownOption
                });
            }

            // Triggers always live at a lower order, so a single forward pass resolves
            // reachability: by the time a question is visited, its trigger's owner is settled.
            var reachableByQuestionId = new Dictionary<int, bool>();
            var ownerByOptionId = new Dictionary<int, int>();

            foreach (var question in ordered)
            {
                foreach (var optionId in question.OptionIds)
                {
                    ownerByOptionId[optionId] = question.QuestionId;
                }
            }

            foreach (var question in ordered)
            {
                var reachable = IsReachable(question, chosen, ownerByOptionId, reachableByQuestionId);
                reachableByQuestionId[question.QuestionId] = reachable;

                var answeredHere = question.OptionIds.Count(chosen.Contains);

                if (!reachable)
                {
                    if (answeredHere > 0)
                    {
                        errors.Add(new EventAnswerErrorDto
                        {
                            QuestionId = question.QuestionId,
                            Reason = EventAnswerErrorReason.AnswerForHiddenQuestion
                        });
                    }

                    continue;
                }

                if (question.SelectType == EventQuestionSelectType.Single && answeredHere > 1)
                {
                    errors.Add(new EventAnswerErrorDto
                    {
                        QuestionId = question.QuestionId,
                        Reason = EventAnswerErrorReason.TooManyAnswers
                    });
                }

                if (question.IsRequired && answeredHere == 0)
                {
                    errors.Add(new EventAnswerErrorDto
                    {
                        QuestionId = question.QuestionId,
                        Reason = EventAnswerErrorReason.RequiredAnswerMissing
                    });
                }
            }

            if (errors.Count > 0)
            {
                throw new EventAnswersInvalidException(errors);
            }
        }

        private static bool IsReachable(
            ResolvedEventQuestionDto question,
            HashSet<int> chosen,
            IReadOnlyDictionary<int, int> ownerByOptionId,
            IReadOnlyDictionary<int, bool> reachableByQuestionId)
        {
            if (question.ShowIfOptionId == null)
            {
                return true;
            }

            var triggerId = question.ShowIfOptionId.Value;

            if (!chosen.Contains(triggerId))
            {
                return false;
            }

            // The trigger was chosen, but it only counts if the question owning it was itself
            // shown — otherwise a hidden branch would resurrect its children.
            return ownerByOptionId.TryGetValue(triggerId, out var ownerId) &&
                   reachableByQuestionId.TryGetValue(ownerId, out var ownerReachable) &&
                   ownerReachable;
        }
    }
}
```

- [ ] **Step 6: Write the interface**

Create `src/api/Shrooms.Premium/Domain/DomainServiceValidators/Events/IEventAnswerValidator.cs`:

```csharp
using System.Collections.Generic;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.DomainServiceValidators.Events
{
    public interface IEventAnswerValidator
    {
        void Validate(
            IReadOnlyList<ResolvedEventQuestionDto> questions,
            IReadOnlyCollection<int> chosenOptionIds,
            IReadOnlyCollection<int> legacyOptionIds);
    }
}
```

- [ ] **Step 7: Run tests to verify they pass**

Run: `dotnet test src/api/Shrooms.Premium.Tests/Shrooms.Premium.Tests.csproj --filter "FullyQualifiedName~EventAnswerValidatorTests" --nologo`

Expected: PASS, 11 tests.

- [ ] **Step 8: Commit**

```bash
git add src/api/Shrooms.Premium/DataTransferObjects/Models/Events/EventAnswerErrorDto.cs \
        src/api/Shrooms.Premium/Domain/DomainExceptions/Event/EventAnswersInvalidException.cs \
        src/api/Shrooms.Premium/Domain/DomainServiceValidators/Events/EventAnswerValidator.cs \
        src/api/Shrooms.Premium/Domain/DomainServiceValidators/Events/IEventAnswerValidator.cs \
        src/api/Shrooms.Premium.Tests/DomainService/EventServices/EventAnswerValidatorTests.cs
git commit -m "feat(events): validate sign-up answers against the question graph"
```

---

### Task 4: View models, DTOs and mappings

**Files:**
- Create: `src/api/Shrooms.Premium/Presentation/WebViewModels/Events/EventQuestionViewModel.cs`
- Create: `src/api/Shrooms.Premium/Presentation/WebViewModels/Events/EventQuestionOptionViewModel.cs`
- Modify: `src/api/Shrooms.Premium/Presentation/WebViewModels/Events/CreateEventViewModel.cs`
- Modify: `src/api/Shrooms.Premium/Presentation/WebViewModels/Events/UpdateEventViewModel.cs`
- Modify: `src/api/Shrooms.Premium/DataTransferObjects/Models/Events/EventOptionsDto.cs`
- Modify: `src/api/Shrooms.Premium/Presentation/WebViewModels/Events/EventOptionsViewModel.cs`
- Modify: `src/api/Shrooms.Premium/Presentation/ModelMappings/Profiles/EventsProfile.cs`
- Modify: `src/api/Shrooms.Premium/DataTransferObjects/Models/Events/CreateEventDto.cs`, `EditEventDto.cs`
- Test: `src/api/Shrooms.Premium.Tests/Controllers/ViewModels/EventQuestionMappingTests.cs`

**Interfaces:**
- Consumes: `EventQuestionStructureDto` (Task 2), `EventQuestionSelectType` (Task 1).
- Produces:
  - `EventQuestionViewModel { int? Id, string ClientId, string Title, int Order, EventQuestionSelectType SelectType, bool IsRequired, EventQuestionConditionViewModel ShowIf, IList<EventQuestionOptionViewModel> Options }`
  - `EventQuestionConditionViewModel { int? OptionId, string OptionClientId }`
  - `EventQuestionOptionViewModel { int? Id, string ClientId, string Name, int Order, OptionRules Rule }`
  - `CreateEventViewModel.Questions`, `UpdateEventViewModel.Questions` — `IList<EventQuestionViewModel>`
  - `EventOptionsDto.Questions` / `EventOptionsViewModel.Questions`, `EventOptionsDto.MyChosenOptions` / `EventOptionsViewModel.MyChosenOptions`
  - `CreateEventDto.Questions`, `EditEventDto.Questions` — `IList<EventQuestionStructureDto>`

The wire shape nests the condition (`"showIf": { "optionClientId": "o1" }`) to match the frontend
contract, while the DTO flattens it to `ShowIfOptionId` / `ShowIfOptionClientId` because the
validator and the entity both want flat fields. The mapping profile is where that flattening lives.

- [ ] **Step 1: Write the failing test**

Create `src/api/Shrooms.Premium.Tests/Controllers/ViewModels/EventQuestionMappingTests.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Presentation.ModelMappings.Profiles;
using Shrooms.Premium.Presentation.WebViewModels.Events;

namespace Shrooms.Premium.Tests.Controllers.ViewModels
{
    public class EventQuestionMappingTests
    {
        private IMapper _mapper;

        [SetUp]
        public void TestInitializer()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<EventsProfile>());
            _mapper = config.CreateMapper();
        }

        [Test]
        public void Should_Flatten_A_ClientId_Condition_Onto_The_Dto()
        {
            var viewModel = new EventQuestionViewModel
            {
                Id = null,
                ClientId = "q2",
                Title = "Which pizza?",
                Order = 1,
                SelectType = EventQuestionSelectType.Single,
                IsRequired = true,
                ShowIf = new EventQuestionConditionViewModel { OptionClientId = "o1" },
                Options = new List<EventQuestionOptionViewModel>
                {
                    new EventQuestionOptionViewModel { ClientId = "o3", Name = "Margherita", Order = 0, Rule = OptionRules.Default }
                }
            };

            var dto = _mapper.Map<EventQuestionViewModel, EventQuestionStructureDto>(viewModel);

            Assert.That(dto.ShowIfOptionClientId, Is.EqualTo("o1"));
            Assert.That(dto.ShowIfOptionId, Is.Null);
            Assert.That(dto.Options.Single().Name, Is.EqualTo("Margherita"));
        }

        [Test]
        public void Should_Flatten_A_Real_OptionId_Condition_Onto_The_Dto()
        {
            var viewModel = new EventQuestionViewModel
            {
                Id = 12,
                Title = "Anything we should know?",
                Order = 2,
                SelectType = EventQuestionSelectType.Multi,
                IsRequired = false,
                ShowIf = new EventQuestionConditionViewModel { OptionId = 41 },
                Options = new List<EventQuestionOptionViewModel>()
            };

            var dto = _mapper.Map<EventQuestionViewModel, EventQuestionStructureDto>(viewModel);

            Assert.That(dto.ShowIfOptionId, Is.EqualTo(41));
            Assert.That(dto.ShowIfOptionClientId, Is.Null);
        }

        [Test]
        public void Should_Map_A_Null_Condition_To_An_Always_Shown_Question()
        {
            var viewModel = new EventQuestionViewModel
            {
                ClientId = "q1",
                Title = "Pick your dish",
                Order = 0,
                ShowIf = null,
                Options = new List<EventQuestionOptionViewModel>()
            };

            var dto = _mapper.Map<EventQuestionViewModel, EventQuestionStructureDto>(viewModel);

            Assert.That(dto.ShowIfOptionId, Is.Null);
            Assert.That(dto.ShowIfOptionClientId, Is.Null);
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test src/api/Shrooms.Premium.Tests/Shrooms.Premium.Tests.csproj --filter "FullyQualifiedName~EventQuestionMappingTests" --nologo`

Expected: FAIL — compile error, `EventQuestionViewModel` does not exist.

- [ ] **Step 3: Create the view models**

Create `src/api/Shrooms.Premium/Presentation/WebViewModels/Events/EventQuestionViewModel.cs`:

```csharp
using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventQuestionViewModel
    {
        public int? Id { get; set; }

        /// <summary>Client-generated; required when <see cref="Id"/> is null.</summary>
        public string ClientId { get; set; }

        public string Title { get; set; }

        public int Order { get; set; }

        public EventQuestionSelectType SelectType { get; set; }

        public bool IsRequired { get; set; }

        /// <summary>Null means the question is always shown.</summary>
        public EventQuestionConditionViewModel ShowIf { get; set; }

        public IList<EventQuestionOptionViewModel> Options { get; set; } = new List<EventQuestionOptionViewModel>();
    }

    public class EventQuestionConditionViewModel
    {
        /// <summary>Set when the trigger option already exists. Mutually exclusive with <see cref="OptionClientId"/>.</summary>
        public int? OptionId { get; set; }

        /// <summary>Set when the trigger option is inserted in this same request.</summary>
        public string OptionClientId { get; set; }
    }
}
```

Create `src/api/Shrooms.Premium/Presentation/WebViewModels/Events/EventQuestionOptionViewModel.cs`:

```csharp
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventQuestionOptionViewModel
    {
        public int? Id { get; set; }

        public string ClientId { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        public OptionRules Rule { get; set; }
    }
}
```

- [ ] **Step 4: Add `Questions` to the write view models and DTOs**

Add to `CreateEventViewModel` and `UpdateEventViewModel` (both in
`src/api/Shrooms.Premium/Presentation/WebViewModels/Events/`):

```csharp
        public IList<EventQuestionViewModel> Questions { get; set; } = new List<EventQuestionViewModel>();
```

Add to `CreateEventDto` and `EditEventDto` (both in
`src/api/Shrooms.Premium/DataTransferObjects/Models/Events/`):

```csharp
        public IList<EventQuestionStructureDto> Questions { get; set; } = new List<EventQuestionStructureDto>();
```

- [ ] **Step 5: Extend the read models**

Add to `src/api/Shrooms.Premium/DataTransferObjects/Models/Events/EventOptionsDto.cs`:

```csharp
        public IEnumerable<EventQuestionStructureDto> Questions { get; set; } = new List<EventQuestionStructureDto>();

        /// <summary>The calling user's current answers, so the wizard can prefill on reopen.</summary>
        public IEnumerable<int> MyChosenOptions { get; set; } = new List<int>();
```

Add to `src/api/Shrooms.Premium/Presentation/WebViewModels/Events/EventOptionsViewModel.cs`:

```csharp
        public IEnumerable<EventQuestionViewModel> Questions { get; set; } = new List<EventQuestionViewModel>();

        public IEnumerable<int> MyChosenOptions { get; set; } = new List<int>();
```

- [ ] **Step 6: Add the mappings**

In `src/api/Shrooms.Premium/Presentation/ModelMappings/Profiles/EventsProfile.cs`, inside the
constructor, add:

```csharp
            CreateMap<EventQuestionOptionViewModel, EventQuestionOptionStructureDto>().ReverseMap();

            CreateMap<EventQuestionViewModel, EventQuestionStructureDto>()
                .ForMember(dest => dest.ShowIfOptionId,
                    opt => opt.MapFrom(src => src.ShowIf == null ? null : src.ShowIf.OptionId))
                .ForMember(dest => dest.ShowIfOptionClientId,
                    opt => opt.MapFrom(src => src.ShowIf == null ? null : src.ShowIf.OptionClientId));

            CreateMap<EventQuestionStructureDto, EventQuestionViewModel>()
                .ForMember(dest => dest.ShowIf, opt => opt.MapFrom(src =>
                    src.ShowIfOptionId == null && src.ShowIfOptionClientId == null
                        ? null
                        : new EventQuestionConditionViewModel
                        {
                            OptionId = src.ShowIfOptionId,
                            OptionClientId = src.ShowIfOptionClientId
                        }));
```

- [ ] **Step 7: Run tests to verify they pass**

Run: `dotnet test src/api/Shrooms.Premium.Tests/Shrooms.Premium.Tests.csproj --filter "FullyQualifiedName~EventQuestionMappingTests" --nologo`

Expected: PASS, 3 tests.

- [ ] **Step 8: Run the whole test project to check nothing regressed**

Run: `dotnet test src/api/Shrooms.Premium.Tests/Shrooms.Premium.Tests.csproj --nologo`

Expected: 0 failures. The baseline measured on 2026-08-17, before any task in this plan, was
**488 passed / 0 failed**, so the count should only grow.

- [ ] **Step 9: Commit**

```bash
git add src/api/Shrooms.Premium/Presentation/WebViewModels/Events/ \
        src/api/Shrooms.Premium/DataTransferObjects/Models/Events/ \
        src/api/Shrooms.Premium/Presentation/ModelMappings/Profiles/EventsProfile.cs \
        src/api/Shrooms.Premium.Tests/Controllers/ViewModels/EventQuestionMappingTests.cs
git commit -m "feat(events): wire sign-up question view models and mappings"
```

---

### Task 5: Write path — persist the question tree on create and update

**Files:**
- Create: `src/api/Shrooms.Premium/Domain/Services/Events/EventQuestionWriter.cs`
- Create: `src/api/Shrooms.Premium/Domain/Services/Events/IEventQuestionWriter.cs`
- Modify: `src/api/Shrooms.Premium/Domain/Services/Events/EventService.cs` (create path near line 181 `MapNewOptions`, update path near line 231 `UpdateEventOptions`)
- Modify: `src/api/Shrooms.IoC/Modules/PremiumModule.cs` (register the writer and both validators)
- Test: `src/api/Shrooms.Premium.Tests/DomainService/EventServices/EventQuestionWriterTests.cs`

**Interfaces:**
- Consumes: `IEventQuestionStructureValidator` (Task 2), `EventQuestionStructureDto` (Task 2), `EventQuestion` (Task 1).
- Produces: `IEventQuestionWriter.WriteAsync(Guid eventId, IList<EventQuestionStructureDto> questions, string userId)` — inserts, updates, soft-deletes and resolves `clientId` references, then calls `ValidateResolved`.

The writer is its own class rather than more private methods on `EventService`, which is already
699 lines. It owns one job: turn a desired-state payload into rows.

**Spec invariant 2 holds by construction.** "A question's options must all belong to that
question's event" is satisfied because the writer stamps `EventId = eventId` on every inserted
option and only ever resolves existing options from questions already scoped to this event. A
payload naming an option ID from a *different* event therefore fails the `.Single(...)` lookup with
`InvalidOperationException` rather than a clean `400` — acceptable for Phase 1 given the frontend
cannot produce such a payload, but worth converting to an `EventException` if it ever surfaces.

**Orphaned participant answers are deliberately left in place.** Soft-deleting an option does not
remove the `EventParticipant`↔`EventOption` join rows pointing at it. Reads self-heal — the global
`!IsDeleted` query filter means a deleted option never comes back from `MyChosenOptions` or any
other projection — so this is invisible to both frontends. Actively deleting those rows and
reporting how many participants were affected is the spec's §6 behaviour and belongs to Phase 2.

- [ ] **Step 1: Write the failing tests**

Create `src/api/Shrooms.Premium.Tests/DomainService/EventServices/EventQuestionWriterTests.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;
using Shrooms.Premium.Domain.Services.Events;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    public class EventQuestionWriterTests
    {
        private IEventQuestionWriter _writer;
        private DbSet<EventQuestion> _questionsDbSet;
        private DbSet<EventOption> _optionsDbSet;

        private readonly Guid _eventId = Guid.NewGuid();

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _questionsDbSet = uow.MockDbSetForAsync<EventQuestion>(new List<EventQuestion>());
            _optionsDbSet = uow.MockDbSetForAsync<EventOption>(new List<EventOption>());

            _writer = new EventQuestionWriter(uow, new EventQuestionStructureValidator());
        }

        [Test]
        public async Task Should_Insert_A_Question_With_Its_Options()
        {
            var questions = new List<EventQuestionStructureDto>
            {
                new EventQuestionStructureDto
                {
                    ClientId = "q1",
                    Title = "Pick your dish",
                    Order = 0,
                    SelectType = EventQuestionSelectType.Single,
                    IsRequired = true,
                    Options = new List<EventQuestionOptionStructureDto>
                    {
                        new EventQuestionOptionStructureDto { ClientId = "o1", Name = "Pizza", Order = 0 },
                        new EventQuestionOptionStructureDto { ClientId = "o2", Name = "Pasta", Order = 1 }
                    }
                }
            };

            await _writer.WriteAsync(_eventId, questions, "user-1");

            _questionsDbSet.Received(1).Add(Arg.Is<EventQuestion>(q => q.Title == "Pick your dish" && q.EventId == _eventId));
            _optionsDbSet.Received(2).Add(Arg.Any<EventOption>());
        }

        [Test]
        public void Should_Reject_A_Condition_Whose_Trigger_Is_Absent_From_The_Payload()
        {
            var questions = new List<EventQuestionStructureDto>
            {
                new EventQuestionStructureDto
                {
                    ClientId = "q1",
                    Title = "Which pizza?",
                    Order = 0,
                    ShowIfOptionClientId = "removed-in-this-request",
                    Options = new List<EventQuestionOptionStructureDto>
                    {
                        new EventQuestionOptionStructureDto { ClientId = "o1", Name = "Margherita", Order = 0 }
                    }
                }
            };

            Assert.ThrowsAsync<EventException>(async () => await _writer.WriteAsync(_eventId, questions, "user-1"));
        }

        [Test]
        public async Task Should_Soft_Delete_Questions_Absent_From_The_Payload()
        {
            var existing = new EventQuestion
            {
                Id = 7,
                EventId = _eventId,
                Title = "Old question",
                Order = 0,
                IsDeleted = false,
                Options = new List<EventOption>()
            };

            var uow = Substitute.For<IUnitOfWork2>();
            _questionsDbSet = uow.MockDbSetForAsync(new List<EventQuestion> { existing });
            _optionsDbSet = uow.MockDbSetForAsync<EventOption>(new List<EventOption>());
            _writer = new EventQuestionWriter(uow, new EventQuestionStructureValidator());

            await _writer.WriteAsync(_eventId, new List<EventQuestionStructureDto>(), "user-1");

            Assert.That(existing.IsDeleted, Is.True);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test src/api/Shrooms.Premium.Tests/Shrooms.Premium.Tests.csproj --filter "FullyQualifiedName~EventQuestionWriterTests" --nologo`

Expected: FAIL — compile error, `EventQuestionWriter` does not exist.

- [ ] **Step 3: Write the interface**

Create `src/api/Shrooms.Premium/Domain/Services/Events/IEventQuestionWriter.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.Services.Events
{
    public interface IEventQuestionWriter
    {
        /// <summary>
        /// Applies the full desired state of an event's question tree. Rows present in the
        /// database but absent from <paramref name="questions"/> are soft-deleted.
        /// </summary>
        Task WriteAsync(Guid eventId, IList<EventQuestionStructureDto> questions, string userId);
    }
}
```

- [ ] **Step 4: Write the implementation**

Create `src/api/Shrooms.Premium/Domain/Services/Events/EventQuestionWriter.cs`:

```csharp
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

            SoftDeleteAbsent(existing, desired, userId);

            // Pass 1: questions, without conditions — the trigger options may not exist yet.
            var questionByClientId = new Dictionary<string, EventQuestion>();
            var entities = new List<(EventQuestionStructureDto Dto, EventQuestion Entity)>();

            foreach (var dto in desired)
            {
                var entity = dto.Id == null
                    ? InsertQuestion(eventId, dto)
                    : UpdateQuestion(existing, dto);

                if (dto.ClientId != null)
                {
                    questionByClientId[dto.ClientId] = entity;
                }

                entities.Add((dto, entity));
            }

            // Pass 2: options. Saving here assigns the identity values that pass 3 needs.
            var optionByClientId = new Dictionary<string, EventOption>();

            foreach (var (dto, entity) in entities)
            {
                WriteOptions(eventId, dto, entity, existing, optionByClientId, userId);
            }

            await _uow.SaveChangesAsync(userId);

            // Pass 3: resolve conditions now that every option has a real ID.
            foreach (var (dto, entity) in entities)
            {
                entity.ShowIfOptionId = ResolveCondition(dto, optionByClientId);
            }

            _structureValidator.ValidateResolved(BuildResolved(entities));

            await _uow.SaveChangesAsync(userId);
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
                }
            }
        }

        private static int? ResolveCondition(
            EventQuestionStructureDto dto,
            IReadOnlyDictionary<string, EventOption> optionByClientId)
        {
            if (dto.ShowIfOptionId != null)
            {
                return dto.ShowIfOptionId;
            }

            if (dto.ShowIfOptionClientId == null)
            {
                return null;
            }

            return optionByClientId[dto.ShowIfOptionClientId].Id;
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

        private static IReadOnlyList<ResolvedEventQuestionDto> BuildResolved(
            List<(EventQuestionStructureDto Dto, EventQuestion Entity)> entities)
        {
            return entities.Select(pair => new ResolvedEventQuestionDto
            {
                QuestionId = pair.Entity.Id,
                Order = pair.Entity.Order,
                SelectType = pair.Entity.SelectType,
                IsRequired = pair.Entity.IsRequired,
                ShowIfOptionId = pair.Entity.ShowIfOptionId,
                OptionIds = (pair.Entity.Options ?? new List<EventOption>()).Select(o => o.Id).ToList()
            }).ToList();
        }
    }
}
```

- [ ] **Step 5: Call the writer from EventService**

In `src/api/Shrooms.Premium/Domain/Services/Events/EventService.cs`:

Add the field and constructor parameter:

```csharp
        private readonly IEventQuestionWriter _eventQuestionWriter;
```

In the create path, after `MapNewOptions(newEventDto, newEvent);` and after the event has been
saved (so `newEvent.Id` exists), add:

```csharp
            await _eventQuestionWriter.WriteAsync(newEvent.Id, newEventDto.Questions, newEventDto.UserId);
```

In the update path, after `UpdateEventOptions(eventDto, eventToUpdate);` add:

```csharp
            await _eventQuestionWriter.WriteAsync(eventToUpdate.Id, eventDto.Questions, eventDto.UserId);
```

- [ ] **Step 6: Register in the IoC container**

Registration uses `Microsoft.Extensions.DependencyInjection`, not Autofac. In
`src/api/Shrooms.Premium/IoC/Modules/EventsModule.cs`, inside `AddPremiumEvents`, add before
`return services;`:

```csharp
            services.AddScoped<IEventQuestionWriter, EventQuestionWriter>();
            services.AddScoped<IEventQuestionStructureValidator, EventQuestionStructureValidator>();
            services.AddScoped<IEventAnswerValidator, EventAnswerValidator>();
```

`Shrooms.Premium.Domain.DomainServiceValidators.Events` and
`Shrooms.Premium.Domain.Services.Events` are already imported in that file, so no new `using` is
needed.

- [ ] **Step 7: Run tests to verify they pass**

Run: `dotnet test src/api/Shrooms.Premium.Tests/Shrooms.Premium.Tests.csproj --filter "FullyQualifiedName~EventQuestionWriterTests" --nologo`

Expected: PASS, 3 tests.

- [ ] **Step 8: Commit**

```bash
git add src/api/Shrooms.Premium/Domain/Services/Events/EventQuestionWriter.cs \
        src/api/Shrooms.Premium/Domain/Services/Events/IEventQuestionWriter.cs \
        src/api/Shrooms.Premium/Domain/Services/Events/EventService.cs \
        src/api/Shrooms.IoC/Modules/PremiumModule.cs \
        src/api/Shrooms.Premium.Tests/DomainService/EventServices/EventQuestionWriterTests.cs
git commit -m "feat(events): persist the sign-up question tree on create and update"
```

---

### Task 6: Read path — Options and Update hydration

**Files:**
- Modify: `src/api/Shrooms.Premium/Domain/Services/Events/List/EventListingService.cs` (`GetEventOptionsAsync` line 59, `MapOptionsToDto` line 405)
- Modify: `src/api/Shrooms.Premium/Domain/Services/Events/EventService.cs` (the `GetEventForUpdate` projection near line 339)
- Test: `src/api/Shrooms.Premium.Tests/DomainService/EventServices/EventQuestionReadTests.cs`

**Interfaces:**
- Consumes: `EventOptionsDto.Questions` / `.MyChosenOptions` (Task 4), `EventQuestion` (Task 1).
- Produces: `GET /Events/Options` returns `questions[]` and `myChosenOptions[]`; `GET /Events/Update` returns `questions[]`.

`MapOptionsToDto()` is a static `Expression<Func<Event, EventOptionsDto>>` used inside a
`.Select()`, so it must stay translatable to SQL. `MyChosenOptions` depends on the calling user,
which the expression does not receive — so `GetEventOptionsAsync` takes the user ID as a closure
parameter and the expression becomes a method returning the expression.

- [ ] **Step 1: Write the failing test**

Create `src/api/Shrooms.Premium.Tests/DomainService/EventServices/EventQuestionReadTests.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;
using Shrooms.Premium.Domain.Services.Events.List;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    public class EventQuestionReadTests
    {
        private IEventListingService _eventListingService;
        private DbSet<Event> _eventsDbSet;

        private readonly Guid _eventId = Guid.NewGuid();
        private readonly int _organizationId = 2;
        private const string UserId = "user-1";

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            var pizza = new EventOption { Id = 10, EventId = _eventId, Option = "Pizza", Order = 0, QuestionId = 1 };
            var pasta = new EventOption { Id = 11, EventId = _eventId, Option = "Pasta", Order = 1, QuestionId = 1 };

            var question = new EventQuestion
            {
                Id = 1,
                EventId = _eventId,
                Title = "Pick your dish",
                Order = 0,
                SelectType = EventQuestionSelectType.Single,
                IsRequired = true,
                ShowIfOptionId = null,
                Options = new List<EventOption> { pizza, pasta }
            };

            var @event = new Event
            {
                Id = _eventId,
                OrganizationId = _organizationId,
                MaxChoices = 1,
                EventOptions = new List<EventOption> { pizza, pasta },
                EventParticipants = new List<EventParticipant>
                {
                    new EventParticipant
                    {
                        EventId = _eventId,
                        ApplicationUserId = UserId,
                        EventOptions = new List<EventOption> { pizza }
                    }
                }
            };

            _eventsDbSet = uow.MockDbSetForAsync(new List<Event> { @event });
            uow.MockDbSetForAsync(new List<EventQuestion> { question });

            _eventListingService = new EventListingService(uow, Substitute.For<IEventValidationService>());
        }

        [Test]
        public async Task Should_Return_The_Question_Tree_On_Options()
        {
            var result = await _eventListingService.GetEventOptionsAsync(
                _eventId,
                new UserAndOrganizationDto { UserId = UserId, OrganizationId = _organizationId });

            Assert.That(result.Questions.Count(), Is.EqualTo(1));
            Assert.That(result.Questions.Single().Title, Is.EqualTo("Pick your dish"));
            Assert.That(result.Questions.Single().Options.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task Should_Return_The_Calling_Users_Own_Answers_For_Prefill()
        {
            var result = await _eventListingService.GetEventOptionsAsync(
                _eventId,
                new UserAndOrganizationDto { UserId = UserId, OrganizationId = _organizationId });

            Assert.That(result.MyChosenOptions, Is.EquivalentTo(new[] { 10 }));
        }

        [Test]
        public async Task Should_Return_No_Answers_For_A_User_Who_Has_Not_Joined()
        {
            var result = await _eventListingService.GetEventOptionsAsync(
                _eventId,
                new UserAndOrganizationDto { UserId = "someone-else", OrganizationId = _organizationId });

            Assert.That(result.MyChosenOptions, Is.Empty);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test src/api/Shrooms.Premium.Tests/Shrooms.Premium.Tests.csproj --filter "FullyQualifiedName~EventQuestionReadTests" --nologo`

Expected: FAIL — `result.Questions` is empty and `MyChosenOptions` is empty.

- [ ] **Step 3: Extend `GetEventOptionsAsync`**

In `src/api/Shrooms.Premium/Domain/Services/Events/List/EventListingService.cs`, add a
`DbSet<EventQuestion>` field initialised in the constructor (`IUnitOfWork2.GetDbSet<T>()` returns
`DbSet<T>`, not `IDbSet<T>`):

```csharp
        private readonly DbSet<EventQuestion> _eventQuestionsDbSet;
```

```csharp
            _eventQuestionsDbSet = uow.GetDbSet<EventQuestion>();
```

Replace the body of `GetEventOptionsAsync` with:

```csharp
        public async Task<EventOptionsDto> GetEventOptionsAsync(Guid eventId, UserAndOrganizationDto userOrg)
        {
            var eventOptionsDto = await _eventsDbSet
                .Include(e => e.EventOptions)
                .Where(e => e.Id == eventId && e.OrganizationId == userOrg.OrganizationId)
                .Select(MapOptionsToDto())
                .SingleOrDefaultAsync();

            _eventValidationService.CheckIfEventExists(eventOptionsDto);

            eventOptionsDto.Questions = await _eventQuestionsDbSet
                .Include(q => q.Options)
                .Where(q => q.EventId == eventId)
                .OrderBy(q => q.Order)
                .Select(q => new EventQuestionStructureDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    Order = q.Order,
                    SelectType = q.SelectType,
                    IsRequired = q.IsRequired,
                    ShowIfOptionId = q.ShowIfOptionId,
                    Options = q.Options
                        .OrderBy(o => o.Order)
                        .Select(o => new EventQuestionOptionStructureDto
                        {
                            Id = o.Id,
                            Name = o.Option,
                            Order = o.Order,
                            Rule = o.Rule
                        })
                        .ToList()
                })
                .ToListAsync();

            eventOptionsDto.MyChosenOptions = await _eventsDbSet
                .Where(e => e.Id == eventId)
                .SelectMany(e => e.EventParticipants)
                .Where(p => p.ApplicationUserId == userOrg.UserId)
                .SelectMany(p => p.EventOptions)
                .Select(o => o.Id)
                .ToListAsync();

            return eventOptionsDto;
        }
```

- [ ] **Step 4: Hydrate the builder on `GET /Events/Update`**

In `src/api/Shrooms.Premium/Domain/Services/Events/EventService.cs`, in the method that builds the
update view (the projection containing `Options = e.EventOptions.Select(...)` near line 339), add
a sibling assignment:

```csharp
                Questions = e.EventQuestions
                    .OrderBy(q => q.Order)
                    .Select(q => new EventQuestionStructureDto
                    {
                        Id = q.Id,
                        Title = q.Title,
                        Order = q.Order,
                        SelectType = q.SelectType,
                        IsRequired = q.IsRequired,
                        ShowIfOptionId = q.ShowIfOptionId,
                        Options = q.Options
                            .OrderBy(o => o.Order)
                            .Select(o => new EventQuestionOptionStructureDto
                            {
                                Id = o.Id,
                                Name = o.Option,
                                Order = o.Order,
                                Rule = o.Rule
                            })
                            .ToList()
                    })
                    .ToList(),
```

This requires an `EventQuestions` navigation on `Event`. Add to
`src/api/Shrooms.DataLayer.EntityModels/Models/Events/Event.cs`:

```csharp
        public virtual ICollection<EventQuestion> EventQuestions { get; set; }
```

and add the inverse to `EventQuestionEntityConfig.Configure`, replacing the `HasOne(e => e.Event)`
block written in Task 1:

```csharp
            builder.HasOne(e => e.Event)
                .WithMany(e => e.EventQuestions)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);
```

Since this changes the model, regenerate the migration check:

```bash
dotnet ef migrations has-pending-model-changes --project src/api/Shrooms.DataLayer --context ShroomsDbContext
```

If it reports pending changes, add a follow-up migration:

```bash
dotnet ef migrations add AddEventQuestionsNavigation --project src/api/Shrooms.DataLayer --context ShroomsDbContext --output-dir EFCoreMigrations
dotnet ef database update --project src/api/Shrooms.DataLayer --context ShroomsDbContext
```

A pure navigation property normally produces no schema change, so an empty migration here is the
expected outcome and can be deleted rather than applied.

- [ ] **Step 5: Run tests to verify they pass**

Run: `dotnet test src/api/Shrooms.Premium.Tests/Shrooms.Premium.Tests.csproj --filter "FullyQualifiedName~EventQuestionReadTests" --nologo`

Expected: PASS, 3 tests.

- [ ] **Step 6: Commit**

```bash
git add src/api/Shrooms.Premium/Domain/Services/Events/List/EventListingService.cs \
        src/api/Shrooms.Premium/Domain/Services/Events/EventService.cs \
        src/api/Shrooms.DataLayer.EntityModels/Models/Events/Event.cs \
        src/api/Shrooms.DataLayer/DAL/EntityTypeConfigurations/EventQuestionEntityConfig.cs \
        src/api/Shrooms.Premium.Tests/DomainService/EventServices/EventQuestionReadTests.cs
git commit -m "feat(events): return the sign-up question tree and the user's own answers"
```

---

### Task 7: Enforce answers on join and option change

**Files:**
- Modify: `src/api/Shrooms.Premium/Domain/Services/Events/Participation/EventParticipationService.cs` (`ValidateEventBeforeJoin` line 336, `UpdateSelectedOptionsAsync` line 298)
- Modify: `src/api/Shrooms.Premium/Presentation/Api/Controllers/EventController.cs` (`Join` line 347, `UpdateSelectedOptions` line 640)
- Test: `src/api/Shrooms.Premium.Tests/DomainService/EventServices/EventJoinAnswerValidationTests.cs`

**Interfaces:**
- Consumes: `IEventAnswerValidator` (Task 3), `EventAnswersInvalidException` (Task 3), `ResolvedEventQuestionDto` (Task 2).
- Produces: `POST /Events/Join` and `POST /Events/Options` return `400` with `{ code, errors[] }` when answers do not satisfy the tree. `POST /Events/AddColleague` inherits this because `AddColleagueAsync` delegates to `JoinAsync`.

- [ ] **Step 1a: Extend the existing participation fixture**

`EventParticipantServiceTests` already drives `JoinAsync` successfully in ~10 tests, so extend it
rather than duplicating a 70-line `[SetUp]`.

In `src/api/Shrooms.Premium.Tests/DomainService/EventServices/EventParticipantServiceTests.cs`,
add a field next to the other DbSet fields:

```csharp
        private DbSet<EventQuestion> _eventQuestionsDbSet;
```

In `TestInitializer`, after the `_eventOptionsDbSet` block (currently lines 60–61), add:

```csharp
            _eventQuestionsDbSet = Substitute.For<DbSet<EventQuestion>, IQueryable<EventQuestion>, IAsyncEnumerable<EventQuestion>>();
            _uow2.GetDbSet<EventQuestion>().Returns(_eventQuestionsDbSet);
```

Then add the new constructor argument at the `new EventParticipationService(` call (currently
line 74) — a real validator, not a substitute, because the assertions are about its behaviour:

```csharp
            _eventParticipationService =
                new EventParticipationService(
                    _uow2,
                    _systemClockMock,
                    roleService,
                    permissionService,
                    _eventValidationServiceMock,
                    _wallService,
                    _asyncRunner,
                    new EventAnswerValidator());
```

Add these `using` directives to the file if absent:

```csharp
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;
```

- [ ] **Step 1b: Write the failing service tests**

Append to the same `EventParticipantServiceTests` class. The helper seeds the food tree from the
spec: q1 "Pick your dish" (required, single, options 10 Pizza / 11 Pasta) and q2 "Which pizza?"
(required, single, shown if 10, options 20 / 21).

```csharp
        private Guid MockEventWithQuestions()
        {
            var eventGuid = Guid.NewGuid();

            var pizza = new EventOption { Id = 10, EventId = eventGuid, Option = "Pizza", Order = 0, QuestionId = 1 };
            var pasta = new EventOption { Id = 11, EventId = eventGuid, Option = "Pasta", Order = 1, QuestionId = 1 };
            var margherita = new EventOption { Id = 20, EventId = eventGuid, Option = "Margherita", Order = 0, QuestionId = 2 };
            var pepperoni = new EventOption { Id = 21, EventId = eventGuid, Option = "Pepperoni", Order = 1, QuestionId = 2 };

            var questions = new List<EventQuestion>
            {
                new EventQuestion
                {
                    Id = 1, EventId = eventGuid, Title = "Pick your dish", Order = 0,
                    SelectType = EventQuestionSelectType.Single, IsRequired = true, ShowIfOptionId = null,
                    Options = new List<EventOption> { pizza, pasta }
                },
                new EventQuestion
                {
                    Id = 2, EventId = eventGuid, Title = "Which pizza?", Order = 1,
                    SelectType = EventQuestionSelectType.Single, IsRequired = true, ShowIfOptionId = 10,
                    Options = new List<EventOption> { margherita, pepperoni }
                }
            };

            _eventQuestionsDbSet.SetDbSetDataForAsync(questions);
            _eventOptionsDbSet.SetDbSetDataForAsync(new List<EventOption> { pizza, pasta, margherita, pepperoni });

            var @event = new Event
            {
                Id = eventGuid,
                OrganizationId = 2,
                MaxChoices = 1,
                MaxParticipants = 20,
                StartDate = DateTime.Parse("2016-04-05"),
                EndDate = DateTime.Parse("2016-04-06"),
                RegistrationDeadline = DateTime.Parse("2016-04-04"),
                EventOptions = new List<EventOption> { pizza, pasta, margherita, pepperoni },
                EventParticipants = new List<EventParticipant>()
            };

            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event });

            return eventGuid;
        }

        [Test]
        public void Should_Reject_A_Join_Missing_A_Required_Answer()
        {
            var eventGuid = MockEventWithQuestions();
            MockUsers();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            var joinDto = new EventJoinDto
            {
                EventId = eventGuid,
                ChosenOptions = new List<int>(),
                ParticipantIds = new List<string> { "testUserId" },
                UserId = "testUserId",
                OrganizationId = 2
            };

            var ex = Assert.ThrowsAsync<EventAnswersInvalidException>(
                async () => await _eventParticipationService.JoinAsync(joinDto));

            Assert.That(ex.Errors.Any(e => e.QuestionId == 1 && e.Reason == EventAnswerErrorReason.RequiredAnswerMissing), Is.True);
        }

        [Test]
        public void Should_Reject_A_Join_Answering_A_Hidden_Question()
        {
            var eventGuid = MockEventWithQuestions();
            MockUsers();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            var joinDto = new EventJoinDto
            {
                EventId = eventGuid,
                ChosenOptions = new List<int> { 11, 20 },   // Pasta chosen, yet a pizza sub-option answered
                ParticipantIds = new List<string> { "testUserId" },
                UserId = "testUserId",
                OrganizationId = 2
            };

            var ex = Assert.ThrowsAsync<EventAnswersInvalidException>(
                async () => await _eventParticipationService.JoinAsync(joinDto));

            Assert.That(ex.Errors.Any(e => e.QuestionId == 2 && e.Reason == EventAnswerErrorReason.AnswerForHiddenQuestion), Is.True);
        }

        [Test]
        public void Should_Accept_A_Join_With_A_Complete_Branch()
        {
            var eventGuid = MockEventWithQuestions();
            MockUsers();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            var joinDto = new EventJoinDto
            {
                EventId = eventGuid,
                ChosenOptions = new List<int> { 10, 20 },
                ParticipantIds = new List<string> { "testUserId" },
                UserId = "testUserId",
                OrganizationId = 2
            };

            Assert.DoesNotThrowAsync(async () => await _eventParticipationService.JoinAsync(joinDto));
        }
```

The existing `Should_Successfully_Join_Event_Without_Options` test in this same file is the
legacy-flat-option regression check — it must keep passing, which proves events with no questions
are unaffected. Confirm the mock helper name for seeding a DbSet matches what the fixture already
uses (`SetDbSetDataForAsync` in `Shrooms.Tests.Extensions`); if the existing tests use a different
helper, use theirs.

- [ ] **Step 1c: Write the failing controller test**

Catch-block ordering is the subtlest bug in this task: `EventAnswersInvalidException` derives from
`EventException`, so if the generic catch is listed first it silently swallows the structured one
and the frontend receives a bare string instead of routable errors. The test must therefore
exercise the **controller action** and assert the response body — asserting the exception's
inheritance would pass regardless of catch order and prove nothing.

Add to the existing `src/api/Shrooms.Premium.Tests/Controllers/WebApi/EventControllerTests.cs`,
reusing that file's existing `[SetUp]` and substituted services:

```csharp
        [Test]
        public async Task Join_Should_Return_The_Structured_Body_When_Answers_Are_Invalid()
        {
            _eventParticipationService
                .JoinAsync(Arg.Any<EventJoinDto>())
                .Returns<Task>(_ => throw new EventAnswersInvalidException(new List<EventAnswerErrorDto>
                {
                    new EventAnswerErrorDto { QuestionId = 12, Reason = EventAnswerErrorReason.RequiredAnswerMissing },
                    new EventAnswerErrorDto { QuestionId = 14, Reason = EventAnswerErrorReason.AnswerForHiddenQuestion }
                }));

            var result = await _eventController.Join(new EventJoinViewModel
            {
                EventId = Guid.NewGuid(),
                ChosenOptions = new List<int>()
            });

            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest, Is.Not.Null, "invalid answers must produce a 400");

            // Serialize rather than reflect over the anonymous type — this asserts the shape the
            // frontend actually receives.
            var json = System.Text.Json.JsonSerializer.Serialize(badRequest.Value);

            Assert.That(json, Does.Contain("EventAnswersInvalid"));
            Assert.That(json, Does.Contain("RequiredAnswerMissing"));
            Assert.That(json, Does.Contain("AnswerForHiddenQuestion"));
            Assert.That(json, Does.Contain("12"));
            Assert.That(json, Does.Contain("14"));
        }
```

If `EventControllerTests` does not already expose `_eventParticipationService` and
`_eventController` as fields, add them following the pattern the file already uses for its other
substituted services. Add these `using` directives if absent:

```csharp
using Microsoft.AspNetCore.Mvc;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;
```

**This test must fail before the controller change and pass after.** If it passes before Step 4,
the assertion is not reaching the catch block — fix the test, do not proceed.

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test src/api/Shrooms.Premium.Tests/Shrooms.Premium.Tests.csproj --filter "FullyQualifiedName~EventParticipantServiceTests|FullyQualifiedName~EventControllerTests" --nologo`

Expected: FAIL — the three new join tests throw nothing, because answers are not yet validated.

- [ ] **Step 3: Load the question tree and validate on join**

In `src/api/Shrooms.Premium/Domain/Services/Events/Participation/EventParticipationService.cs`,
add the field, constructor parameter and DbSet:

```csharp
        private readonly IEventAnswerValidator _eventAnswerValidator;
        private readonly DbSet<EventQuestion> _eventQuestionsDbSet;
        private readonly DbSet<EventOption> _eventOptionsDbSet;
```

initialised in the constructor body alongside the existing `GetDbSet` calls:

```csharp
            _eventQuestionsDbSet = _uow.GetDbSet<EventQuestion>();
            _eventOptionsDbSet = _uow.GetDbSet<EventOption>();
            _eventAnswerValidator = eventAnswerValidator;
```

Note `EventParticipationService` currently has **no** `_eventOptionsDbSet` — it holds only
`_eventsDbSet`, `_usersDbSet` and `_eventParticipantsDbSet`. Both new DbSets are additions, and
`IEventAnswerValidator eventAnswerValidator` is a new constructor parameter, so every existing
`new EventParticipationService(...)` call in the test project needs the extra argument.

Add a helper:

```csharp
        private async Task ValidateAnswersAsync(Guid eventId, ICollection<int> chosenOptions)
        {
            var questions = await _eventQuestionsDbSet
                .Include(q => q.Options)
                .Where(q => q.EventId == eventId)
                .OrderBy(q => q.Order)
                .Select(q => new ResolvedEventQuestionDto
                {
                    QuestionId = q.Id,
                    Order = q.Order,
                    SelectType = q.SelectType,
                    IsRequired = q.IsRequired,
                    ShowIfOptionId = q.ShowIfOptionId,
                    OptionIds = q.Options.Select(o => o.Id).ToList()
                })
                .ToListAsync();

            if (questions.Count == 0)
            {
                return;     // legacy flat-option event: MaxChoices rules already applied
            }

            var legacyOptionIds = await _eventOptionsDbSet
                .Where(o => o.EventId == eventId && o.QuestionId == null)
                .Select(o => o.Id)
                .ToListAsync();

            _eventAnswerValidator.Validate(questions, chosenOptions?.ToList() ?? new List<int>(), legacyOptionIds);
        }
```

Call it from `JoinAsync`, immediately after `ValidateEventBeforeJoin(joinDto, eventDto);`:

```csharp
                await ValidateAnswersAsync(joinDto.EventId, joinDto.ChosenOptions);
```

and from `UpdateSelectedOptionsAsync`, after the existing
`_eventValidationService.CheckIfUserParticipatesInEvent(...)` call:

```csharp
            await ValidateAnswersAsync(changeOptionsDto.EventId, changeOptionsDto.ChosenOptions);
```

`AddColleagueAsync` needs no change — it delegates to `JoinAsync`.

- [ ] **Step 4: Return the structured body from the controller**

In `src/api/Shrooms.Premium/Presentation/Api/Controllers/EventController.cs`, in **both** the
`Join` action (line 347) and the `UpdateSelectedOptions` action (line 640), add a catch **above**
the existing `catch (EventException e)`:

```csharp
            catch (EventAnswersInvalidException e)
            {
                return BadRequest(new
                {
                    code = EventAnswersInvalidException.ErrorCode,
                    errors = e.Errors.Select(error => new
                    {
                        questionId = error.QuestionId,
                        reason = error.Reason.ToString()
                    })
                });
            }
```

Order matters: `EventAnswersInvalidException` derives from `EventException`, so if the generic
catch comes first it swallows the structured one and the frontend gets a bare string.

- [ ] **Step 5: Register the validator in IoC**

Already added in Task 5, Step 6. Verify `EventAnswerValidator` is registered; if not, add:

```csharp
            builder.RegisterType<EventAnswerValidator>().As<IEventAnswerValidator>().InstancePerRequest();
```

- [ ] **Step 6: Run tests to verify they pass**

Run: `dotnet test src/api/Shrooms.Premium.Tests/Shrooms.Premium.Tests.csproj --filter "FullyQualifiedName~EventParticipantServiceTests|FullyQualifiedName~EventControllerTests" --nologo`

Expected: PASS — the three new join tests, the new controller test, and every pre-existing test in
`EventParticipantServiceTests` (including `Should_Successfully_Join_Event_Without_Options`, which
proves legacy flat-option events still join).

- [ ] **Step 7: Run the whole test project**

Run: `dotnet test src/api/Shrooms.Premium.Tests/Shrooms.Premium.Tests.csproj --nologo`

Expected: 0 failures, and a total of **488 + the tests added by this plan**.

- [ ] **Step 8: Verify end-to-end against the running stack**

With the Aspire AppHost running, open `http://localhost:50321/swagger` and:

1. `POST /Events/Create` with a `questions[]` payload using `clientId` references (the example in
   the spec's §2 is a valid body).
2. `GET /Events/Options?eventId=<id>` — confirm `questions[]` comes back with real IDs and
   `myChosenOptions` is `[]`.
3. `POST /Events/Join` with `chosenOptions: []` — confirm `400` with
   `{"code":"EventAnswersInvalid","errors":[{"questionId":<id>,"reason":"RequiredAnswerMissing"}]}`.
4. `POST /Events/Join` with a complete branch — confirm `200`.
5. `GET /Events/Options?eventId=<id>` again — confirm `myChosenOptions` now holds the chosen IDs.

- [ ] **Step 9: Commit**

```bash
git add src/api/Shrooms.Premium/Domain/Services/Events/Participation/EventParticipationService.cs \
        src/api/Shrooms.Premium/Presentation/Api/Controllers/EventController.cs \
        src/api/Shrooms.Premium.Tests/DomainService/EventServices/EventParticipantServiceTests.cs \
        src/api/Shrooms.Premium.Tests/Controllers/WebApi/EventControllerTests.cs
git commit -m "feat(events): enforce sign-up answers on join and option change"
```

---

## Phase 1 done

At this point the frontend can integrate end-to-end: build questions, save them, reopen the
builder, run the wizard, submit answers, and surface per-step errors.

Deferred to Phase 2 (see the spec): `GET /Events/Details` question grouping for the host responses
panel, `GET /Events/Export` columns, `UpdateAttendStatusViewModel.chosenOptions` and the transition
lifecycle, and the affected-participant count reported on destructive edits.

## Two things to report back to the frontend

1. **Recurrence work does not exist.** `Event.EventRecurring` is an enum column on a single row;
   there are no per-occurrence events to copy questions to. The spec's §7 Q4 should be struck.
2. **`questionId` is nullable in the error body.** It is `null` for `UnknownOption`, which has no
   owning question. Every other reason carries a real ID.
