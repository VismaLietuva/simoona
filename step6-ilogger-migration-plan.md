# Step 6 — Replace Custom `ILogger` with `ILogger<T>`

> **Rule:** This must land as a single atomic commit. Deleting `ILogger.cs` breaks the build
> until every consumer is updated. Do all changes, verify the build, then commit.

---

## Background

`Shrooms.Contracts.Infrastructure.ILogger` is a custom two-method wrapper around NLog and
Application Insights. NLog is completely dead (no `nlog.config`, `UseNLog()` never called).
Application Insights is live via `Microsoft.ApplicationInsights.AspNetCore`, but only if
`AddApplicationInsightsTelemetry()` is registered — which it currently is not in `Program.cs`.

The migration replaces the custom interface with the standard `Microsoft.Extensions.Logging.ILogger<T>`.

---

## Files to change — overview

| # | File | Action |
|---|---|---|
| 0 | `Shrooms.Presentation.Api/Program.cs` | **Pre-requisite:** wire Application Insights |
| 1 | `Shrooms.Infrastructure/Shrooms.Infrastructure.csproj` | Remove `NLog.Web.AspNetCore` package |
| 2 | `Shrooms.IoC/Modules/InfrastructureModule.cs` | Remove `ILogger` registration |
| 3 | `Shrooms.Infrastructure/Logger/Logger.cs` | **Delete** |
| 4 | `Shrooms.Contracts/Infrastructure/ILogger.cs` | **Delete** |
| 5 | `Shrooms.Infrastructure/FireAndForget/AsyncRunner.cs` | Migrate to `ILogger<AsyncRunner>` |
| 6 | `Shrooms.Premium/Infrastructure/VacationBot/VacationBotService.cs` | Migrate to `ILogger<VacationBotService>` |
| 7 | `Shrooms.Premium/Domain/Services/Books/BookCoverService.cs` | Migrate to `ILogger<BookCoverService>` |
| 8 | `Shrooms.Premium/Domain/Services/Books/BookRemindService.cs` | Migrate to `ILogger<BookRemindService>` |
| 9 | `Shrooms.Premium/Domain/Services/Lotteries/LotteryAbortJob.cs` | Migrate to `ILogger<LotteryAbortJob>` |
| 10 | `Shrooms.Premium/Domain/Services/WebHookCallbacks/LoyaltyKudos/LoyaltyKudosService.cs` | Migrate to `ILogger<LoyaltyKudosService>` |
| 11 | `Shrooms.Premium.Tests/DomainService/LotteryServices/LotteryAbortJobTests.cs` | Update mock |
| 12 | `Shrooms.Premium.Tests/DomainService/LoyaltyKudosTests.cs` | Update mock |
| 13 | `Shrooms.Premium.Tests/DomainService/VacationService/VacationBotServiceTests.cs` | Update mock |

---

## API mapping reference

Every call site uses one of these two methods — map them as follows:

| Custom `ILogger` call | `ILogger<T>` replacement |
|---|---|
| `_logger.Error(ex)` | `_logger.LogError(ex, ex.Message)` |
| `_logger.Debug(msg)` | `_logger.LogDebug(msg)` |
| `_logger.Debug(msg, ex)` | `_logger.LogDebug(ex, msg)` ← note argument order reversal |

---

## Change 0 — Pre-requisite: wire Application Insights in `Program.cs`

**File:** `src/api/Shrooms.Presentation.Api/Program.cs`

Add this line early in the builder section, alongside the other `builder.Services` calls:

```diff
  var builder = WebApplication.CreateBuilder(args);

  builder.Services.AddHealthChecks();
+ builder.Services.AddApplicationInsightsTelemetry();
```

> Without this, `ILogger<T>` calls will no longer send exceptions to Application Insights.
> The `Microsoft.ApplicationInsights.AspNetCore` package is already in
> `Shrooms.Infrastructure.csproj`, so no new NuGet reference is needed.

---

## Change 1 — Remove NLog package from `Shrooms.Infrastructure.csproj`

**File:** `src/api/Shrooms.Infrastructure/Shrooms.Infrastructure.csproj`

```diff
- <PackageReference Include="NLog.Web.AspNetCore" Version="5.3.15" />
```

---

## Change 2 — Remove `ILogger` registration from `InfrastructureModule.cs`

**File:** `src/api/Shrooms.IoC/Modules/InfrastructureModule.cs`

```diff
- using Shrooms.Infrastructure.Logger;
  ...
- services.AddScoped<ILogger, Logger>();
```

