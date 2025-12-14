# Loom

Loom is a prototype to demonstrate path-based multitenancy for ASP.NET Web Forms applications running on .NET Framework 4.8. 

The first segment of every URL identifies the tenant (organization), and all application logic operates within that tenant's context.

## Why path-based multitenancy?

Multitenant applications serve multiple customers (tenants) from a single codebase. There are three common approaches to identifying tenants:

| Approach | Example | Tradeoffs |
|----------|---------|-----------|
| **Subdomain** | `acme.example.com` | Requires wildcard DNS and SSL certificates. More complex infrastructure configuration and release management. Harder to develop locally. |
| **Path segment** | `example.com/acme` | Single domain, standard SSL. Works easily in local development. |
| **Query string** | `example.com?tenant=acme` | Fragile, easily lost. Not recommended. |

This prototype uses the **path segment** approach because it offers the best balance of simplicity, security, and developer experience.

> **Note:** Historically, we have always used the subdomain approach for multitenant software architecture. This prototype demonstrates how an ASP.NET Web Forms application can be migrated to a path approach. This is worthwhile to consider, because it is very difficult to share authentication cookies between technology platforms such as React and ASP.NET Web Forms when one uses `localhost` and the other uses a different subdomain, such as `local-abc.example.com`.

Loom also shows how incoming requests on a subdomain are automatically rerouted to use the correct path, with the subdomain removed.

## HTTP request flow

Here's what happens when a browser requests `/orange/about`:

```
Browser requests: /orange/about
        |
        v
+----------------------------------------------------------+
|  IIS URL Rewrite Module                                  |
|                                                          |
|  1. Match: ^([^/]+)/(.*)$ captures "orange" and "about"  |
|  2. Set server variable: ORGANIZATION_SLUG = "orange"    |
|  3. Rewrite URL to: /about                               |
+----------------------------------------------------------+
        |
        v
+----------------------------------------------------------+
|  ASP.NET Pipeline: Application_BeginRequest              |
|                                                          |
|  1. OrganizationResolver reads ORGANIZATION_SLUG         |
|  2. Validates "orange" exists in OrganizationCache       |
|  3. Stores slug in HttpContext.Current.Items             |
+----------------------------------------------------------+
        |
        v
+----------------------------------------------------------+
|  Page Execution: About.aspx                              |
|                                                          |
|  1. HttpOrganizationContext reads from Items             |
|  2. Page renders with tenant-specific data               |
+----------------------------------------------------------+
        |
        v
+----------------------------------------------------------+
|  Response Filter: OrganizationUrlResponseFilter          |
|                                                          |
|  1. Scans HTML for href, src, and action attributes      |
|  2. Rewrites root-relative URLs to include tenant        |
|  3. Sends modified HTML to browser                       |
+----------------------------------------------------------+
```

## Key components

| Component | Responsibility |
|-----------|----------------|
| `Web.config` (rewrite rules) | Extracts the tenant slug from the URL path before ASP.NET sees the request. Stores it in a server variable and rewrites the URL to remove the tenant prefix. |
| `OrganizationResolver` | Reads the server variable, validates the tenant exists, and stores it in `HttpContext.Current.Items` for the duration of the request. If the requested tenant does not exist (or the tenant cannot be determined for the request) then the client is redirected to an error page. |
| `HttpOrganizationContext` | Provides a clean interface for pages to access the current tenant. Implements `IOrganizationContext` for testability. |
| `OrganizationCache` | In-memory cache of valid tenants and their settings. In production, this would load from a database on application start. |
| `OrganizationUrl` | Helper class for generating tenant-prefixed URLs in code-behind. Use `OrganizationUrl.Resolve("~/Page")` to generate correct paths. |
| `OrganizationUrlResponseFilter` | A response filter that automatically rewrites root-relative URLs in HTML output. This allows pages to use simple paths like `href="/about"` without manually prefixing the tenant. Long-term, all such "unresolved" paths should be removed from the UI, and then this filter can be removed. |

## URL rewrite rules in IIS

Three rules in `Web.config` handle URL processing:

### 1. RemoveTrailingSlash

Canonicalizes URLs by redirecting `/orange/` to `/orange`. This prevents duplicate content and ensures consistent URLs throughout the application.

### 2. OrganizationRootRewrite

Handles requests to the tenant root, like `/orange`. Rewrites to `Default.aspx` while capturing the slug.

```xml
<rule name="OrganizationRootRewrite" stopProcessing="false">
    <match url="^([^/]+)/?$" />
    <conditions>
        <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
        <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
        <add input="{R:1}" pattern="\.(ico|png|jpg|...)$" negate="true" />
    </conditions>
    <action type="Rewrite" url="Default" />
    <serverVariables>
        <set name="ORGANIZATION_SLUG" value="{R:1}" />
    </serverVariables>
</rule>
```

