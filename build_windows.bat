@echo off
echo Building SneedSmoother...
echo.

REM Check if dotnet is available
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK is not installed or not in PATH
    echo Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

REM Clean and build
echo Cleaning previous builds...
dotnet clean

echo Building Release version...
dotnet build --configuration Release --verbosity normal

if %errorlevel% neq 0 (
    echo.
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo.
echo SUCCESS: Build completed successfully!
echo.
echo The executable should be in: bin\Release\net8.0-windows\
echo.
pause 