@echo off
setlocal EnableExtensions

set "SCRIPT_DIR=%~dp0"
set "PID_FILE=%SCRIPT_DIR%.demo-server.pid"

if not exist "%PID_FILE%" (
    echo No tracked demo server is running.
    exit /b 0
)

set /p SERVER_PID=<"%PID_FILE%"
if "%SERVER_PID%"=="" (
    del "%PID_FILE%" >nul 2>&1
    echo No tracked demo server is running.
    exit /b 0
)

tasklist /fi "PID eq %SERVER_PID%" | find "%SERVER_PID%" >nul
if errorlevel 1 (
    del "%PID_FILE%" >nul 2>&1
    echo Demo server PID %SERVER_PID% is no longer running.
    exit /b 0
)

taskkill /PID %SERVER_PID% /T /F >nul
if errorlevel 1 (
    echo Failed to stop demo server PID %SERVER_PID%.
    exit /b 1
)

del "%PID_FILE%" >nul 2>&1
echo Demo server stopped ^(PID %SERVER_PID%^).
exit /b 0
