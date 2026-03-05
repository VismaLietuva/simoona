<#
.SYNOPSIS
    Sets up a fresh Simoona database: seeds reference data and creates the first admin user.

.DESCRIPTION
    1. Runs seed.sql against the target SQL Server.
    2. Computes an ASP.NET Core Identity V3 (PBKDF2-SHA256) password hash in PowerShell.
    3. Inserts the admin user + assigns the Admin role + adds them to the Official wall.

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
    [string]$ConnectionString = $env:ConnectionStrings__DefaultConnection,
    [Parameter(Mandatory)][string]$Email,
    [Parameter(Mandatory)][string]$Password,
    [string]$OrgName = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Data

function Invoke-SqlBatches([string]$Sql) {
    $conn = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $conn.Open()
    try {
        # Split on GO statements (T-SQL batch separator)
        $batches = $Sql -split '(?m)^\s*GO\s*$'
        foreach ($batch in $batches) {
            $trimmed = $batch.Trim()
            if ($trimmed -eq '') { continue }
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = $trimmed
            $cmd.CommandTimeout = 120
            $cmd.ExecuteNonQuery() | Out-Null
        }
    } finally {
        $conn.Close()
    }
}

function Invoke-Sql([string]$Query) {
    Invoke-SqlBatches -Sql $Query
}

function Invoke-SqlScalar([string]$Query) {
    $conn = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $conn.Open()
    try {
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $Query
        $cmd.CommandTimeout = 30
        return $cmd.ExecuteScalar()
    } finally {
        $conn.Close()
    }
}

# ── 1. Seed reference data ────────────────────────────────────────────────────
Write-Host "→ Seeding reference data..." -ForegroundColor Cyan
$ScriptDir = Split-Path $MyInvocation.MyCommand.Path
$seedSql = Get-Content "$ScriptDir\seed.sql" -Raw
Invoke-SqlBatches -Sql $seedSql
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

$checkExists = Invoke-SqlScalar "SELECT COUNT(1) FROM dbo.AspNetUsers WHERE NormalizedEmail='$normalised'"
if ([int]$checkExists -ne 0) {
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
