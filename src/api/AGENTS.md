# Agents: backend API (`src/api`)

Instructions for AI coding agents working on the ASP.NET Core API. The frontend is the Next.js app in the `simoona-nextjs` repository; it has its own `AGENTS.md`.

## Cancellation on GET requests

When the browser gives up on a request, the Next.js route handler aborts its backend fetch, and ASP.NET Core cancels `HttpContext.RequestAborted`. GET endpoints pass that token down so the SQL stops too.

- **GET actions** take `CancellationToken cancellationToken = default` as the last parameter and pass it to the service they call.
- **Service methods** on a GET path take the same optional last parameter and pass it to every async EF call (`ToListAsync(cancellationToken)`, `FirstOrDefaultAsync`, `SingleOrDefaultAsync`, `CountAsync`, `AnyAsync`, `ToDictionaryAsync`, `FindAsync(keys, cancellationToken)`, `ToPagedListAsync(page, size, null, cancellationToken)`) and to the helper services they call (`RoleService.GetRoleIdByNameAsync`, `PermissionService.UserHasPermissionAsync`, ...). Keep the parameter optional (`= default`) so other callers and existing NSubstitute setups keep working.
- **GET only.** Do not pass the token to `SaveChangesAsync` or any write: a write stopped halfway is worse than one that finishes after the caller left.
- **Avoid calls that cannot take a token** on a GET path, for example `UserManager.GetUsersInRoleAsync`. Express them as an EF filter in the same query instead (`RoleService.OnlyUsersWithRoleName`, `ExcludeUsersWithRole`).
- **Do not catch `OperationCanceledException`** in actions or services. `RequestAbortedExceptionFilter` answers it with 499 when the caller is gone and logs it at Debug level.
- **CA2016 is a build warning** (`src/api/.editorconfig`). A CA2016 warning means a token was dropped somewhere on the path; fix it rather than suppressing it.

### Tests

- Mocked DbSets (`MockDbSetForAsync`, `SetDbSetDataForAsync`, `MockDbContext`) honour a cancelled token the way EF does. A new or changed GET service method gets a test that passes `new CancellationToken(true)` and expects `Throws.InstanceOf<OperationCanceledException>()`.
- Stub methods that take a token with `Arg.Any<CancellationToken>()`. A stub without it only matches `CancellationToken.None` and silently returns a default value once a test passes a real token.
