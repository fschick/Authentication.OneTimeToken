# Ensure unsigned powershell script execution is allowed: Set-ExecutionPolicy -ExecutionPolicy RemoteSigned

param (
	[Parameter(Mandatory=$false)] [String] $version,
    [Parameter(Mandatory=$false)] [String] $publshFolder,
	[Parameter(Mandatory=$false)] [String] $nugetUrl, 
	[Parameter(Mandatory=$false)] [String] $apiKey
)

. $PSScriptRoot/_core.ps1

Push-Location $PSScriptRoot/../..

# Build and publish NuGet package
Publish-Nuget -version $version -publshFolder $publshFolder
Push-Nuget -publshFolder $publshFolder -serverUrl $nugetUrl -apiKey $apiKey

Pop-Location