@echo off

set mypath=%cd%
echo %mypath%

rem 1. Pack the HitachiQA project
dotnet build HitachiQA\HitachiQA.csproj
dotnet pack HitachiQA\HitachiQA.csproj

taskkill /IM chromedriver.exe /F
cd "C:\Users\macosta\Desktop\test folder\Test8\"
dotnet clean test8.csproj

rem 2. Remove version 1.0.1 of the hitachiqa package from the .nuget folder
del /Q "C:\Users\macosta\.nuget\packages\hitachiqa\1.0.4\*.*"

rem 3. Clean and build the project
dotnet build test8.csproj

echo Done!
pause
