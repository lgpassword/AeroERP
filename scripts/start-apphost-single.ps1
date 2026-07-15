param(
  [int]$Port = 5099,
  [string]$Configuration = "Debug",
  [string]$Output = ".artifacts/apphost-single",
  [string]$SqlitePath = "",
  [switch]$NoPublish,
  [switch]$StopExisting
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$outputPath = Join-Path $repoRoot $Output
$logsPath = Join-Path $repoRoot "logs"
$appHostProject = Join-Path $repoRoot "src/AeroERP.AppHost/AeroERP.AppHost.csproj"
$appHostExe = Join-Path $outputPath "AeroERP.AppHost.exe"
$stdoutLog = Join-Path $logsPath "apphost-single.out.log"
$stderrLog = Join-Path $logsPath "apphost-single.err.log"

New-Item -ItemType Directory -Force -Path $logsPath | Out-Null

$listeners = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
if ($listeners -and -not $StopExisting) {
  $processIds = ($listeners | Select-Object -ExpandProperty OwningProcess -Unique) -join ", "
  Write-Host "Port $Port is already listening. Owning process id(s): $processIds"
  Write-Host "Use -StopExisting to stop the current listener before starting AeroERP."
  exit 0
}

if ($listeners -and $StopExisting) {
  $listeners | Select-Object -ExpandProperty OwningProcess -Unique | ForEach-Object {
    Stop-Process -Id $_ -Force
  }
  Start-Sleep -Seconds 2
}

Push-Location $repoRoot
try {
  if (-not $NoPublish) {
    dotnet publish $appHostProject `
      -c $Configuration `
      -o $outputPath `
      -p:PublishSingleFile=true `
      -p:SelfContained=false `
      -p:UseAppHost=true
  }

  if (-not (Test-Path $appHostExe)) {
    throw "Single-file AppHost was not found: $appHostExe"
  }

  Remove-Item -LiteralPath $stdoutLog,$stderrLog -ErrorAction SilentlyContinue

  if ($SqlitePath) {
    $resolvedSqlitePath = if ([System.IO.Path]::IsPathRooted($SqlitePath)) {
      $SqlitePath
    } else {
      Join-Path $repoRoot $SqlitePath
    }
    $env:ConnectionStrings__Sqlite = "Data Source=$resolvedSqlitePath"
  }

  try {
    $process = Start-Process `
      -FilePath $appHostExe `
      -ArgumentList @("--urls", "http://localhost:$Port") `
      -WorkingDirectory $repoRoot `
      -RedirectStandardOutput $stdoutLog `
      -RedirectStandardError $stderrLog `
      -PassThru `
      -WindowStyle Hidden
  } finally {
    Remove-Item Env:\ConnectionStrings__Sqlite -ErrorAction SilentlyContinue
  }

  $ready = $false
  for ($i = 0; $i -lt 30; $i++) {
    Start-Sleep -Seconds 1
    if (Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue) {
      $ready = $true
      break
    }
    if ($process.HasExited) {
      throw "AeroERP.AppHost exited early with code $($process.ExitCode). See $stderrLog"
    }
  }

  if (-not $ready) {
    throw "AeroERP.AppHost did not listen on port $Port within 30 seconds. See $stderrLog"
  }

  Write-Host "AeroERP AppHost is running."
  Write-Host "Backend: http://localhost:$Port"
  Write-Host "ProcessId: $($process.Id)"
  Write-Host "Logs: $stdoutLog"
} finally {
  Pop-Location
}
