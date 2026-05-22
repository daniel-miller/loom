# Loom

Loom is a prototype that demonstrates **path-based multitenancy** for ASP.NET Web Forms on .NET Framework 4.8. It follows the GitHub URL convention: the first segment of every URL is either a **tenant slug** or a **reserved app-scope path**, and a shared reserved-word list disambiguates the two.

```
/red                       → tenant "red" home
/red/about                 → tenant "red" about page
/red/organizations/search  → tenant "red" organization list
/about                     → app-scope about (no tenant context)
/healthz                   → app-scope health probe
/login                     → reserved (page not implemented; 404)
```

## Why path-based multitenancy?

| Approach | Example | Tradeoffs |
|----------|---------|-----------|
| **Subdomain** | `acme.example.com` | Wildcard DNS + SSL. Complex local development. Cookie sharing across stacks is painful. |
| **Path segment** | `example.com/acme` | Single domain. Standard SSL. Easy local development. |
| **Query string** | `example.com?tenant=acme` | Fragile, easily lost. Not recommended. |

Loom uses **path segment**. Legacy subdomain URLs are automatically redirected to the canonical path form so old links keep working during migration.

## HTTP request flow

```
Browser requests: /red/about
        |
        v
+----------------------------------------------------------+
|  IIS URL Rewrite (Web.config)                            |
|                                                          |
|  1. RedirectToHttps: skip when X-Forwarded-Proto=https   |
|     or host is localhost                                 |
|  2. TenantStaticAssetRewrite: pass static files through  |
|     (e.g. /red/css/theme.css → css/theme.css)            |
|  3. ReservedSlugs rewriteMap excludes /about, /login,    |
|     /healthz, etc. from tenant rewriting                 |
|  4. OrganizationPathRewrite: captures "red" as slug,     |
|     rewrites to tenants/about, sets ORGANIZATION_SLUG    |
+----------------------------------------------------------+
        |
        v
+----------------------------------------------------------+
|  Application_BeginRequest (Global.asax.cs)               |
|                                                          |
|  OrganizationResolver.Resolve(context)                   |
|   - reads ORGANIZATION_SLUG from server variables        |
|   - validates against OrganizationCache + reserved list  |
|   - snapshots settings, stores both slug and settings    |
|     in HttpContext.Items                                 |
+----------------------------------------------------------+
        |
        v
+----------------------------------------------------------+
|  Page Execution: tenants/about.aspx                      |
|                                                          |
|  TenantPage base class instantiates WebOrganizationContext|
|  in OnInit; the page reads OrgContext.Settings           |
+----------------------------------------------------------+
        |
        v
+----------------------------------------------------------+
|  Application_PostRequestHandlerExecute (Global.asax.cs)  |
|                                                          |
|  Attaches OrganizationUrlResponseFilter when the slug    |
|  is a real tenant and the response is HTML. The filter   |
|  prefixes root-relative href/src/action URLs with the    |
|  tenant slug as bytes stream out.                        |
+----------------------------------------------------------+
```

## Repository layout

```
src/
  default.aspx                         app-scope landing (no tenant)
  about.aspx                           app-scope about (no tenant)
  context-missing.aspx                 error page when no slug found
  context-invalid.aspx                 error page when slug unknown
  Global.asax(.cs)                     pipeline wiring
  Web.config                           rewrite rules + customErrors + HSTS
  Loom.csproj                          legacy Web Application project
  state/
    IOrganizationContext.cs            tenant context abstraction
    OrganizationSettings.cs            immutable per-tenant data
    OrganizationCache.cs               snapshot-replaceable in-memory cache
    OrganizationUrlResponseFilter.cs   streaming HTML URL rewriter
  web/
    OrganizationResolver.cs            request-time tenant resolution
    WebOrganizationContext.cs          IOrganizationContext implementation
    OrganizationUrl.cs                 URL builder for code-behind
    OrganizationHtml.cs                safe HTML rendering helpers
    RouteConfiguration.cs              FriendlyUrls registration
  tenants/
    TenantPage.cs                      base class for tenant-scoped pages
    home.aspx                          tenant home (was Default.aspx)
    about.aspx                         tenant about page
    organizations/search.aspx          tenant list

tests/
  Loom.Tests/                          xUnit + Moq test project
```

