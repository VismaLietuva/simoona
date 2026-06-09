<#
.SYNOPSIS
    Sets up IIS to serve Simoona webapp at http://app.simoona.local/
    Replicates what the old build.cake local-setup did.

.DESCRIPTION
    - Creates the "Simoona" application pool (LocalSystem)
    - Creates (or updates) the "SimoonaWebApp" IIS site bound to app.simoona.local:80
      pointing at the gulp build output folder
    - Verifies the hosts entry 127.0.0.1 app.simoona.local

    Must be run as Administrator.

.EXAMPLE
    .\local-iis-setup.ps1
#>

#Requires -RunAsAdministrator

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$webAppHostName   = "app.simoona.local"
$applicationPool  = "Simoona"
$siteName         = "SimoonaWebApp"
$webAppBuildPath  = Resolve-Path "$PSScriptRoot\..\src\webapp\build" -ErrorAction SilentlyContinue

if (-not $webAppBuildPath) {
    Write-Error "Build folder not found at $PSScriptRoot\..\src\webapp\build — run 'npx gulp build-dev' first."
    exit 1
}

Import-Module WebAdministration

# ── 1. Application Pool ───────────────────────────────────────────────────────
if (Test-Path "IIS:\AppPools\$applicationPool") {
    Write-Host "→ App pool '$applicationPool' already exists." -ForegroundColor Yellow
} else {
    Write-Host "→ Creating app pool '$applicationPool'..." -ForegroundColor Cyan
    New-WebAppPool -Name $applicationPool | Out-Null
    Set-ItemProperty "IIS:\AppPools\$applicationPool" -Name processModel.identityType -Value 0  # LocalSystem
    Write-Host "  App pool created." -ForegroundColor Green
}

# ── 2. IIS Website ────────────────────────────────────────────────────────────
if (Get-Website -Name $siteName -ErrorAction SilentlyContinue) {
    Write-Host "→ Site '$siteName' already exists — updating physical path and binding..." -ForegroundColor Yellow
    Set-ItemProperty "IIS:\Sites\$siteName" -Name physicalPath -Value $webAppBuildPath.Path
    # Remove existing bindings and re-add
    Get-WebBinding -Name $siteName | Remove-WebBinding
    New-WebBinding -Name $siteName -Protocol "http" -HostHeader $webAppHostName -IPAddress "*" -Port 80
    Write-Host "  Site updated." -ForegroundColor Green
} else {
    Write-Host "→ Creating IIS site '$siteName'..." -ForegroundColor Cyan
    New-Website -Name $siteName `
                -PhysicalPath $webAppBuildPath.Path `
                -ApplicationPool $applicationPool `
                -HostHeader $webAppHostName `
                -IPAddress "*" `
                -Port 80 `
                -Force | Out-Null
    Write-Host "  Site created." -ForegroundColor Green
}

# Start it if stopped
$site = Get-Website -Name $siteName
if ($site.State -ne "Started") {
    Start-Website -Name $siteName
    Write-Host "  Site started." -ForegroundColor Green
}

# ── 3. Hosts entry ────────────────────────────────────────────────────────────
$hostsFile = "$env:SystemRoot\System32\drivers\etc\hosts"
$hostsContent = Get-Content $hostsFile -Raw
if ($hostsContent -notmatch [regex]::Escape($webAppHostName)) {
    Write-Host "→ Adding hosts entry for $webAppHostName..." -ForegroundColor Cyan
    Add-Content $hostsFile "`r`n127.0.0.1`t$webAppHostName"
    Write-Host "  Hosts entry added." -ForegroundColor Green
} else {
    Write-Host "→ Hosts entry for $webAppHostName already present." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Done! Simoona webapp is available at http://$webAppHostName/" -ForegroundColor White
Write-Host "API still runs via Kestrel — start it from Visual Studio or 'dotnet run'." -ForegroundColor Gray
Write-Host ""
Write-Host "To update the frontend after code changes:" -ForegroundColor Gray
Write-Host "  cd src\webapp && npx gulp build-dev" -ForegroundColor Gray
