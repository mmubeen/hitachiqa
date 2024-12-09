@echo off

set mypath=%cd%
echo %mypath%

dotnet clean
dotnet build
dotnet pack
dotnet tool install --global --add-source ./bin/Release Hsl.HitachiQA.Builder

echo Done!
pause
