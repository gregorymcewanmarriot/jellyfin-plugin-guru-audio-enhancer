$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$Project = Join-Path $Root "Jellyfin.Plugin.GuruAudioEnhancer/Jellyfin.Plugin.GuruAudioEnhancer.csproj"
[xml]$ProjectXml = Get-Content $Project
$Version = $ProjectXml.Project.PropertyGroup.Version | Select-Object -First 1
$Publish = Join-Path $Root "publish"
$Dist = Join-Path $Root "dist"
$Package = Join-Path $Dist "package"
$Asset = Join-Path $Dist "GuruAudioEnhancer-$Version-jf12.zip"

Remove-Item $Publish -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $Package -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $Publish, $Package, $Dist | Out-Null

dotnet restore $Project
dotnet publish $Project -c Release -o $Publish "-p:Version=$Version" "-p:AssemblyVersion=$Version" "-p:FileVersion=$Version"

Copy-Item (Join-Path $Publish "Jellyfin.Plugin.GuruAudioEnhancer.dll") $Package
Copy-Item (Join-Path $Root "docs/images/logo.png") $Package
python (Join-Path $Root ".github/scripts/create_meta.py") --version $Version --target-abi "12.0.0.0" --repository "local/GuruAudioEnhancer" --output (Join-Path $Package "meta.json")

Remove-Item $Asset -Force -ErrorAction SilentlyContinue
Compress-Archive -Path (Join-Path $Package "*") -DestinationPath $Asset
Write-Host "Built package: $Asset"
Write-Host "DLL: $(Join-Path $Publish 'Jellyfin.Plugin.GuruAudioEnhancer.dll')"
