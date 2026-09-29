$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $PSScriptRoot
$unityEditor = 'D:\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe'

if (-not (Test-Path -LiteralPath $unityEditor)) {
    throw "Unity Editor not found: $unityEditor"
}

Start-Process -FilePath $unityEditor -ArgumentList '-projectPath', $projectRoot

