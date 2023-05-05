@echo off

set mypath=%cd%
echo %mypath%

rem 1. Pack the HitachiQA project
dotnet pack HitachiQA\HitachiQA.csproj

taskkill /IM chromedriver.exe /F
cd "C:\Users\macosta\Desktop\test folder\test\"
dotnet clean test.csproj

rem 2. Remove version 1.0.1 of the hitachiqa package from the .nuget folder
del /Q "C:\Users\macosta\.nuget\packages\hitachiqa\1.0.3\*.*"

rem 3. Clean and build the Blackbird.AutomatedTesting.SOTests project
dotnet build test.csproj

echo Done!
pause
