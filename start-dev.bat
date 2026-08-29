@echo off
setlocal
cd /d %~dp0
where dotnet >nul 2>nul || (echo .NET 8 SDK is required. & pause & exit /b 1)
start "Pars API" cmd /k "dotnet run --project Pars.API\Pars.API.csproj"
timeout /t 3 >nul
start "Pars Web" cmd /k "dotnet run --project Pars.Web\Pars.Web.csproj"
echo Pars System v1.0 started in development mode.
endlocal
