
param (
    [string]$projectName = $(throw "Please specify the projectName."),
    [string]$targetFramework = "net6.0",
    [string]$targetHost = $(throw "Please specify the targetHost."),
    [string]$outputFolder = $(throw "please provide output directory"),
    [string]$assetsDir = $(throw "please provide assets directory")
)

Write-Host "Project Name: $projectName"
Write-Host "Target Framework: $targetFramework"
Write-Host "Target Host: $targetHost"
Write-Host "Output Folder: $outputFolder"

Set-Location -Path $outputFolder
#
# Check if specflow template exists
#
$templateName = "MSTest Test Project"
$templateExists = $false

# Get the list of templates and search for the SpecFlow template
$templates = dotnet new --list
if ($templates -match "$templateName") {
    $templateExists = $true
}

# Fail the script if the template does not exist
if (!$templateExists) {
    Write-Error "The mstest template does not exist."
    exit 1
}

dotnet new sln -n $projectName -o ./$projectName
dotnet new mstest -n $projectName -f $targetFramework

cd ./$projectName

dotnet sln add ./$projectName.csproj

Remove-Item "UnitTest1.cs"
Remove-Item "usings.cs"

New-Item -ItemType Directory -Path ./Features
New-Item -ItemType Directory -Path ./StepDefinitions
New-Item -ItemType Directory -Path ./Pages

Copy-Item "$assetsDir/assets/default.runsettings"   ./
Copy-Item "$assetsDir/assets/ImplicitUsings.cs"     ./
Copy-Item "$assetsDir/assets/Nuget.config"          ./
Copy-Item "$assetsDir/assets/specflow.json"         ./

Copy-Item "$assetsDir/assets/HsalSearch.feature"    ./Features/
Copy-Item "$assetsDir/assets/HsalSearchSteps.cs"    ./StepDefinitions/
Copy-Item "$assetsDir/assets/HsalHome.cs"           ./Pages/


$fileNames = @()

$filenames+= "./StepDefinitions/HsalSearchSteps.cs"
$filenames+= "./Pages/HsalHome.cs"
$filenames+= "./default.runsettings"
$filenames+= "./$projectName.csproj"

foreach($fileName in $fileNames)
{
    $fileContent = Get-Content $fileName
    $fileContent = $fileContent -replace "{{ProjectName}}", $projectName
    $fileContent = $fileContent -replace "{{TargetHost}}", $targetHost
    $fileContent = $fileContent -replace "</TargetFramework>", '</TargetFramework>
	<RunSettingsFilePath>$(MSBuildProjectDirectory)\default.runsettings</RunSettingsFilePath>'

    $fileContent | Set-Content $fileName
}


dotnet add package SpecFlow.MsTest --version 3.9.74
dotnet add package FluentAssertions --version 6.10.0
dotnet add package HitachiQA -n --version 1.0.1
dotnet add package MSTest.TestAdapter --version 1.0.1
dotnet add package Microsoft.NET.Test.Sdk --version 17.3.2

cd ../
