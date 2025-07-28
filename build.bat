@echo off
setlocal ENABLEEXTENSIONS ENABLEDELAYEDEXPANSION

echo [🔍] find rc.exe x64...

set "RC_PATH="

:: Find rc.exe in any x64 folder x64 only
for /f "delims=" %%R in ('where /R "C:\Program Files (x86)\Windows Kits" rc.exe ^| findstr /i "\\x64\\rc.exe"') do (
    set "RC_PATH=%%R"
    goto :found
)

echo [❌] Failed to find rc.exe x64. Please make sure Windows SDK installed.
exit /b 1

:found
echo [✅] Found rc.exe: %RC_PATH%
echo.

:: make sure app.rc exists
if not exist "app.rc" (
    echo [❌] File app.rc not Found.
    exit /b 1
)

:: Compile app.rc to app.res
echo [⚙️ ] Run: "%RC_PATH%" app.rc
"%RC_PATH%" app.rc
if errorlevel 1 (
    echo [❌] Failed compile app.rc → app.res
    exit /b 1
)

echo [✅] Success: app.res created
echo.

:: run publish
echo [🚀] Publish application...
dotnet publish -c Release -r win-x64 --self-contained true ^
  /p:PublishTrimmed=false /p:PublishSingleFile=true ^
  /p:IncludeNativeLibrariesForSelfExtract=true

if errorlevel 1 (
    echo [❌] Failed to publish application.
    exit /b 1
)

:: show
set "OUT_DIR=bin\Release\net7.0\win-x64\publish"
if exist "%OUT_DIR%\ProcessMemoryViewer.exe" (
    echo.
    echo [✅] Publish successfull: %OUT_DIR%\ProcessMemoryViewer.exe
    explorer "%OUT_DIR%"
) else (
    echo [❌] File EXE NOT Found !
)

exit /b 0
