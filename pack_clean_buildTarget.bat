@echo off

set mypath=%cd%
echo %mypath%

rem 1. Pack the HitachiQA project
dotnet pack HitachiQA\HitachiQA.csproj

taskkill /IM chromedriver.exe /F
cd "C:\Users\macosta\source\repos\Demo\Demo"
dotnet clean Demo.csproj

rem 2. Remove version 1.0.1 of the hitachiqa package from the .nuget folder
del /Q "C:\Users\macosta\.nuget\packages\hitachiqa\1.0.1\*.*"

rem 3. Clean and build the Blackbird.AutomatedTesting.SOTests project
dotnet build Demo.csproj

echo Done!
pause