## Key components

| Component | Responsibility |
|-----------|----------------|
| `Web.config` rewrite rules | First-line routing. Redirects HTTP→HTTPS, strips trailing slashes, passes static assets through, excludes reserved paths, captures the tenant slug into a server variable, rewrites to the matching file under `tenants/`. |
| `OrganizationResolver` | Runs in `Application_BeginRequest`. Validates the slug, checks the reserved list, snapshots `OrganizationSettings` from the cache, stores both in `HttpContext.Items`. Also handles legacy-subdomain redirects and validates startup configuration. |
| `OrganizationCache` | Static in-memory store with atomic `Reload()`. Holds the `ReservedSlugs` list, enforces the canonical slug format, and raises a `Reloaded` event so derived state (e.g. the response filter regex) can rebuild. |
| `WebOrganizationContext` | Reads slug and settings from `HttpContext.Items`. Throws if the resolver did not run — never touches the cache directly, so a concurrent reload cannot affect an in-flight request. |
| `TenantPage` | Base class for tenant-scoped `.aspx` pages. Wires the context in `OnInit` and exposes `protected IOrganizationContext OrgContext` so derived pages avoid the boilerplate. |
| `OrganizationUrl` | Builds tenant-prefixed URLs in code-behind. Either reads the current slug from the request context or accepts an explicit slug. |
| `OrganizationHtml` | Safe rendering helpers. HTML-encodes user-supplied names and whitelists colors against a regex (CSS keyword or hex). |
| `OrganizationUrlResponseFilter` | Streaming write-only `Stream`. Prefixes root-relative `href`/`src`/`action` URLs with the tenant slug, splitting at `>` boundaries so attribute matches are never severed. Honors `Response.ContentEncoding` and rebuilds its regex when the cache reloads. |

## Reserved paths (GitHub-style convention)

The first URL segment is either a tenant slug or a reserved name. `OrganizationCache.ReservedSlugs` enumerates the reserved names. The same names appear in `<rewriteMap name="ReservedSlugs">` in `Web.config`; on startup, `OrganizationResolver.EnsureConfigured` reads the rewriteMap from disk and throws if the two lists have drifted.

Categories of reserved names:

- Built-in pages: `default`, `about`, `organizations`, `context-missing`, `context-invalid`
- Authentication / account: `login`, `logout`, `signin`, `signup`, `oauth`, `sso`, `sessions`, `account`, …
- User-scope: `dashboard`, `settings`, `profile`, `notifications`, `messages`, `users`
- App features: `admin`, `search`, `explore`, `new`, `edit`, `trending`, `system`
- Marketing: `blog`, `pricing`, `features`, `legal`, `terms`, `privacy`, `support`, …
- Operational: `api`, `healthz`, `metrics`, `ping`, `status`
- Static asset roots: `css`, `js`, `img`, `images`, `fonts`, `assets`, `media`, `static`, `public`, `themes`, `downloads`
- Crawler files: `favicon.ico`, `robots.txt`, `sitemap.xml`

Adding a reserved name requires updating both `OrganizationCache.ReservedSlugs` and the `<rewriteMap>` in `Web.config`. The startup drift check enforces this; an app pool refusing to boot is the symptom.

## Canonical slug format

Defined once in `OrganizationCache.SlugPatternSource`:

```
[a-z0-9](?:[a-z0-9-]{0,37}[a-z0-9])?
```

Lowercase alphanumeric and hyphens, 1–39 characters, no leading or trailing hyphen. The `LegacySubdomainPattern` embeds the same string so subdomain matching and tenant lookup agree. `IsValidSlugFormat(string)` exposes the check for onboarding flows.

URL matching against the dictionary is case-insensitive: `/RED`, `/Red`, and `/red` all resolve to the same tenant.

## Static files

Tenant pages can use root-relative paths like `/css/theme.css`. The response filter prefixes them with the tenant slug, producing `/red/css/theme.css` in the rendered HTML. The IIS `TenantStaticAssetRewrite` rule then strips the tenant prefix and serves the file from disk. The trip is invisible to the browser and to the page author.

## Error handling

Two error pages handle tenant resolution failures:

- `context-missing.aspx` — no tenant slug found in the URL
- `context-invalid.aspx` — slug not recognized (shows the requested value)

