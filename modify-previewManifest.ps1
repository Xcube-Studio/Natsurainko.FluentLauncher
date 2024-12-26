$manifestPath = "Natsurainko.FluentLauncher\Package-Preview.appxmanifest"
$absoluteManifestPath = Resolve-Path -Path $manifestPath

[xml]$xmlContent = Get-Content -Path $manifestPath
$version = $xmlContent.Package.Identity.Version
Write-Output "Stable Identity.Version: $version"

$url = "https://github.com/Xcube-Studio/FluentLauncher.PreviewChannel.PackageInstaller/releases/download/v0.0.1/PackageInstaller-64.exe"
$installerPath = "PackageInstaller-64.exe"

$ErrorActionPreference = "Stop"
$ErrorVariable = "DownloadError"

try {
    Invoke-WebRequest -Uri $url -OutFile $installerPath -ErrorAction Stop -ErrorVariable DownloadError
    Write-Output "File downloaded successfully to $installerPath"
} catch {
    Write-Error "Download failed: $($DownloadError[0])"
    exit 1
}

$output = & .\$installerPath query --getBuildCountOfVersion $version

if ($output -match "BuildCount: (\d+)") {
    $build = $matches[1]
    Write-Output "BuildCount: $build"

    $build = [int]$build + 1

    $versionParts = $version.Split('.')
    $versionParts[-1] = $build
    $newVersion = $versionParts -join '.'

    Write-Output "New Version: $newVersion"
} else {
    Write-Error "Failed to retrieve BuildCount."
    exit 1
}

$xmlContent.Package.Identity.Version = $newVersion
$xmlContent.Save($absoluteManifestPath)