### 3. OrganizationPathRewrite

Handles all other tenant requests, like `/orange/about`. Strips the tenant prefix and rewrites to the actual page path.

```xml
<rule name="OrganizationPathRewrite" stopProcessing="false">
    <match url="^([^/]+)/(.*)$" />
    <conditions>
        <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
        <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
        <add input="{R:1}" pattern="^(ContextMissing|ContextInvalid)$" negate="true" />
    </conditions>
    <action type="Rewrite" url="{R:2}" />
    <serverVariables>
        <set name="ORGANIZATION_SLUG" value="{R:1}" />
    </serverVariables>
</rule>
```

> **Note:** The `ORGANIZATION_SLUG` server variable must be added to IIS's allowed server variables list. This is configured at the server level, not in `Web.config`. See the [IIS Configuration](#iis-configuration) section below.

## HTTP response filter

The `OrganizationUrlResponseFilter` intercepts HTML responses and rewrites root-relative URLs to include the tenant prefix. This allows developers to write simple markup without worrying about tenant paths:

```html
<!-- What you write -->
<a href="/about">About</a>
<img src="/images/logo.png" />

<!-- What the browser receives (for tenant "orange") -->
<a href="/orange/about">About</a>
<img src="/orange/images/logo.png" />
```

The filter uses a regex to match `href`, `src`, and `action` attributes with root-relative paths, skipping URLs that are already prefixed or are protocol-relative.

Most important, this achieves backward-compatibility by eliminating the need to find and replace all unresolved URLs in the existing UI. Unresolved URLs can be updated over time, and the application can run with both tenant-resolved and tenant-unresolved URLs.

> **Limitation:** The response filter only rewrites HTML attributes. JavaScript code that constructs URLs dynamically must use the tenant prefix explicitly. Consider exposing the current tenant slug to JavaScript via a data attribute or global variable.

## Accessing the current tenant context

In page code-behind, use `HttpOrganizationContext`:

```csharp
public partial class MyPage : Page
{
    private IOrganizationContext _orgContext = new HttpOrganizationContext();

    protected void Page_Load(object sender, EventArgs e)
    {
        var tenantName = _orgContext.Settings.Name;
        var tenantSlug = _orgContext.Slug;
        
        // Use tenant information...
    }
}
```

To generate tenant-aware URLs in code:

```csharp
// Returns "/orange/reports" (assuming current tenant is "orange")
var url = OrganizationUrl.Resolve("~/reports");

// Force a specific tenant
var url = OrganizationUrl.Resolve("~/reports", "blue");
```

## Static files

Static files (CSS, JavaScript, images) in directories like `/css` and `/img` are served directly by IIS without passing through the rewrite rules. This is because the rewrite conditions check `{REQUEST_FILENAME}` and skip actual files.

For tenant-specific static files, you have two options:

1. Store them in a path that includes the tenant: `/public/orange/logo.png`
2. Serve them dynamically through an HTTP handler that reads from tenant-specific storage

## Error handling

Two error pages handle tenant resolution failures:

- **context-missing.aspx** - Shown when no tenant slug is present in the URL
- **context-invalid.aspx** - Shown when the tenant slug doesn't match any known tenant

These pages are excluded from the rewrite rules and operate under the special `empty` tenant context.

## IIS Express configuration

When running locally in IIS Express, you need to allow the `ORGANIZATION_SLUG` server variable for URL Rewrite to work.

1. Open `.vs\{SolutionName}\config\applicationhost.config` (the `.vs` folder is hidden)

2. Find the `<rewrite>` section inside `<system.webServer>`

3. Add the `<allowedServerVariables>` block:

```xml
<rewrite>
    <allowedServerVariables>
        <add name="ORGANIZATION_SLUG" />
    </allowedServerVariables>
    <!-- existing rules -->
</rewrite>
```

4. Restart IIS Express completely (exit from system tray or kill `iisexpress.exe` in Task Manager)

Without this change, you'll see: `HTTP Error 500.50 - URL Rewrite Module Error. The server variable "ORGANIZATION_SLUG" is not allowed to be set.`

## IIS Configuration

When running in IIS, you need to allow the server variable at the server level. Use IIS Manager or run:

```
appcmd.exe set config -section:system.webServer/rewrite/allowedServerVariables /+"[name='ORGANIZATION_SLUG']" /commit:apphost
```

Alternatively, edit `%windir%\system32\inetsrv\config\applicationHost.config` directly.
