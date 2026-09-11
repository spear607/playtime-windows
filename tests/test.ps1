$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$testDir = Join-Path $repo ('work\tests-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDir -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $repo 'Playtime.exe') -Destination $testDir
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
& $csc /nologo /target:exe "/reference:$testDir\Playtime.exe" "/out:$testDir\CoreTests.exe" (Join-Path $PSScriptRoot 'CoreTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed' }
& (Join-Path $testDir 'CoreTests.exe') (Join-Path $testDir 'data')
if ($LASTEXITCODE -ne 0) { throw 'Tests failed' }