> `ILogger<T>` is registered automatically by the framework — no replacement line needed.

---

## Change 3 — Delete `Logger.cs`

**File:** `src/api/Shrooms.Infrastructure/Logger/Logger.cs`

Delete the file entirely.

---

## Change 4 — Delete `ILogger.cs`

**File:** `src/api/Shrooms.Contracts/Infrastructure/ILogger.cs`

Delete the file entirely.

---

## Change 5 — `AsyncRunner.cs`

**File:** `src/api/Shrooms.Infrastructure/FireAndForget/AsyncRunner.cs`

`AsyncRunner` resolves the logger from the DI scope at runtime (not via constructor injection),
so no constructor change is needed.

```diff
+ using Microsoft.Extensions.Logging;
  ...
- var logger = scope.ServiceProvider.GetService<ILogger>();
+ var logger = scope.ServiceProvider.GetService<ILogger<AsyncRunner>>();
  ...
- logger?.Error(ex);
+ logger?.LogError(ex, ex.Message);
```

> `ILogger<T>` is always available from the built-in DI container — no extra registration needed.

---

## Change 6 — `VacationBotService.cs`

**File:** `src/api/Shrooms.Premium/Infrastructure/VacationBot/VacationBotService.cs`

```diff
+ using Microsoft.Extensions.Logging;
  ...
- private readonly ILogger _logger;
+ private readonly ILogger<VacationBotService> _logger;

- public VacationBotService(HttpClient httpClient, IApplicationSettings appSettings, ILogger logger)
+ public VacationBotService(HttpClient httpClient, IApplicationSettings appSettings, ILogger<VacationBotService> logger)
```

Call sites (2 occurrences):

```diff
- _logger.Error(e);
+ _logger.LogError(e, e.Message);
```

```diff
- _logger.Error(new Exception(json));
+ _logger.LogError(json);
```

> The second call wraps a string in a new `Exception` only to log it — `LogError(string)` is
> the correct replacement; no exception object is needed.

---

## Change 7 — `BookCoverService.cs`

**File:** `src/api/Shrooms.Premium/Domain/Services/Books/BookCoverService.cs`

```diff
+ using Microsoft.Extensions.Logging;
  ...
- private readonly ILogger _logger;
+ private readonly ILogger<BookCoverService> _logger;

- public BookCoverService(IUnitOfWork2 uow, IBookInfoService bookService, ILogger logger)
+ public BookCoverService(IUnitOfWork2 uow, IBookInfoService bookService, ILogger<BookCoverService> logger)
  ...
- _logger.Error(ex);
+ _logger.LogError(ex, ex.Message);
```

---

## Change 8 — `BookRemindService.cs`

**File:** `src/api/Shrooms.Premium/Domain/Services/Books/BookRemindService.cs`

```diff
+ using Microsoft.Extensions.Logging;
  ...
- private readonly ILogger _logger;
+ private readonly ILogger<BookRemindService> _logger;

- public BookRemindService(..., ILogger logger)
+ public BookRemindService(..., ILogger<BookRemindService> logger)
  ...
- _logger.Debug(e.Message, e);
+ _logger.LogDebug(e, e.Message);
```

> **Watch the argument order.** The custom `Debug(string msg, Exception e)` takes message first.
> `ILogger<T>.LogDebug(Exception ex, string msg)` takes exception first.

---

## Change 9 — `LotteryAbortJob.cs`

**File:** `src/api/Shrooms.Premium/Domain/Services/Lotteries/LotteryAbortJob.cs`

```diff
+ using Microsoft.Extensions.Logging;
  ...
- private readonly ILogger _logger;
+ private readonly ILogger<LotteryAbortJob> _logger;

  public LotteryAbortJob(IKudosService kudosService,
      ILotteryParticipantService lotteryParticipantService,
-     ILogger logger,
+     ILogger<LotteryAbortJob> logger,
      IAsyncRunner asyncRunner,
      IUnitOfWork2 uow,
      ILotteryService lotteryService)
  ...
- _logger.Error(e);
+ _logger.LogError(e, e.Message);
```

---

## Change 10 — `LoyaltyKudosService.cs`

**File:** `src/api/Shrooms.Premium/Domain/Services/WebHookCallbacks/LoyaltyKudos/LoyaltyKudosService.cs`

