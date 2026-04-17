@echo off
setlocal EnableExtensions EnableDelayedExpansion

set "SCRIPT_DIR=%~dp0"
set "BASE_URL=http://localhost:5050"
set "STARTED_BY_SCRIPT=0"
set "TMP_DIR=%TEMP%\owasp-api-demo"
set "BODY_FILE=%TMP_DIR%\response-body.txt"
set "HEADERS_FILE=%TMP_DIR%\response-headers.txt"
set "STATUS_FILE=%TMP_DIR%\response-status.txt"
set "DEMO_KEY=%~1"

if "%DEMO_KEY%"=="" set "DEMO_KEY=full"

call :resolve_demo "%DEMO_KEY%"
if errorlevel 1 exit /b 1

if not exist "%TMP_DIR%" mkdir "%TMP_DIR%" >nul 2>&1

call :is_healthy
if errorlevel 1 (
    call "%SCRIPT_DIR%start-demo-server.cmd"
    if errorlevel 1 exit /b 1
    set "STARTED_BY_SCRIPT=1"
)

echo.
echo ============================================================
echo   !DEMO_BANNER!
echo ============================================================
echo Base URL : %BASE_URL%
echo.

if /i "%DEMO_KEY%"=="full" (
    call :section "0. Home"
    call :run_get_json "Home" "%BASE_URL%/"
)

if /i "%DEMO_KEY%"=="misconfig" call :run_misconfiguration_demo
if /i "%DEMO_KEY%"=="auth" call :run_authentication_demo
if /i "%DEMO_KEY%"=="injection" call :run_injection_demo
if /i "%DEMO_KEY%"=="access" call :run_access_control_demo
if /i "%DEMO_KEY%"=="full" (
    call :run_misconfiguration_demo
    call :run_authentication_demo
    call :run_injection_demo
    call :run_access_control_demo
)

echo ============================================================
echo !DEMO_COMPLETE!
echo ============================================================

if "%STARTED_BY_SCRIPT%"=="1" (
    echo.
    call "%SCRIPT_DIR%stop-demo-server.cmd"
)

exit /b 0

:resolve_demo
set "DEMO_BANNER="
set "DEMO_COMPLETE="

if /i "%~1"=="misconfig" (
    set "DEMO_BANNER=OWASP API Security Demo - A05 Security Misconfiguration"
    set "DEMO_COMPLETE=Misconfiguration demo completed."
    exit /b 0
)

if /i "%~1"=="auth" (
    set "DEMO_BANNER=OWASP API Security Demo - A02 Authentication"
    set "DEMO_COMPLETE=Authentication demo completed."
    exit /b 0
)

if /i "%~1"=="injection" (
    set "DEMO_BANNER=OWASP API Security Demo - A03 Injection"
    set "DEMO_COMPLETE=Injection demo completed."
    exit /b 0
)

if /i "%~1"=="access" (
    set "DEMO_BANNER=OWASP API Security Demo - A01 Broken Access Control"
    set "DEMO_COMPLETE=Access control demo completed."
    exit /b 0
)

if /i "%~1"=="full" (
    set "DEMO_BANNER=OWASP API Security Demo - End-to-End CLI Walkthrough"
    set "DEMO_COMPLETE=Demo walkthrough completed."
    exit /b 0
)

echo Unknown demo section: %~1
echo Valid sections: misconfig, auth, injection, access, full
exit /b 1

:run_misconfiguration_demo
call :section "1. A05 Security Misconfiguration"
call :run_get_json "Verbose error leak" "%BASE_URL%/api/vulnerable/misconfig/error"
call :run_get_headers "Vulnerable response headers" "%BASE_URL%/api/vulnerable/misconfig/headers"
call :run_get_headers "Secure response headers" "%BASE_URL%/api/secure/misconfig/headers"
exit /b 0

