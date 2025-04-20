function Read-Stable-Version{
    $manifestPath = Resolve-Path -Path "Natsurainko.FluentLauncher\Package-Stable.appxmanifest"
    [xml]$xmlContent = Get-Content -Path $manifestPath

    return $xmlContent.Package.Identity.Version
}

function Write-Preview-Version {
    param (
        [Parameter(Mandatory=$true)]
        [string]$Version
    )

    $manifestPath = Resolve-Path -Path "Natsurainko.FluentLauncher\Package-Preview.appxmanifest"
    [xml]$xmlContent = Get-Content -Path $manifestPath

    $xmlContent.Package.Identity.Version = $Version
    $xmlContent.Save($manifestPath)
}

function Get-ApiReleaseJson {
    $HttpClient = New-Object System.Net.Http.HttpClient
    $HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0")

    try {
        return $HttpClient.GetStringAsync("https://api.github.com/repos/Xcube-Studio/Natsurainko.FluentLauncher/releases").Result
    } catch {
        Write-Error "Failed to fetch releases from GitHub API: $_"
        exit 1
    }
}

function Get-BuildCountOfVersion {
    param (
        [Parameter(Mandatory=$true)]
        [string]$Version
    )

    $Releases = Get-ApiReleaseJson | ConvertFrom-Json 
        | Where-Object { $_.prerelease -eq $true } 
        | Sort-Object {[datetime]$_.published_at} -Descending

    foreach ($Release in $Releases) {
        $Pattern = '(?<=``` json)([\s\S]+?)(?=```)'
        $Match = [regex]::Match($Release.body, $Pattern)
        if (-not $Match.Success) {
            continue
        }

        $JsonBody = $Match.Groups[1].Value | ConvertFrom-Json

        if ($JsonBody.previousStableVersion -eq $Version) {
            return [int]$JsonBody.build + 1
        }
    }
}

$stableVersion = Read-Stable-Version
Write-Output "Read Current Stable Version: $stableVersion"

$buildCount = Get-BuildCountOfVersion -Version $stableVersion
Write-Output "Got Build Count Of Stable Version: $buildCount"

$versionParts = $stableVersion.Split('.')
$versionParts[-1] = [int]$buildCount
$newVersion = $versionParts -join '.'

Write-Output "New Preview Version: $newVersion"
Write-Preview-Version -Version $newVersion