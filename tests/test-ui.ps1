$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$work = Join-Path $repo ('work\ui-final-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $work -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $repo 'Playtime.exe') -Destination $work
$fw = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
foreach ($name in @('UiChecks','AddGameChecks')) {
 & "$fw\csc.exe" /nologo /target:winexe /reference:System.Xaml.dll /reference:Microsoft.CSharp.dll "/reference:$work\Playtime.exe" "/reference:$fw\WPF\WindowsBase.dll" "/reference:$fw\WPF\PresentationCore.dll" "/reference:$fw\WPF\PresentationFramework.dll" "/out:$work\$name.exe" (Join-Path $PSScriptRoot "$name.cs")
 if ($LASTEXITCODE -ne 0) { throw "$name compilation failed" }
 $resultDir = Join-Path $work $name
 $p = Start-Process -FilePath "$work\$name.exe" -ArgumentList ('"' + $resultDir + '"') -WindowStyle Hidden -PassThru
 if (-not $p.WaitForExit(25000)) { throw "$name timed out. Inspect the isolated test process." }
 if ($p.ExitCode -ne 0) { throw "$name failed. Inspect $resultDir" }
 Get-Content -LiteralPath (Join-Path $resultDir 'result.txt')
}
Write-Host $work
