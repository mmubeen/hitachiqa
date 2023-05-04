
param (
    [string]$projectName = $(throw "Please specify the projectName."),
    [string]$dotnetFramework = "net6.0",
    [string]$targetHost = $(throw "Please specify the targetHost."),
    [string]$outputFolder = $(throw "please provide output directory"),
    [string]$assetsDir = $(throw "please provide assets directory"),
    [string]$framework = $(throw "please provide a framework options ('playwright', 'selenium')")
)

Write-Host "Project Name: $projectName"
Write-Host "Dotnet Framework: $dotnetFramework"
Write-Host "Target Host: $targetHost"
Write-Host "Output Folder: $outputFolder"
Write-Host "Framework: $framework"

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
dotnet new mstest -n $projectName -f $dotnetFramework

cd ./$projectName

dotnet sln add ./$projectName.csproj

Remove-Item "UnitTest1.cs"
Remove-Item "usings.cs"

New-Item -ItemType Directory -Path ./Features
New-Item -ItemType Directory -Path ./StepDefinitions
New-Item -ItemType Directory -Path ./Pages
New-Item -ItemType Directory -Path ./bin/Debug/$dotnetFramework/

Copy-Item "$assetsDir/assets/default.runsettings"   ./
Copy-Item "$assetsDir/assets/ImplicitUsings.cs"     ./
Copy-Item "$assetsDir/assets/Nuget.config"          ./
Copy-Item "$assetsDir/assets/specflow.json"         ./

Copy-Item "$assetsDir/assets/HsalSearch.feature"    ./Features/
if ($framework -ieq "playwright") {
    Copy-Item "$assetsDir/assets/playwright/HsalSearchSteps.cs"    ./StepDefinitions/
    Copy-Item "$assetsDir/assets/playwright/HsalHome.cs"           ./Pages/    
    Copy-Item "$assetsDir/assets/playwright/appsettings.json"           ./    

    
} else {
    Copy-Item "$assetsDir/assets/selenium/HsalSearchSteps.cs"    ./StepDefinitions/
    Copy-Item "$assetsDir/assets/selenium/HsalHome.cs"           ./Pages/    
    Copy-Item "$assetsDir/assets/selenium/appsettings.json"      ./    

}



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

#please keep formatting as is
    $fileContent = $fileContent -replace "</TargetFramework>", '</TargetFramework>
	<RunSettingsFilePath>$(MSBuildProjectDirectory)\default.runsettings</RunSettingsFilePath>'

#please keep formatting as is
    $fileContent = $fileContent -replace "</Project>", '  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>'

    $fileContent | Set-Content $fileName
}


dotnet add package SpecFlow.MsTest --version 3.9.74
dotnet add package FluentAssertions --version 6.10.0
dotnet add package HitachiQA -n --version 1.0.2
#dotnet add package HitachiQA -n --version 1.0.1-CI-20230504-020919
dotnet add package MSTest.TestAdapter --version 1.0.1
dotnet add package Microsoft.NET.Test.Sdk --version 17.3.2




cd ../
