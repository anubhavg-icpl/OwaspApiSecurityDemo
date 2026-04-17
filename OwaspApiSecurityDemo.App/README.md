# OWASP API Security Demo (.NET Framework 4.8)

This project is a simple self-hosted Web API demo for presenting OWASP API Security issues side by side with safer implementations.

## What it shows

- **A05 Security Misconfiguration**
  - Verbose error leakage vs generic error handling
  - Missing headers vs central security headers
  - Default credentials exposure vs policy-based handling
- **A02 Authentication**
  - Credentials in URL
  - Unsigned / tamperable token validation
  - Missing brute-force controls vs lockout
  - Short-lived signed token example
- **A03 Injection**
  - SQL injection via string concatenation
  - NoSQL operator injection
  - Command injection preview with unsafe concatenation
- **A01 Broken Access Control**
  - BOLA on user orders
  - Missing admin check on export

## Run

```powershell
dotnet restore .\OwaspApiSecurityDemo.App\OwaspApiSecurityDemo.App.csproj
dotnet build .\OwaspApiSecurityDemo.App\OwaspApiSecurityDemo.App.csproj
dotnet run --project .\OwaspApiSecurityDemo.App\OwaspApiSecurityDemo.App.csproj
```

Base URL: `http://localhost:5050/`

Browser demo page: `http://localhost:5050/browser`

## Tests

```powershell
dotnet test .\OwaspApiSecurityDemo.App.Tests\OwaspApiSecurityDemo.App.Tests.csproj
```

The test project includes codebase-visible unit cases for:

- secure auth lockout behavior
- vulnerable vs secure token validation
- vulnerable SQL and NoSQL injection behavior
- secure security-header handling
- broken access control vs protected access control

## Batch scripts

These are proper Windows batch scripts for a command-line demo:

```cmd
.\Scripts\start-demo-server.cmd
.\Scripts\run-end-to-end-demo.cmd
.\Scripts\run-misconfiguration-demo.cmd
.\Scripts\run-authentication-demo.cmd
.\Scripts\run-injection-demo.cmd
.\Scripts\run-access-control-demo.cmd
.\Scripts\stop-demo-server.cmd
```

`run-end-to-end-demo.cmd` is the full presenter walkthrough. The focused scripts run just one category each:

- `run-misconfiguration-demo.cmd` -> A05 Security Misconfiguration
- `run-authentication-demo.cmd` -> A02 Authentication
- `run-injection-demo.cmd` -> A03 Injection
- `run-access-control-demo.cmd` -> A01 Broken Access Control

All runner scripts:

- starts the server if it is not already running
- sends the requests in presentation order
- prints the exact CLI request/response flow
- stops the server at the end only if the script started it

Use the end-to-end script for a full dry run, or pick an individual script when you only want one demo section.

## Recommended presentation flow

For the cleanest live demo:

1. Run `.\Scripts\start-demo-server.cmd`
2. Keep the app running in the background
3. Run `.\Scripts\run-end-to-end-demo.cmd`
4. If needed, stop the server with `.\Scripts\stop-demo-server.cmd`

## Timed demo checklists

### 30 second demo

1. Open `http://localhost:5050/browser`
2. Click `Vulnerable headers`
3. Click `Secure headers`
4. Point out the missing vs centralized security headers

### 60 second demo

1. Open `http://localhost:5050/browser`
2. Click `Login in URL`
3. Click `Secure login`
4. Click `BOLA order lookup`
5. Click `Secure order lookup`

### 90 second demo

1. Open `http://localhost:5050/browser`
2. Click `Verbose error` then `Safe error`
3. Click `Login in URL` then `Secure login`
4. Click `SQL injection` then `Parameterized SQL`
5. Click `BOLA order lookup` then `Secure order lookup`

## Patch walkthrough in code

If you open the controllers during the demo, each vulnerable and secure pair now includes presenter comments that explain:

- what the vulnerable endpoint is doing wrong
- what the patch should change
- where the runnable secure version lives

Suggested code pairs to open while speaking:

- `Controllers\VulnerableMisconfigurationController.cs` -> `Controllers\SecureMisconfigurationController.cs`
- `Controllers\VulnerableAuthenticationController.cs` -> `Controllers\SecureAuthenticationController.cs`
- `Controllers\VulnerableInjectionController.cs` -> `Controllers\SecureInjectionController.cs`
- `Controllers\VulnerableAccessControlController.cs` -> `Controllers\SecureAccessControlController.cs`

## Suggested live walkthrough

### 1. Misconfiguration

```powershell
Invoke-RestMethod http://localhost:5050/api/vulnerable/misconfig/error
Invoke-WebRequest http://localhost:5050/api/vulnerable/misconfig/headers | Select-Object -ExpandProperty Headers
Invoke-WebRequest http://localhost:5050/api/secure/misconfig/headers | Select-Object -ExpandProperty Headers
```

### 2. Authentication

Vulnerable login with credentials in the URL:

```powershell
Invoke-RestMethod -Method Post "http://localhost:5050/api/vulnerable/auth/login?username=alice&password=alice123"
```

Secure login with JSON body:

```powershell
$secureLogin = Invoke-RestMethod -Method Post -Uri "http://localhost:5050/api/secure/auth/login" -ContentType "application/json" -Body '{"username":"alice","password":"alice123"}'
$secureToken = $secureLogin.token
Invoke-RestMethod -Headers @{ Authorization = "Bearer $secureToken" } "http://localhost:5050/api/secure/auth/me"
```

### 3. Injection

```powershell
Invoke-RestMethod "http://localhost:5050/api/vulnerable/injection/sql-users?search=1%20OR%201=1--"
Invoke-RestMethod "http://localhost:5050/api/secure/injection/sql-users?id=1"

Invoke-RestMethod -Method Post -Uri "http://localhost:5050/api/vulnerable/injection/nosql-login" -ContentType "application/json" -Body '{"username":{"$gt":""},"password":{"$gt":""}}'
Invoke-RestMethod -Method Post -Uri "http://localhost:5050/api/secure/injection/nosql-login" -ContentType "application/json" -Body '{"username":"alice","password":"alice123"}'

Invoke-RestMethod "http://localhost:5050/api/vulnerable/injection/command-preview?fileName=invoice.pdf;whoami"
Invoke-RestMethod "http://localhost:5050/api/secure/injection/command-preview?fileName=invoice.pdf"
```

### 4. Broken Access Control

Get a user token, then try another user's data:

```powershell
$aliceToken = (Invoke-RestMethod -Method Post -Uri "http://localhost:5050/api/secure/auth/login" -ContentType "application/json" -Body '{"username":"alice","password":"alice123"}').token
Invoke-RestMethod "http://localhost:5050/api/vulnerable/access/orders/2"
Invoke-RestMethod -Headers @{ Authorization = "Bearer $aliceToken" } "http://localhost:5050/api/secure/access/orders/2"
```

Admin export comparison:

```powershell
$adminToken = (Invoke-RestMethod -Method Post -Uri "http://localhost:5050/api/secure/auth/login" -ContentType "application/json" -Body '{"username":"admin","password":"admin123"}').token
Invoke-RestMethod "http://localhost:5050/api/vulnerable/access/admin/export"
Invoke-RestMethod -Headers @{ Authorization = "Bearer $adminToken" } "http://localhost:5050/api/secure/access/admin/export"
```

## Demo notes

- The vulnerable command example **does not execute OS commands**; it only shows the dangerous command string.
- Seed users are intentionally simple for presentation:
  - `alice / alice123`
  - `bob / bob123`
  - `admin / admin123`
- These credentials are for a local training demo only.
