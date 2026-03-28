#Requires -Version 5.1
<#
.SYNOPSIS
    Installs the .NET SDK needed to build vcon.

.DESCRIPTION
    vcon targets net8.0-windows (WPF + WinForms); see Directory.Build.props.
    This script ensures a .NET 8 SDK is present. It prefers winget, then the
    official dotnet-install.ps1 from dot.net.

.PARAMETER Force
    Run the installer even if an 8.x SDK is already detected.

.EXAMPLE
    .\install_dotnet.ps1
#>
[CmdletBinding()]
param(
    [switch] $Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RequiredSdkMajor = 8
$DotNetInstallScriptUri = 'https://dot.net/v1/dotnet-install.ps1'

function Test-DotNet8SdkPresent {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnet) { return $false }
    $list = & dotnet --list-sdks 2>$null
    if ($LASTEXITCODE -ne 0) { return $false }
    foreach ($line in $list) {
        if ($line -match '^\s*(\d+)\.') {
            if ([int]$Matches[1] -eq $RequiredSdkMajor) { return $true }
        }
    }
    return $false
}

function Install-ViaWinget {
    $winget = Get-Command winget -ErrorAction SilentlyContinue
    if (-not $winget) { return $false }

    Write-Host 'Installing Microsoft.DotNet.SDK.8 via winget...'
    $wingetArgs = @(
        'install',
        '-e',
        '--id', 'Microsoft.DotNet.SDK.8',
        '--accept-package-agreements',
        '--accept-source-agreements'
    )
    & winget @wingetArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "winget exited with code $LASTEXITCODE; trying dotnet-install.ps1 fallback."
        return $false
    }
    return $true
}

function Install-ViaDotNetInstallScript {
    $temp = Join-Path ([System.IO.Path]::GetTempPath()) ('dotnet-install-' + [Guid]::NewGuid().ToString('n') + '.ps1')
    try {
        Write-Host "Downloading dotnet-install.ps1 from $DotNetInstallScriptUri ..."
        Invoke-WebRequest -Uri $DotNetInstallScriptUri -OutFile $temp -UseBasicParsing

        Write-Host "Installing .NET SDK $RequiredSdkMajor.0 (latest in channel)..."
        & $temp -Channel "$RequiredSdkMajor.0"
        if (-not $?) {
            throw 'dotnet-install.ps1 reported failure.'
        }
    }
    finally {
        Remove-Item -LiteralPath $temp -ErrorAction SilentlyContinue
    }

    $dotnetRoot = $env:DOTNET_ROOT
    if (-not $dotnetRoot) {
        $dotnetRoot = Join-Path $env:LOCALAPPDATA 'Microsoft\dotnet'
    }
    $dotnetExe = Join-Path $dotnetRoot 'dotnet.exe'
    if (Test-Path -LiteralPath $dotnetExe) {
        Write-Host ""
        Write-Host "SDK was installed under: $dotnetRoot"
        Write-Host 'If `dotnet` is not found in a new terminal, add this directory to PATH:'
        Write-Host "  $dotnetRoot"
    }
}

if (-not $Force -and (Test-DotNet8SdkPresent)) {
    Write-Host "A .NET $RequiredSdkMajor.x SDK is already installed."
    & dotnet --list-sdks
    exit 0
}

if ($env:OS -ne 'Windows_NT') {
    Write-Error 'vcon builds on Windows (net8.0-windows). Run this script on Windows.'
    exit 1
}

if (Install-ViaWinget) {
    # Refresh PATH in this session (typical install location).
    $machinePath = [Environment]::GetEnvironmentVariable('Path', 'Machine')
    $userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
    $env:Path = @($machinePath, $userPath) -join ';'
}

if (-not (Test-DotNet8SdkPresent)) {
    Install-ViaDotNetInstallScript
}

if (-not (Test-DotNet8SdkPresent)) {
    Write-Error 'Installation finished but a .NET 8 SDK was not detected. Open a new terminal and run: dotnet --list-sdks'
    exit 1
}

Write-Host ''
Write-Host 'dotnet SDKs:'
& dotnet --list-sdks
Write-Host ''
Write-Host 'Done. You can build with: dotnet build'
