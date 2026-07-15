param(
  [string]$Configuration = "Debug",
  [switch]$SkipRestore,
  [switch]$SkipFrontend,
  [switch]$InstallNodeDependencies
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $repoRoot "AeroERP.slnx"
$appHostTests = Join-Path $repoRoot "tests/AeroERP.AppHost.Tests/AeroERP.AppHost.Tests.csproj"

function Invoke-Step {
  param(
    [string]$Name,
    [scriptblock]$Command
  )

  Write-Host ""
  Write-Host "==> $Name"
  & $Command
  if ($LASTEXITCODE -ne 0) {
    throw "$Name failed with exit code $LASTEXITCODE."
  }
}

Push-Location $repoRoot
try {
  if (-not $SkipRestore) {
    Invoke-Step "Restore .NET solution" {
      dotnet restore $solutionPath
    }
  }

  Invoke-Step "Build .NET solution" {
    dotnet build $solutionPath --no-restore --configuration $Configuration --disable-build-servers
  }

  Invoke-Step "Run AppHost integration tests" {
    dotnet test $appHostTests --no-build --configuration $Configuration --logger "console;verbosity=minimal"
  }

  if (-not $SkipFrontend) {
    if ($InstallNodeDependencies) {
      Invoke-Step "Install npm dependencies" {
        npm ci
      }
    }

    Invoke-Step "Build frontend workspace" {
      npm run build
    }
  }

  Write-Host ""
  Write-Host "Verification completed successfully."
} finally {
  Pop-Location
}
