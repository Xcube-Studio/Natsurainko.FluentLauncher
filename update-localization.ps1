$localizer = ".\FluentLauncher.LocalizationPoroject\build.bat"

# Check if localizer exists
if (-not (Test-Path $localizer)) {
    Write-Host "Localization tool not found! Check the localization project submodule."
    exit
}

# Check modification dates

# Function to get the most recent modification date in a directory
function Get-MostRecentModificationDate {
    param (
        [string]$path
    )
    Get-ChildItem -Path $path -Recurse | 
    Sort-Object LastWriteTime -Descending | 
    Select-Object -First 1 -ExpandProperty LastWriteTime
}

# Get the most recent modification dates
$latestViewsDate = Get-MostRecentModificationDate -path ".\FluentLauncher.LocalizationPoroject\Views"
$latestStringsDate = Get-MostRecentModificationDate -path ".\Natsurainko.FluentLauncher\Strings"

# Compare dates and exit if no compilation is needed
if ($latestStringsDate -ge $latestViewsDate) {
    Write-Host "Skipped generation of resw files. Translations are up-to-date."
    exit
}

# Change directory and call build.bat
Set-Location .\FluentLauncher.LocalizationPoroject
echo `n | & ".\build.bat"

# Copy resw files generated to the FluentLauncher project
$source = Join-Path (Get-Location) "Strings"
$target = Join-Path $PSScriptRoot "Natsurainko.FluentLauncher\Strings"
Copy-Item -Path $source -Destination $target -Recurse -Force

# Display message
Write-Host "COPIED $source TO $target"
