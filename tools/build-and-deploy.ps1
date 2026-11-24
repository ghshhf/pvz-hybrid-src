# pvz-hybrid-src: one-shot build -> deploy -> headless verify.
# Usage: powershell -ExecutionPolicy Bypass -File tools\build-and-deploy.ps1
# NOTE: ASCII-only source (PowerShell 5.1 reads files as GBK; Chinese literals break parsing).
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$Repo = Split-Path -Parent $PSScriptRoot
$Sdk  = 'D:\DevTools\dotnet\dotnet.exe'
$Root = 'D:\pvz-game\game-0.31'
$Game = (Get-ChildItem $Root -Directory | Select-Object -First 1).FullName
$DataDir = Get-ChildItem $Game -Directory | Where-Object { $_.Name -like 'data_*' } | Select-Object -First 1
$DataDir = $DataDir.FullName
$SrcDll = Join-Path $Repo 'bin\Debug\net9.0\PlantsVsZombies.dll'
$DstDll = Join-Path $DataDir 'PlantsVsZombies.dll'
$Exe = (Get-ChildItem $Game -Filter '*.exe' -File | Where-Object { $_.Name -like '*Csharp*' } | Select-Object -First 1).FullName
$Log = Join-Path $PSScriptRoot 'last-run.log'

$env:DOTNET_ROOT = Split-Path -Parent $Sdk
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'

Write-Host "[1/4] Build..."
Set-Location $Repo
$out = & $Sdk build -c Debug -v:minimal 2>&1 | Out-String
if ($LASTEXITCODE -ne 0 -or ([regex]::Matches($out, 'error CS\d+').Count -gt 0)) {
    Write-Error "Build failed: $out"
}
Write-Host "      OK ($((Get-Item $SrcDll).Length) bytes)"

Write-Host "[2/4] Backup + deploy dll..."
if (-not (Test-Path "$DstDll.orig")) { Copy-Item $DstDll "$DstDll.orig" -Force }
Copy-Item $SrcDll $DstDll -Force
Write-Host "      OK"

Write-Host "[3/4] Headless smoke test..."
$cmdline = 'cmd.exe /c ""' + $Exe + '" --headless --quit-after 200 > "' + $Log + '" 2>&1"'
$wmi = [wmiclass]'Win32_Process'
$r = $wmi.Create($cmdline, $Game, $null)
if ($r.ReturnValue -ne 0) { Write-Error "Launch failed: WMI result $($r.ReturnValue)" }
Start-Sleep -Seconds 22
if (-not (Test-Path $Log)) { Write-Error "No runtime log produced" }
$c = Get-Content $Log -Raw
if ($c -notmatch 'Full gameplay resources ready') { Write-Error "Game did not finish resource load: $c" }
$realErrors = $c -split "`n" | Where-Object {
    $_ -match '(^|\s)(ERROR|Unhandled exception|SCRIPT ERROR)' -and $_ -notmatch 'Previous Godot log'
}
if ($realErrors) { Write-Error "Runtime errors: $($realErrors -join ' | ')" }
Write-Host "      OK (resources ready, 0 real errors)"

Write-Host "[4/4] Done. Dll deployed to game dir."
Write-Host "Runtime log: $Log"
