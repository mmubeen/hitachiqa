@echo off

set mypath=%cd%
echo %mypath%

rem 1. Pack the HitachiQA project
dotnet pack HitachiQA\HitachiQA.csproj

taskkill /IM chromedriver.exe /F
cd "C:\Users\macosta\source\repos\Blackbird.AutomatedTesting\Blackbird.AutomatedTesting.Tests\Blackbird.AutomatedTesting.SOTests"
dotnet clean Blackbird.AutomatedTesting.SOTests.csproj

rem 2. Remove version 1.0.0 of the hitachiqa package from the .nuget folder
del /Q "C:\Users\macosta\.nuget\packages\hitachiqa\1.0.0\*.*"

rem 3. Clean and build the Blackbird.AutomatedTesting.SOTests project
dotnet build Blackbird.AutomatedTesting.SOTests.csproj

echo Done!
pause
