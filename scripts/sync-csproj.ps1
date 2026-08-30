# Refreshes battle.sln and patches .csproj Compile lists when Unity project files are stale.
# Prefer Unity: Edit -> Preferences -> External Tools -> Regenerate project files (with Unity open).

$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

$unity = "C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe"
$logFile = Join-Path $root "Temp\regenerate-log.txt"

if (Test-Path $unity) {
    Write-Host "Regenerating via Unity (close other Unity instances if this fails)..."
    & $unity -batchmode -nographics -quit -projectPath $root -logFile $logFile
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Unity batch regenerate failed. Use Regenerate project files in Unity, or close Unity and rerun this script."
    }
}

function Update-CompileList {
    param(
        [string]$ProjectPath,
        [string[]]$CompilePaths,
        [string]$Pattern
    )

    $lines = $CompilePaths | Sort-Object | ForEach-Object {
        "    <Compile Include=`"$_`" />"
    }
    $itemGroup = (@("  <ItemGroup>") + $lines + "  </ItemGroup>") -join "`n"
    $content = Get-Content $ProjectPath -Raw
    $content = [regex]::Replace($content, $Pattern, $itemGroup, 1)
    Set-Content $ProjectPath $content -NoNewline
}

$runtimeScripts = Get-ChildItem (Join-Path $root "Assets\Scripts\*.cs") | ForEach-Object {
    "Assets\Scripts\$($_.Name)"
}
$runtimeScripts += "Assets\TutorialInfo\Readme.cs"

$editorScripts = Get-ChildItem (Join-Path $root "Assets\Scripts\Editor\*.cs") | ForEach-Object {
    "Assets\Scripts\Editor\$($_.Name)"
}
$editorScripts += "Assets\TutorialInfo\Editor\ReadmeEditor.cs"

Update-CompileList (Join-Path $root "Assembly-CSharp.csproj") $runtimeScripts '(?s)  <ItemGroup>\s*(?:    <Compile Include="[^"]+" />\s*)+  </ItemGroup>'
Update-CompileList (Join-Path $root "Assembly-CSharp-Editor.csproj") $editorScripts '(?s)  <ItemGroup>\s*(?:    <Compile Include="[^"]+" />\s*)+  </ItemGroup>'

if (-not (Test-Path (Join-Path $root "battle.sln"))) {
    Push-Location $root
    dotnet new sln -n battle -o . --force | Out-Null
    dotnet sln battle.sln add Assembly-CSharp.csproj Assembly-CSharp-Editor.csproj | Out-Null
    Pop-Location
}

Write-Host "Done. Reload Cursor and run: C#: Restart Language Server"
