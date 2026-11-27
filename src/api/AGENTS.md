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

## Permissions and restricted roles

Roles are built from module permissions, and some leave most modules out, for example a role with only `VACATIONS_BASIC` and `TIMEREGISTRATION_BASIC`. "Holds permission X" always means what `PermissionService.GetUserPermissionsAsync` returns: a permission of any of the user's roles, in a module that is active for the organization.

### Notifications go only to people who can open the module

- **Email services derive from `NotificationServiceBase`.** A module's service passes its basic permission to the base constructor, for example `base(appSettings, mailTemplate, mailingService, permissionService, BasicPermissions.Event)`, and the base drops receivers without it before rendering. A new module email service does the same.
- **Wall posts, comments and mentions** need `WallReadPermission.For(wallType)`: `EVENT_BASIC` on event walls, `WALL_BASIC` elsewhere. Roles such as External read event walls on the event page without `WALL_BASIC`.
- **Bell notifications** filter their recipient ids in `NotificationService` (core and premium) with the same permission as the matching email.
- **Any other recipient list** goes through `IPermissionService.FilterEmailsByPermissionAsync` or `FilterUserIdsByPermissionAsync`, which look users up in batches. Do not check permissions one recipient at a time.
- **Being involved does not exempt an email.** Kudos received, a report joining an event (sent to the manager) and a book taken all need the module's permission: the recipient could not open the link otherwise.
- **Keep these exempt:** vacation and overtime (managers get them by being the manager, whatever their own role), service requests (marker roles such as `ServiceRequestNotification` carry no module permission), and birthday, support and account mail (administrators or the account's own user). The web app's notification settings always show the vacation and time registration toggles for that reason.

### Lists of users

To list only users who hold a permission, add `PermissionService.OnlyUsersWithPermission(permission)` to the users query, as `EmployeeListingService` does, so paging and counting stay in SQL.

### Endpoints about one user

- **A user's own record does not need the module permission.** When the user id is in the route or query string, name it in the attribute: `[PermissionAuthorize(Permission = BasicPermissions.ApplicationUser, OwnUserIdParameter = "id")]` lets callers in for their own id and keeps the permission for anyone else.
- **When the id is in the body,** the attribute cannot see it. Check "the user themself or an `APPLICATIONUSER_ADMINISTRATION` holder" in the action, as `PutPersonalInfo` (`CanAccessAsync`), `PutJobInfo`, `PutOfficeInfo` and `PutExams` do, and leave the basic-permission attribute off.
- **Endpoints that only read or change the signed-in user** (`User/GeneralSettings`, `User/Notifications`, `User/Logins`) take the user from the token and need only `[Authorize]`.

### Organization filter

`EfRepository` scopes `IOrganization` entities to the signed-in user's `OrganizationId` claim, and falls back to the `DefaultOrganizationId` setting only when there is no request user (background work). One local API serves tenants whose organizations have different ids, so do not filter request data by `DefaultOrganizationId`.