:run_authentication_demo
call :section "2. A02 Authentication"
set "VULN_LOGIN_URL=%BASE_URL%/api/vulnerable/auth/login?username=alice&password=alice123"
call :run_post_empty_json "Vulnerable login with credentials in URL" "!VULN_LOGIN_URL!"

set "ALICE_LOGIN_BODY=%TMP_DIR%\alice-login.json"
> "%ALICE_LOGIN_BODY%" echo {"username":"alice","password":"alice123"}
call :run_post_json "Secure login with JSON body" "%BASE_URL%/api/secure/auth/login" "%ALICE_LOGIN_BODY%"
call :extract_token "%BODY_FILE%" ALICE_TOKEN
call :run_get_json_auth "Secure profile lookup" "%BASE_URL%/api/secure/auth/me" "alice-token" "!ALICE_TOKEN!"
exit /b 0

:run_injection_demo
call :section "3. A03 Injection"
call :run_get_json_encoded "SQL injection attempt" "%BASE_URL%/api/vulnerable/injection/sql-users" "search=1 OR 1=1--"
call :run_get_json "Parameterized SQL lookup" "%BASE_URL%/api/secure/injection/sql-users?id=1"

set "NOSQL_VULN_BODY=%TMP_DIR%\nosql-vulnerable.json"
> "%NOSQL_VULN_BODY%" echo {"username":{"$gt":""},"password":{"$gt":""}}
call :run_post_json "NoSQL operator injection" "%BASE_URL%/api/vulnerable/injection/nosql-login" "%NOSQL_VULN_BODY%"

set "NOSQL_SAFE_BODY=%TMP_DIR%\nosql-secure.json"
> "%NOSQL_SAFE_BODY%" echo {"username":"alice","password":"alice123"}
call :run_post_json "Typed NoSQL login" "%BASE_URL%/api/secure/injection/nosql-login" "%NOSQL_SAFE_BODY%"

call :run_get_json "Unsafe command construction" "%BASE_URL%/api/vulnerable/injection/command-preview?fileName=invoice.pdf;whoami"
call :run_get_json "Allow-listed command input" "%BASE_URL%/api/secure/injection/command-preview?fileName=invoice.pdf"
exit /b 0

:run_access_control_demo
call :section "4. A01 Broken Access Control"
set "ALICE_LOGIN_BODY=%TMP_DIR%\alice-login.json"
> "%ALICE_LOGIN_BODY%" echo {"username":"alice","password":"alice123"}
call :run_post_json "Secure login for Alice token" "%BASE_URL%/api/secure/auth/login" "%ALICE_LOGIN_BODY%"
call :extract_token "%BODY_FILE%" ALICE_TOKEN

call :run_get_json "Vulnerable order lookup" "%BASE_URL%/api/vulnerable/access/orders/2"
call :run_get_json_auth "Secure order lookup with Alice token" "%BASE_URL%/api/secure/access/orders/2" "alice-token" "!ALICE_TOKEN!"

set "ADMIN_LOGIN_BODY=%TMP_DIR%\admin-login.json"
> "%ADMIN_LOGIN_BODY%" echo {"username":"admin","password":"admin123"}
call :run_post_json "Secure admin login" "%BASE_URL%/api/secure/auth/login" "%ADMIN_LOGIN_BODY%"
call :extract_token "%BODY_FILE%" ADMIN_TOKEN
call :run_get_json "Vulnerable admin export" "%BASE_URL%/api/vulnerable/access/admin/export"
call :run_get_json_auth "Secure admin export" "%BASE_URL%/api/secure/access/admin/export" "admin-token" "!ADMIN_TOKEN!"
exit /b 0

:section
echo ------------------------------------------------------------
echo %~1
echo ------------------------------------------------------------
exit /b 0

:run_get_json
call :print_request "%~1" "GET" "%~2" "" ""
curl.exe -sS -o "%BODY_FILE%" -w "%%{http_code}" "%~2" > "%STATUS_FILE%"
call :print_json_response
exit /b %ERRORLEVEL%

