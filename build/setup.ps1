<#
.SYNOPSIS
    Sets up a fresh Simoona database: seeds reference data and creates the first admin user.

.DESCRIPTION
    1. Runs seed.sql against the target SQL Server using sqlcmd.
    2. Computes an ASP.NET Core Identity V3 (PBKDF2-SHA256) password hash in PowerShell.
    3. Inserts the admin user + assigns the Admin role + adds them to the Official wall.

    Requires sqlcmd to be installed on the host.
    EF Core migrations must be applied before running this script.

.PARAMETER ConnectionString
    ADO.NET connection string for the SimoonaDB database.
    Example: "Server=localhost,1434;Database=SimoonaDB;User Id=sa;Password=Password!123;TrustServerCertificate=True"

.PARAMETER Email
    E-mail address for the admin account (also used as the username).

.PARAMETER Password
    Password for the admin account.

.PARAMETER OrgName
    Short name of the organisation (default: testorg).
    Updates dbo.Organizations after seeding.

.EXAMPLE
    .\setup.ps1 -ConnectionString "Server=localhost,1434;Database=SimoonaDB;User Id=sa;Password=Password!123;TrustServerCertificate=True" -Email admin@company.com -Password 'S3cur3P@ss!'
    .\setup.ps1 -ConnectionString "Server=localhost,1434;Database=SimoonaDB;User Id=sa;Password=Password!123;TrustServerCertificate=True" -Email admin@company.com -Password 'S3cur3P@ss!' -OrgName acme
#>
param(
    [Parameter(Mandatory)][string]$ConnectionString,
    [Parameter(Mandatory)][string]$Email,
    [Parameter(Mandatory)][string]$Password,
    [string]$OrgName = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Parse connection string into sqlcmd arguments (avoid DbConnectionStringBuilder type issues)
$dict = @{}
$ConnectionString -split ';' | ForEach-Object {
    $kv = $_ -split '=', 2
    if ($kv.Count -eq 2) { $dict[$kv[0].Trim()] = $kv[1].Trim() }
}
$Server   = if ($dict['Server'])       { $dict['Server'] }       else { $dict['Data Source'] }
$Database = if ($dict['Database'])     { $dict['Database'] }     else { $dict['Initial Catalog'] }
$User     = if ($dict['User Id'])      { $dict['User Id'] }      elseif ($dict['User ID']) { $dict['User ID'] } else { $dict['UID'] }
$DbPass   = if ($dict['Password'])     { $dict['Password'] }     else { $dict['PWD'] }

$SqlArgs = @("-S", $Server, "-d", $Database, "-b", "-No")
if ($User) { $SqlArgs += @("-U", $User, "-P", $DbPass) } else { $SqlArgs += "-E" }  # -E = Windows auth

function Invoke-Sql([string]$Query) {
    & sqlcmd @SqlArgs -Q $Query
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed (exit $LASTEXITCODE)" }
}

# ── 1. Seed reference data ────────────────────────────────────────────────────
Write-Host "→ Seeding reference data..." -ForegroundColor Cyan
$ScriptDir = Split-Path $MyInvocation.MyCommand.Path
& sqlcmd @SqlArgs -i "$ScriptDir\seed.sql"
if ($LASTEXITCODE -ne 0) { throw "seed.sql failed" }
Write-Host "  Reference data seeded." -ForegroundColor Green

# ── 2. Update organisation name (optional) ───────────────────────────────────
if ($OrgName -ne "") {
    Write-Host "→ Updating organisation name to '$OrgName'..." -ForegroundColor Cyan
    Invoke-Sql "UPDATE dbo.Organizations SET Name='$OrgName', ShortName='$OrgName' WHERE Id=1"
    Write-Host "  Organisation updated." -ForegroundColor Green
}

# ── 3. Compute ASP.NET Core Identity V3 password hash (PBKDF2-SHA256) ────────
Write-Host "→ Hashing password..." -ForegroundColor Cyan
$saltBytes = [byte[]]::new(16)
[System.Security.Cryptography.RandomNumberGenerator]::Fill($saltBytes)

$pbkdf2 = New-Object System.Security.Cryptography.Rfc2898DeriveBytes(
    [System.Text.Encoding]::UTF8.GetBytes($Password),
    $saltBytes,
    10000,
    [System.Security.Cryptography.HashAlgorithmName]::SHA256
)
$subkey = $pbkdf2.GetBytes(32)
$pbkdf2.Dispose()

# Header: 0x01 | PRF(1=HMACSHA256) | iterations | saltLen  – all big-endian uint32
function ConvertTo-BigEndian([uint32]$v) {
    $b = [System.BitConverter]::GetBytes($v); [Array]::Reverse($b); return $b
}
$hashBytes     = [byte[]]@(0x01) + (ConvertTo-BigEndian 1) + (ConvertTo-BigEndian 10000) + (ConvertTo-BigEndian 16) + $saltBytes + $subkey
$passwordHash  = [System.Convert]::ToBase64String($hashBytes)
$securityStamp = [System.Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(20))
$userId        = [System.Guid]::NewGuid().ToString()
$normalised    = $Email.ToUpperInvariant()

# ── 4. Insert admin user ──────────────────────────────────────────────────────
Write-Host "→ Creating admin user '$Email'..." -ForegroundColor Cyan

$checkExists = (& sqlcmd @SqlArgs -h -1 -Q "SET NOCOUNT ON; SELECT COUNT(1) FROM dbo.AspNetUsers WHERE NormalizedEmail='$normalised'")
if ($checkExists.Trim() -ne "0") {
    Write-Host "  User '$Email' already exists – skipping." -ForegroundColor Yellow
    exit 0
}

Invoke-Sql @"
INSERT INTO dbo.AspNetUsers
    (Id, FirstName, LastName, OrganizationId, Email, NormalizedEmail,
     EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled,
     AccessFailedCount, UserName, NormalizedUserName, SecurityStamp,
     IsManagingDirector, EmploymentDate, PasswordHash, IsDeleted, ConcurrencyStamp,
     IsAbsent, IsAnonymized, TotalKudos, RemainingKudos, SittingPlacesChanged, SpentKudos,
     IsOwner, Created, Modified, IsTutorialComplete)
VALUES
    ('$userId', 'Admin', 'Admin', 1, '$Email', '$normalised',
     1, 0, 0, 0,
     0, '$Email', '$normalised', '$securityStamp',
     1, GETDATE(), '$passwordHash', 0, NEWID(),
     0, 0, 0, 0, 0, 0,
     0, GETDATE(), GETDATE(), 1)
"@

Invoke-Sql "INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) SELECT '$userId', Id FROM dbo.AspNetRoles WHERE NormalizedName = 'ADMIN'"

Invoke-Sql "INSERT INTO dbo.WallModerators (WallId, UserId, IsDeleted, Created, Modified) VALUES (1, '$userId', 0, '1900-01-01', '1900-01-01')"

Invoke-Sql @"
INSERT INTO dbo.WallMembers
    (WallId, UserId, Created, CreatedBy, Modified, ModifiedBy,
     IsDeleted, EmailNotificationsEnabled, AppNotificationsEnabled)
VALUES (1, '$userId', GETDATE(), NULL, GETDATE(), NULL, 0, 0, 1)
"@

Write-Host "  Admin user '$Email' created." -ForegroundColor Green
Write-Host "Done." -ForegroundColor White
