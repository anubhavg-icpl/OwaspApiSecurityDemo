using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace OwaspApiSecurityDemo.App.Controllers
{
    public sealed class BrowserDemoController : ApiController
    {
        [HttpGet]
        [Route("browser")]
        public HttpResponseMessage Index()
        {
            const string html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <title>OWASP API Security Browser Demo</title>
    <style>
        :root {
            color-scheme: dark;
            --bg: #0f172a;
            --panel: #111827;
            --panel-alt: #1f2937;
            --border: #334155;
            --text: #e5e7eb;
            --muted: #94a3b8;
            --accent: #38bdf8;
            --danger: #f87171;
            --ok: #4ade80;
            --warn: #fbbf24;
        }

        * { box-sizing: border-box; }

        body {
            margin: 0;
            font-family: Segoe UI, Arial, sans-serif;
            background: linear-gradient(180deg, #020617 0%, #0f172a 100%);
            color: var(--text);
        }

        .page {
            max-width: 1200px;
            margin: 0 auto;
            padding: 24px;
        }

        h1, h2, h3, p { margin-top: 0; }

        .hero, .panel, .result {
            background: rgba(17, 24, 39, 0.95);
            border: 1px solid var(--border);
            border-radius: 14px;
            box-shadow: 0 12px 30px rgba(0, 0, 0, 0.25);
        }

        .hero {
            padding: 24px;
            margin-bottom: 20px;
        }

        .hero p, .note, .mini {
            color: var(--muted);
        }

        .grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
            gap: 16px;
            margin-bottom: 20px;
        }

        .checklist-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
            gap: 16px;
            margin-bottom: 20px;
        }

        .panel {
            padding: 18px;
        }

        .panel h2 {
            font-size: 1.05rem;
            margin-bottom: 10px;
        }

        .panel-actions {
            display: flex;
            flex-wrap: wrap;
            gap: 8px;
            margin-top: 12px;
        }

        button, a.button-link {
            border: 0;
            border-radius: 10px;
            padding: 10px 12px;
            background: #2563eb;
            color: white;
            font-weight: 600;
            cursor: pointer;
            text-decoration: none;
            display: inline-block;
        }

        button.alt, a.alt {
            background: var(--panel-alt);
            border: 1px solid var(--border);
        }

        button.warn {
            background: #7c2d12;
        }

        button.ok {
            background: #166534;
        }

        .result {
            padding: 18px;
        }

        .status-row {
            display: flex;
            gap: 12px;
            flex-wrap: wrap;
            margin-bottom: 12px;
        }

        .badge {
            padding: 6px 10px;
            border-radius: 999px;
            font-size: 0.85rem;
            border: 1px solid var(--border);
            background: #0b1220;
        }

        .ok-text { color: var(--ok); }
        .warn-text { color: var(--warn); }
        .danger-text { color: var(--danger); }

        pre {
            white-space: pre-wrap;
            word-break: break-word;
            background: #020617;
            border: 1px solid #1e293b;
            border-radius: 12px;
            padding: 14px;
            margin: 0;
            overflow: auto;
        }

        .token-row {
            display: flex;
            flex-wrap: wrap;
            gap: 8px;
            margin-top: 10px;
        }

        ol {
            margin: 0;
            padding-left: 20px;
        }

        li + li {
            margin-top: 8px;
        }
    </style>
</head>
<body>
    <div class=""page"">
        <section class=""hero"">
            <h1>OWASP API Security Browser Demo</h1>
            <p>Use this page during the presentation to run the vulnerable and secure examples directly in the browser and show the live JSON result.</p>
            <div class=""token-row"">
                <button class=""ok"" onclick=""loginSecure('alice')"">Get Alice token</button>
                <button class=""ok"" onclick=""loginSecure('admin')"">Get Admin token</button>
                <a class=""button-link alt"" href=""/"" target=""_blank"" rel=""noreferrer"">Open API home JSON</a>
            </div>
            <p class=""mini"">Short forms: <strong>BOLA</strong> = Broken Object Level Authorization, <strong>JWT</strong> = JSON Web Token, <strong>CSP</strong> = Content Security Policy, <strong>HSTS</strong> = HTTP Strict Transport Security.</p>
        </section>

        <section class=""checklist-grid"">
            <article class=""panel"">
                <h2>30 second checklist</h2>
                <ol>
                    <li>Click <strong>Vulnerable headers</strong>.</li>
                    <li>Click <strong>Secure headers</strong>.</li>
                    <li>Say: same feature, safer defaults.</li>
                </ol>
            </article>

            <article class=""panel"">
                <h2>60 second checklist</h2>
                <ol>
                    <li>Click <strong>Login in URL</strong>.</li>
                    <li>Click <strong>Secure login</strong>.</li>
                    <li>Click <strong>BOLA order lookup</strong>.</li>
                    <li>Click <strong>Secure order lookup</strong>.</li>
                </ol>
            </article>

            <article class=""panel"">
                <h2>90 second checklist</h2>
                <ol>
                    <li>Click <strong>Verbose error</strong>.</li>
                    <li>Click <strong>Login in URL</strong>.</li>
                    <li>Click <strong>SQL injection</strong>.</li>
                    <li>Click <strong>BOLA order lookup</strong>.</li>
                    <li>Then click the secure pair for each one.</li>
                </ol>
            </article>
        </section>

        <section class=""grid"">
            <article class=""panel"">
                <h2>A05 Security Misconfiguration</h2>
                <p class=""note"">Show leaked errors, missing headers, and the safer equivalents.</p>
                <div class=""panel-actions"">
                    <button class=""warn"" onclick=""runExample('misconfig-error-vuln')"">Verbose error</button>
                    <button class=""ok"" onclick=""runExample('misconfig-error-secure')"">Safe error</button>
                    <button class=""warn"" onclick=""runExample('misconfig-headers-vuln')"">Vulnerable headers</button>
                    <button class=""ok"" onclick=""runExample('misconfig-headers-secure')"">Secure headers</button>
                    <button class=""warn"" onclick=""runExample('misconfig-default-vuln')"">Default credentials</button>
                    <button class=""ok"" onclick=""runExample('misconfig-default-secure')"">Policy fix</button>
                </div>
            </article>

            <article class=""panel"">
                <h2>A02 Authentication</h2>
                <p class=""note"">Compare credentials in the URL with signed, short-lived tokens.</p>
                <div class=""panel-actions"">
                    <button class=""warn"" onclick=""runExample('auth-login-vuln')"">Login in URL</button>
                    <button class=""warn"" onclick=""runExample('auth-transport-vuln')"">Credential transport leak</button>
                    <button class=""ok"" onclick=""runExample('auth-login-secure')"">Secure login</button>
                    <button class=""ok"" onclick=""runExample('auth-me-secure')"">Secure profile</button>
                </div>
            </article>

            <article class=""panel"">
                <h2>A03 Injection</h2>
                <p class=""note"">Use the preset payloads to show SQL, NoSQL, and command injection behavior.</p>
                <div class=""panel-actions"">
                    <button class=""warn"" onclick=""runExample('inj-sql-vuln')"">SQL injection</button>
                    <button class=""ok"" onclick=""runExample('inj-sql-secure')"">Parameterized SQL</button>
                    <button class=""warn"" onclick=""runExample('inj-nosql-vuln')"">NoSQL operator injection</button>
                    <button class=""ok"" onclick=""runExample('inj-nosql-secure')"">Typed NoSQL login</button>
                    <button class=""warn"" onclick=""runExample('inj-command-vuln')"">Unsafe command</button>
                    <button class=""ok"" onclick=""runExample('inj-command-secure')"">Allow-listed command</button>
                </div>
            </article>

            <article class=""panel"">
                <h2>A01 Broken Access Control</h2>
                <p class=""note"">Show BOLA and admin-only authorization with Alice and Admin tokens.</p>
                <div class=""panel-actions"">
                    <button class=""warn"" onclick=""runExample('access-orders-vuln')"">BOLA order lookup</button>
                    <button class=""ok"" onclick=""runExample('access-orders-secure')"">Secure order lookup</button>
                    <button class=""warn"" onclick=""runExample('access-export-vuln')"">Open admin export</button>
                    <button class=""ok"" onclick=""runExample('access-export-secure')"">Protected admin export</button>
                </div>
            </article>
        </section>

        <section class=""result"">
            <h2>Live result</h2>
            <div class=""status-row"">
                <div id=""result-name"" class=""badge"">No request yet</div>
                <div id=""result-status"" class=""badge"">Status: -</div>
            </div>
            <h3>Request</h3>
            <pre id=""request-box"">Click a demo button.</pre>
            <h3>Response headers</h3>
            <pre id=""headers-box"">-</pre>
            <h3>Response body</h3>
            <pre id=""body-box"">-</pre>
        </section>
    </div>

    <script>
        const tokens = {};

        const examples = {
            'misconfig-error-vuln': { name: 'Verbose error leak', method: 'GET', url: '/api/vulnerable/misconfig/error' },
            'misconfig-error-secure': { name: 'Safe generic error', method: 'GET', url: '/api/secure/misconfig/error' },
            'misconfig-headers-vuln': { name: 'Vulnerable response headers', method: 'GET', url: '/api/vulnerable/misconfig/headers' },
            'misconfig-headers-secure': { name: 'Secure response headers', method: 'GET', url: '/api/secure/misconfig/headers' },
            'misconfig-default-vuln': { name: 'Default credentials exposed', method: 'GET', url: '/api/vulnerable/misconfig/default-credentials' },
            'misconfig-default-secure': { name: 'Default credentials prohibited', method: 'GET', url: '/api/secure/misconfig/default-credentials' },

            'auth-login-vuln': { name: 'Vulnerable login with URL credentials', method: 'POST', url: '/api/vulnerable/auth/login?username=alice&password=alice123' },
            'auth-transport-vuln': { name: 'Credentials in URL transport', method: 'GET', url: '/api/vulnerable/auth/credential-transport?username=alice&password=alice123' },
            'auth-login-secure': { name: 'Secure login', method: 'POST', url: '/api/secure/auth/login', body: { username: 'alice', password: 'alice123' } },
            'auth-me-secure': { name: 'Secure profile with bearer token', method: 'GET', url: '/api/secure/auth/me', token: 'alice', ensureLogin: 'alice' },

            'inj-sql-vuln': { name: 'SQL injection attempt', method: 'GET', url: '/api/vulnerable/injection/sql-users?search=1%20OR%201=1--' },
            'inj-sql-secure': { name: 'Parameterized SQL lookup', method: 'GET', url: '/api/secure/injection/sql-users?id=1' },
            'inj-nosql-vuln': { name: 'NoSQL operator injection', method: 'POST', url: '/api/vulnerable/injection/nosql-login', rawBody: '{""username"":{""$gt"":""""},""password"":{""$gt"":""""}}' },
            'inj-nosql-secure': { name: 'Typed NoSQL login', method: 'POST', url: '/api/secure/injection/nosql-login', body: { username: 'alice', password: 'alice123' } },
            'inj-command-vuln': { name: 'Unsafe command preview', method: 'GET', url: '/api/vulnerable/injection/command-preview?fileName=invoice.pdf;whoami' },
            'inj-command-secure': { name: 'Allow-listed command preview', method: 'GET', url: '/api/secure/injection/command-preview?fileName=invoice.pdf' },

            'access-orders-vuln': { name: 'BOLA order lookup', method: 'GET', url: '/api/vulnerable/access/orders/2' },
            'access-orders-secure': { name: 'Secure order lookup with Alice token', method: 'GET', url: '/api/secure/access/orders/2', token: 'alice', ensureLogin: 'alice' },
            'access-export-vuln': { name: 'Vulnerable admin export', method: 'GET', url: '/api/vulnerable/access/admin/export' },
            'access-export-secure': { name: 'Secure admin export', method: 'GET', url: '/api/secure/access/admin/export', token: 'admin', ensureLogin: 'admin' }
        };

        async function loginSecure(kind) {
            const creds = kind === 'admin'
                ? { username: 'admin', password: 'admin123' }
                : { username: 'alice', password: 'alice123' };

            const response = await fetch('/api/secure/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(creds)
            });

            const payload = await parseBody(response);
            if (response.ok && payload && payload.token) {
                tokens[kind] = payload.token;
            }

            renderResult(kind === 'admin' ? 'Secure admin login' : 'Secure Alice login', {
                method: 'POST',
                url: '/api/secure/auth/login',
                headers: { 'Content-Type': 'application/json' },
                body: creds
            }, response, payload);
        }

        async function runExample(key) {
            const example = examples[key];
            if (!example) {
                return;
            }

            if (example.ensureLogin) {
                await ensureLogin(example.ensureLogin);
            }

            const headers = {};
            let body = undefined;

            if (example.body) {
                headers['Content-Type'] = 'application/json';
                body = JSON.stringify(example.body);
            } else if (example.rawBody) {
                headers['Content-Type'] = 'application/json';
                body = example.rawBody;
            }

            if (example.token && tokens[example.token]) {
                headers['Authorization'] = 'Bearer ' + tokens[example.token];
            }

            const response = await fetch(example.url, {
                method: example.method,
                headers,
                body
            });

            const payload = await parseBody(response);
            renderResult(example.name, {
                method: example.method,
                url: example.url,
                headers,
                body: example.body || example.rawBody || null
            }, response, payload);
        }

        async function ensureLogin(kind) {
            if (!tokens[kind]) {
                await loginSecure(kind);
            }
        }

        async function parseBody(response) {
            const text = await response.text();
            if (!text) {
                return '';
            }

            try {
                return JSON.parse(text);
            } catch (error) {
                return text;
            }
        }

        function renderResult(name, request, response, payload) {
            document.getElementById('result-name').textContent = name;

            const statusBox = document.getElementById('result-status');
            statusBox.textContent = 'Status: ' + response.status + ' ' + response.statusText;
            statusBox.className = 'badge ' + (response.ok ? 'ok-text' : response.status >= 500 ? 'danger-text' : 'warn-text');

            document.getElementById('request-box').textContent = JSON.stringify(request, null, 2);

            const headers = {};
            response.headers.forEach((value, key) => {
                headers[key] = value;
            });
            document.getElementById('headers-box').textContent = JSON.stringify(headers, null, 2);

            document.getElementById('body-box').textContent =
                typeof payload === 'string' ? payload : JSON.stringify(payload, null, 2);
        }
    </script>
</body>
</html>";

            return new HttpResponseMessage
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html")
            };
        }
    }
}