:run_get_json_encoded
call :print_request "%~1" "GET" "%~2?%~3" "" ""
curl.exe -sS -G --data-urlencode "%~3" -o "%BODY_FILE%" -w "%%{http_code}" "%~2" > "%STATUS_FILE%"
call :print_json_response
exit /b %ERRORLEVEL%

:run_post_empty_json
call :print_request "%~1" "POST" "%~2" "[empty body]" ""
curl.exe -sS -X POST --data "" -o "%BODY_FILE%" -w "%%{http_code}" "%~2" > "%STATUS_FILE%"
call :print_json_response
exit /b %ERRORLEVEL%

:run_post_json
call :print_request "%~1" "POST" "%~2" "%~3" ""
curl.exe -sS -X POST -H "Content-Type: application/json" --data "@%~3" -o "%BODY_FILE%" -w "%%{http_code}" "%~2" > "%STATUS_FILE%"
call :print_json_response
exit /b %ERRORLEVEL%

:run_get_json_auth
call :print_request "%~1" "GET" "%~2" "" "Bearer %~3"
curl.exe -sS -H "Authorization: Bearer %~4" -o "%BODY_FILE%" -w "%%{http_code}" "%~2" > "%STATUS_FILE%"
call :print_json_response
exit /b %ERRORLEVEL%

:run_get_headers
call :print_request "%~1" "GET" "%~2" "" ""
curl.exe -sS -D "%HEADERS_FILE%" -o "%BODY_FILE%" -w "%%{http_code}" "%~2" > "%STATUS_FILE%"
call :print_header_response
exit /b %ERRORLEVEL%

:print_request
set "REQ_NAME=%~1"
set "REQ_METHOD=%~2"
set "REQ_URL=%~3"
set "REQ_BODY=%~4"
set "REQ_AUTH=%~5"
echo Request
echo(  Name   : !REQ_NAME!
echo(  Method : !REQ_METHOD!
echo(  URL    : !REQ_URL!
if not "!REQ_BODY!"=="" echo(  Body   : !REQ_BODY!
if not "!REQ_AUTH!"=="" echo(  Auth   : !REQ_AUTH!
echo.
exit /b 0

:print_json_response
set "STATUS="
set /p STATUS=<"%STATUS_FILE%"
echo.
echo Response
echo   Status : %STATUS%
echo   Body   :
powershell -NoProfile -Command "$raw = Get-Content -Raw -LiteralPath '%BODY_FILE%'; try { $obj = $raw | ConvertFrom-Json; $obj | ConvertTo-Json -Depth 10 } catch { if ([string]::IsNullOrWhiteSpace($raw)) { '' } else { $raw.TrimEnd() } }"
echo.
exit /b 0

:print_header_response
set "STATUS="
set /p STATUS=<"%STATUS_FILE%"
echo.
echo Response
echo   Status  : %STATUS%
echo   Headers :
type "%HEADERS_FILE%"
echo   Body    :
powershell -NoProfile -Command "$raw = Get-Content -Raw -LiteralPath '%BODY_FILE%'; try { $obj = $raw | ConvertFrom-Json; $obj | ConvertTo-Json -Depth 10 } catch { if ([string]::IsNullOrWhiteSpace($raw)) { '' } else { $raw.TrimEnd() } }"
echo.
exit /b 0

:extract_token
set "%~2="
for /f "usebackq delims=" %%T in (`powershell -NoProfile -Command "(Get-Content -Raw -LiteralPath '%~1' | ConvertFrom-Json).token"`) do (
    set "%~2=%%T"
)

if not defined %~2 (
    echo Failed to extract token from %~1
    exit /b 1
)

exit /b 0

:is_healthy
curl.exe -sS -o nul "%BASE_URL%/"
if errorlevel 1 exit /b 1
exit /b 0
