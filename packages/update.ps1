$ErrorActionPreference = "Stop"

$PackagesDir = "$env:BUILD_WORKSPACE_DIRECTORY\packages"

Set-Location $PackagesDir

# install packages
paket install

# generate Bazel targets for each package
bazel run @rules_dotnet//tools/paket2bazel -- `
    --dependencies-file "$PackagesDir\paket.dependencies" `
    --output-folder "$PackagesDir"
