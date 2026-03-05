<#
.SYNOPSIS
    Posts a dev log update to a Discord channel via webhook.

.DESCRIPTION
    Reads the webhook URL from discord_config.json and sends a formatted
    embed message to the configured Discord channel.

.PARAMETER Title
    Title of the dev log entry (e.g., "Session: Cloud System & Engine VFX")

.PARAMETER Changes
    Array of change descriptions. Each becomes a bullet point.

.PARAMETER Color
    Optional embed color as decimal int. Default: 3447003 (blue)
    Presets: Blue=3447003, Green=3066993, Orange=15105570,
             Red=15158332, Gold=15844367, Purple=10181046

.PARAMETER Footer
    Optional footer text. Default: "Dev Log"

.EXAMPLE
    .\post_devlog.ps1 -Title "Session: Cloud and VFX" -Changes "Cloud system upgraded","Engine flames added","Bug fixed"

.EXAMPLE
    .\post_devlog.ps1 -Title "Bug Fixes" -Changes "Fixed crash","Patched leak" -Color 3066993 -Footer "My Game | v0.1"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Title,

    [Parameter(Mandatory = $true)]
    [string[]]$Changes,

    [int]$Color = 3447003,

    [string]$Footer = "Dev Log"
)

# Load config
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$configPath = Join-Path $scriptDir "discord_config.json"

if (-not (Test-Path $configPath)) {
    Write-Error "discord_config.json not found at $configPath"
    Write-Host ""
    Write-Host "Make sure discord_config.json is in the same folder as this script." -ForegroundColor Yellow
    Write-Host "See README.md for setup instructions." -ForegroundColor Yellow
    exit 1
}

$config = Get-Content $configPath -Raw | ConvertFrom-Json

if (-not $config.webhook_url -or $config.webhook_url -eq "PASTE_YOUR_WEBHOOK_URL_HERE") {
    Write-Error "No webhook_url configured in discord_config.json"
    Write-Host ""
    Write-Host "Open discord_config.json and paste your Discord webhook URL." -ForegroundColor Yellow
    Write-Host "See README.md for how to create a webhook." -ForegroundColor Yellow
    exit 1
}

# Build the description from changes
$bullets = $Changes | ForEach-Object { [char]0x2022 + " " + $_ }
$description = $bullets -join [char]10

# Timestamp
$timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")

# Build payload manually to avoid ConvertTo-Json encoding issues
$escapedTitle = $Title.Replace('\', '\\').Replace('"', '\"')
$escapedDesc = $description.Replace('\', '\\').Replace('"', '\"')
$escapedDesc = $escapedDesc -replace "`r`n", '\n' -replace "`n", '\n' -replace "`r", ''
$escapedFooter = $Footer.Replace('\', '\\').Replace('"', '\"')

# Use configured bot name, or fall back to "Dev Log"
$botName = if ($config.bot_name) { $config.bot_name } else { "Dev Log" }
$escapedName = $botName.Replace('\', '\\').Replace('"', '\"')

$json = @"
{
  "username": "$escapedName",
  "embeds": [{
    "title": "$escapedTitle",
    "description": "$escapedDesc",
    "color": $Color,
    "timestamp": "$timestamp",
    "footer": { "text": "$escapedFooter" }
  }]
}
"@

# Send it
try {
    $utf8 = [System.Text.Encoding]::UTF8.GetBytes($json)
    Invoke-RestMethod -Uri $config.webhook_url -Method Post -ContentType "application/json; charset=utf-8" -Body $utf8
    Write-Host "Posted: $Title" -ForegroundColor Green
}
catch {
    Write-Error "Failed to post: $_"
    Write-Host "JSON payload was:" -ForegroundColor Yellow
    Write-Host $json
    exit 1
}