```diff
+ using Microsoft.Extensions.Logging;
  ...
- private readonly ILogger _logger;
+ private readonly ILogger<LoyaltyKudosService> _logger;

- public LoyaltyKudosService(IUnitOfWork2 uow, ILogger logger, IAsyncRunner asyncRunner, ...)
+ public LoyaltyKudosService(IUnitOfWork2 uow, ILogger<LoyaltyKudosService> logger, IAsyncRunner asyncRunner, ...)
  ...
- _logger.Error(e);
+ _logger.LogError(e, e.Message);
```

---

## Change 11 — `LotteryAbortJobTests.cs`

**File:** `src/api/Shrooms.Premium.Tests/DomainService/LotteryServices/LotteryAbortJobTests.cs`

```diff
- using Shrooms.Contracts.Infrastructure;
+ using Microsoft.Extensions.Logging;
  ...
- var logger = Substitute.For<ILogger>();
+ var logger = Substitute.For<ILogger<LotteryAbortJob>>();

  _sut = new LotteryAbortJob(_kudosService, _lotteryParticipantService, logger, ...);
```

> `using Shrooms.Contracts.Infrastructure` may still be needed for other types in the file.
> Check before removing — if `IAsyncRunner` etc. are used, keep the using.

---

## Change 12 — `LoyaltyKudosTests.cs`

**File:** `src/api/Shrooms.Premium.Tests/DomainService/LoyaltyKudosTests.cs`

```diff
- using Shrooms.Contracts.Infrastructure;
+ using Microsoft.Extensions.Logging;
  ...
- var loggerMock = Substitute.For<ILogger>();
+ var loggerMock = Substitute.For<ILogger<LoyaltyKudosService>>();

  _loyaltyKudosService = new LoyaltyKudosService(uow, loggerMock, asyncRunner, mapper, _loyaltyKudosCalculator);
```

> Same note: keep `using Shrooms.Contracts.Infrastructure` if other types from it are used.

---

## Change 13 — `VacationBotServiceTests.cs`

**File:** `src/api/Shrooms.Premium.Tests/DomainService/VacationService/VacationBotServiceTests.cs`

```diff
- using Shrooms.Contracts.Infrastructure;
+ using Microsoft.Extensions.Logging;
  ...
- private ILogger _logger;
+ private ILogger<VacationBotService> _logger;
  ...
- _logger = Substitute.For<ILogger>();
+ _logger = Substitute.For<ILogger<VacationBotService>>();
```

---

## Execution order

Perform the changes in this order to keep the solution buildable between saves:

1. **Change 0** — Add `AddApplicationInsightsTelemetry()` to `Program.cs`
2. **Changes 5–10** — Update all 6 consumers (field type, constructor param, call sites)
3. **Changes 11–13** — Update 3 test mocks
4. **Change 2** — Remove `ILogger` registration from `InfrastructureModule.cs`
5. **Changes 3–4** — Delete `Logger.cs` then `ILogger.cs` (build will fail between these two if done separately; do them together)
6. **Change 1** — Remove `NLog.Web.AspNetCore` from `.csproj`
7. `dotnet build` — should be clean
8. `dotnet test` — all existing tests should pass

---

## Done checklist

- [ ] `Program.cs` — `AddApplicationInsightsTelemetry()` added
- [ ] `Shrooms.Infrastructure.csproj` — `NLog.Web.AspNetCore` removed
- [ ] `InfrastructureModule.cs` — `AddScoped<ILogger, Logger>()` removed
- [ ] `Logger.cs` deleted
- [ ] `ILogger.cs` deleted
- [ ] `AsyncRunner.cs` — `GetService<ILogger<AsyncRunner>>()`, `LogError(ex, ex.Message)`
- [ ] `VacationBotService.cs` — `ILogger<VacationBotService>`, two call sites updated
- [ ] `BookCoverService.cs` — `ILogger<BookCoverService>`, `LogError(ex, ex.Message)`
- [ ] `BookRemindService.cs` — `ILogger<BookRemindService>`, `LogDebug(e, e.Message)` (arg order!)
- [ ] `LotteryAbortJob.cs` — `ILogger<LotteryAbortJob>`, `LogError(e, e.Message)`
- [ ] `LoyaltyKudosService.cs` — `ILogger<LoyaltyKudosService>`, `LogError(e, e.Message)`
- [ ] `LotteryAbortJobTests.cs` — mock updated to `ILogger<LotteryAbortJob>`
- [ ] `LoyaltyKudosTests.cs` — mock updated to `ILogger<LoyaltyKudosService>`
- [ ] `VacationBotServiceTests.cs` — mock updated to `ILogger<VacationBotService>`
- [ ] `dotnet build` passes with no errors
- [ ] `dotnet test` passes
