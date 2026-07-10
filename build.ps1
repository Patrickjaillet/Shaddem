param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

dotnet restore ShaderDemo.sln

dotnet publish src/ShaderDemo.App/ShaderDemo.App.csproj `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -o publish

Write-Host "Build output: publish/ShaderDemo.exe"