`Application_Error` in `Global.asax.cs` logs unhandled exceptions via `System.Diagnostics.Trace.TraceError` and lets ASP.NET's configured `customErrors` page render the response.

## Accessing the current tenant context

Tenant-scoped pages inherit `TenantPage`:

```csharp
public partial class MyPage : TenantPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var tenantName = OrgContext.Settings.Name;
        var tenantSlug = OrgContext.Slug;
        // ...
    }
}
```

App-scope pages inherit `Page` directly. They have no tenant context; reading `OrgContext.Settings` would throw, which is intentional — app-scope pages must not pretend to be tenant-aware.

To generate a tenant-aware URL in code:

```csharp
// Reads the slug from the current request context
var url = OrganizationUrl.Resolve(new HttpContextWrapper(Context), "~/reports");

// Force a specific tenant
var url = OrganizationUrl.Resolve(null, "~/reports", "blue");
```

## Cache reload

`OrganizationCache.Reload()` rebuilds the snapshot atomically with `Interlocked.Exchange` and fires `Reloaded`. Subscribers are invoked individually inside try/catch so one throwing handler cannot break the others. The response filter subscribes from its static constructor and rebuilds its slug-aware regex on each reload.

Production deployments should replace the seed data in `Load()` with a database read and call `Reload()` on a schedule or via an admin endpoint.

## HTTPS and security headers

- `RedirectToHttps` URL Rewrite rule redirects HTTP to HTTPS unless the request is from `localhost`/`127.0.0.1` or a TLS-terminating proxy reports `X-Forwarded-Proto: https`.
- HSTS header (`Strict-Transport-Security: max-age=31536000; includeSubDomains`) on every response.
- `<customErrors mode="RemoteOnly" />` suppresses stack-trace pages for remote clients.

## IIS Express configuration

The `ORGANIZATION_SLUG` server variable must be on the IIS allowed list before URL Rewrite will set it. Locally:

1. Open `.vs\loom\config\applicationhost.config` (the `.vs` folder is hidden).
2. Inside `<system.webServer><rewrite>`, add:

```xml
<allowedServerVariables>
    <add name="ORGANIZATION_SLUG" />
</allowedServerVariables>
```

3. Restart IIS Express (exit from system tray or kill `iisexpress.exe`).

Without this, requests fail with `HTTP Error 500.50 - URL Rewrite Module Error. The server variable "ORGANIZATION_SLUG" is not allowed to be set.`

## Production IIS configuration

Allow the server variable at the server level:

```
appcmd.exe set config -section:system.webServer/rewrite/allowedServerVariables /+"[name='ORGANIZATION_SLUG']" /commit:apphost
```

Or edit `%windir%\system32\inetsrv\config\applicationHost.config` directly.

## Build and test

The main project (`src/Loom.csproj`) is a legacy ASP.NET Web Application and requires Visual Studio's MSBuild — `dotnet build` does not work because `Microsoft.WebApplication.targets` is not in the .NET SDK. Build and run from Visual Studio 2022, or from a Developer Command Prompt:

```
msbuild loom.sln /t:Restore;Build
vstest.console.exe tests\Loom.Tests\bin\Debug\net48\Loom.Tests.dll
```

The test project (`tests/Loom.Tests`) uses xUnit + Moq and covers `OrganizationCache`, `OrganizationResolver`, `OrganizationUrl`, `OrganizationHtml`, `OrganizationSettings`, and `OrganizationUrlResponseFilter`.

## Known gaps and follow-ups

- Logging is via `System.Diagnostics.Trace`. Swap in Serilog/NLog before production by introducing a logger interface and replacing the call sites.
- No correlation/request ID stamped in `Application_BeginRequest`. Add for multi-tenant log aggregation.
- The reserved list lives in two files (`OrganizationCache.ReservedSlugs` + `<rewriteMap>` in `Web.config`). Startup drift detection catches divergence; consider generating one from the other.
- `OrganizationResolver.Resolve` is a long method with six branches. Extract per-branch helpers when the routing logic grows further.
- The tenant home page (`tenants/home.aspx`) still contains a demo subdomain link gated by `Loom.Demo.SubdomainOrganizationSlug`. Move or remove for a clean reference template.
