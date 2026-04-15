@echo off
setlocal EnableExtensions EnableDelayedExpansion

set "SCRIPT_DIR=%~dp0"
set "APP_DIR=%SCRIPT_DIR%.."
for %%I in ("%APP_DIR%") do set "APP_DIR=%%~fI"
set "PROJECT=%APP_DIR%\OwaspApiSecurityDemo.App.csproj"
set "BASE_URL=http://localhost:5050/"
set "WINDOW_TITLE=OWASP API Security Demo Server"
set "PID_FILE=%SCRIPT_DIR%.demo-server.pid"
set "LOG_FILE=%SCRIPT_DIR%.demo-server.log"

call :is_healthy
if not errorlevel 1 (
    echo Demo server is already running at %BASE_URL%
    exit /b 0
)

echo Building demo project...
dotnet build "%PROJECT%" -v minimal >nul
if errorlevel 1 (
    echo Build failed.
    exit /b 1
)

echo Starting demo server...
start "%WINDOW_TITLE%" /MIN cmd /c call "%SCRIPT_DIR%server-host.cmd" "%APP_DIR%" "%PROJECT%" "%LOG_FILE%"

call :wait_for_health
if errorlevel 1 (
    echo Demo server did not become ready.
    if exist "%LOG_FILE%" (
        echo --- Server log ---
        type "%LOG_FILE%"
    )
    exit /b 1
)

call :capture_pid
if defined SERVER_PID (
    > "%PID_FILE%" echo !SERVER_PID!
    echo Demo server started on %BASE_URL% ^(PID !SERVER_PID!^)
) else (
    echo Demo server started on %BASE_URL%
)

exit /b 0

:is_healthy
curl.exe -s -o nul "%BASE_URL%"
if errorlevel 1 exit /b 1
exit /b 0

:wait_for_health
for /l %%N in (1,1,40) do (
    call :is_healthy
    if not errorlevel 1 exit /b 0
    >nul timeout /t 1
)
exit /b 1

:capture_pid
set "SERVER_PID="
for /f "usebackq tokens=2 delims=," %%P in (`tasklist /v /fo csv ^| findstr /i /c:"%WINDOW_TITLE%"`) do (
    set "SERVER_PID=%%~P"
    goto :eof
)
goto :eof
