# Ensure unsigned powershell script execution is allowed: Set-ExecutionPolicy -ExecutionPolicy RemoteSigned

param (
  [Parameter()][String]$version
)

. $PSScriptRoot/_core.ps1

Push-Location $PSScriptRoot/../..

# Configure
if (!$version) {
	$version = git describe --tags
}

# Run tests
Test-Project -project FS.Authentication.OneTimeToken.sln -version $version

Pop-Location


