$localizerProject = ".\FluentLauncher.Infra.Localization\FluentLauncher.Infra.Localizer"

# Check if localizer exists
if (-not (Test-Path $localizerProject)) {
    Write-Host "Localization tool not found! Check the localization project submodule."
    exit
}

# Check modification dates
$csvFolder = ".\FluentLauncher.Infra.Localization\Views"
$reswFolder = ".\Natsurainko.FluentLauncher\Assets\Strings"

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
$latestCsvDate = Get-MostRecentModificationDate -path $csvFolder
$latestReswDate = Get-MostRecentModificationDate -path $reswFolder

# Compare dates and exit if no compilation is needed
if ($latestReswDate -ge $latestCsvDate) {
    Write-Host "Skipped generation of resw files. Translations are up-to-date."
    exit
}

# Generate .resw files if trnaslations are updated
dotnet run --project $localizerProject -- --src $csvFolder --out $reswFolder --languages en-US zh-Hans zh-Hant ru-RU uk-UA --default-language en-US
