param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

& "$repoRoot\build.ps1" -Configuration $Configuration

$iscc = Get-Command "iscc" -ErrorAction SilentlyContinue
if (-not $iscc) {
    $fallback = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
    if (Test-Path $fallback) {
        $iscc = $fallback
    } else {
        throw "Inno Setup compiler (ISCC.exe) not found. Install Inno Setup 6 from https://jrsoftware.org/isdl.php or add ISCC.exe to PATH."
    }
} else {
    $iscc = $iscc.Source
}

& $iscc "$PSScriptRoot\ShaderDemo.iss"

Write-Host "Installer output: installer\output\ShaderDemo-Setup-*.exe"
