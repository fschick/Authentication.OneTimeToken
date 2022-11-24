# Ensure unsigned powershell script execution ist allowed: Set-ExecutionPolicy -ExecutionPolicy RemoteSigned

param (
  [Parameter()][String]$version
)

. $PSScriptRoot/_core.ps1

Push-Location $PSScriptRoot/../..

# Configure
if (!$version) {
	$version = git describe --tags
}

# Build
Build-Project -project FS.Authentication.OneTimeToken -version $version

Pop-Location