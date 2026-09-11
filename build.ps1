param([switch]$Package)
$ErrorActionPreference = 'Stop'
$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$compiler = Join-Path $framework 'csc.exe'
$wpf = Join-Path $framework 'WPF'
if (-not (Test-Path -LiteralPath $compiler)) { throw '.NET Framework 4.8 is required to build.' }
Add-Type -AssemblyName System.Drawing
# Draw a small original clock icon; no external icon or artwork dependencies.
$bitmap = New-Object Drawing.Bitmap 64,64
$graphics = [Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = 'AntiAlias'
$graphics.Clear([Drawing.Color]::Transparent)
$brush = New-Object Drawing.SolidBrush ([Drawing.Color]::FromArgb(0,122,255))
$graphics.FillEllipse($brush,2,2,60,60)
$pen = New-Object Drawing.Pen ([Drawing.Color]::White),5
$pen.StartCap = 'Round'; $pen.EndCap = 'Round'
$graphics.DrawLine($pen,32,16,32,33); $graphics.DrawLine($pen,32,33,44,40)
$icon = [Drawing.Icon]::FromHandle($bitmap.GetHicon())
$stream = [IO.File]::Create((Join-Path $PSScriptRoot 'src\app.ico'))
try { $icon.Save($stream) } finally { $stream.Dispose(); $icon.Dispose(); $pen.Dispose(); $brush.Dispose(); $graphics.Dispose(); $bitmap.Dispose() }
$sources = @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'src') -Filter '*.cs' | Select-Object -ExpandProperty FullName)
& $compiler /nologo /target:winexe /platform:x64 /optimize+ /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll /reference:System.Management.dll /reference:Microsoft.CSharp.dll /reference:System.Xaml.dll "/reference:$wpf\WindowsBase.dll" "/reference:$wpf\PresentationCore.dll" "/reference:$wpf\PresentationFramework.dll" "/win32manifest:$PSScriptRoot\src\app.manifest" "/win32icon:$PSScriptRoot\src\app.ico" "/out:$PSScriptRoot\Playtime.exe" $sources
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
if ($Package) {
 $dist = Join-Path $PSScriptRoot 'dist'
 New-Item -ItemType Directory -Path $dist -Force | Out-Null
 $stage = Join-Path $dist ('stage-' + [Guid]::NewGuid().ToString('N'))
 New-Item -ItemType Directory -Path $stage -Force | Out-Null
 Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Playtime.exe'),(Join-Path $PSScriptRoot 'README.md'),(Join-Path $PSScriptRoot 'LICENSE') -Destination $stage
 Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'docs') -Destination $stage -Recurse
 $zip = Join-Path $dist 'Playtime-v0.1.0-windows-x64.zip'
 Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -Force
 Get-FileHash -LiteralPath $zip -Algorithm SHA256 | ForEach-Object { $_.Hash.ToLower() + '  ' + [IO.Path]::GetFileName($_.Path) } | Set-Content -LiteralPath (Join-Path $dist 'SHA256SUMS.txt') -Encoding ascii
 Write-Host $zip
}
