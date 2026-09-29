$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $PSScriptRoot
$jsonFiles = @(
    (Join-Path $projectRoot 'Packages\manifest.json')
)
$jsonFiles += Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Assets\SwipeClean') -Recurse -File |
    Where-Object { $_.Extension -in @('.asmdef', '.inputactions') } |
    Select-Object -ExpandProperty FullName

foreach ($jsonFile in $jsonFiles) {
    Get-Content -LiteralPath $jsonFile -Raw | ConvertFrom-Json | Out-Null
}

dotnet run --project (Join-Path $projectRoot 'Tools\PureLogicChecks\SwipeClean.PureLogicChecks.csproj') --configuration Release
if ($LASTEXITCODE -ne 0) {
    throw 'Pure logic checks failed.'
}

$unityEditorRoot = 'D:\Unity\Hub\Editor\6000.3.23f1'
$unityCore = Join-Path $unityEditorRoot 'Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll'
if (Test-Path -LiteralPath $unityCore) {
    dotnet build (Join-Path $projectRoot 'Tools\UnityApiChecks\SwipeClean.UnityApiChecks.csproj') `
        --configuration Release -p:UnityEditorRoot=$unityEditorRoot
    if ($LASTEXITCODE -ne 0) {
        throw 'Unity API checks failed.'
    }
}

Write-Output "Validated $($jsonFiles.Count) JSON assets, pure logic, and available Unity runtime APIs."
