@echo off
setlocal ENABLEEXTENSIONS ENABLEDELAYEDEXPANSION

echo [🔍] Mencari rc.exe versi x64...

set "RC_PATH="

:: Cari rc.exe di folder x64 saja
for /f "delims=" %%R in ('where /R "C:\Program Files (x86)\Windows Kits" rc.exe ^| findstr /i "\\x64\\rc.exe"') do (
    set "RC_PATH=%%R"
    goto :found
)

echo [❌] Gagal menemukan rc.exe versi x64. Pastikan Windows SDK terinstall.
exit /b 1

:found
echo [✅] Ditemukan rc.exe: %RC_PATH%
echo.

:: Pastikan app.rc ada
if not exist "app.rc" (
    echo [❌] File app.rc tidak ditemukan.
    exit /b 1
)

:: Compile app.rc ke app.res
echo [⚙️ ] Menjalankan: "%RC_PATH%" app.rc
"%RC_PATH%" app.rc
if errorlevel 1 (
    echo [❌] Gagal compile app.rc → app.res
    exit /b 1
)

echo [✅] Sukses: app.res dibuat
echo.

:: Jalankan publish
echo [🚀] Publish aplikasi...
dotnet publish -c Release -r win-x64 --self-contained true ^
  /p:PublishTrimmed=false /p:PublishSingleFile=true ^
  /p:IncludeNativeLibrariesForSelfExtract=true

if errorlevel 1 (
    echo [❌] Gagal publish aplikasi.
    exit /b 1
)

:: Tampilkan hasil
set "OUT_DIR=bin\Release\net7.0\win-x64\publish"
if exist "%OUT_DIR%\ProcessMemoryViewer.exe" (
    echo.
    echo [✅] Publish berhasil: %OUT_DIR%\ProcessMemoryViewer.exe
    explorer "%OUT_DIR%"
) else (
    echo [❌] File EXE tidak ditemukan!
)

exit /b 0
