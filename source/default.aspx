<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="Loom.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Home</title>
    <link href="./css/theme.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div id="main">
            
            <nav>
                <a href="/organizations/search">Organizations</a>
                <a href="/about">About</a>
            </nav>

            <h1 runat="server" id="MainHeading">Welcome!</h1>
            <div class="breadcrumbs">Home</div>

            <p>
                This prototype demonstrates <strong>path-based multitenancy</strong> for ASP.NET Web Forms 
                applications running on .NET Framework 4.8. The first segment of every URL identifies the 
                tenant (organization), and all application logic operates within that tenant's context.
            </p>

            <h2>Why Path-Based Multitenancy?</h2>
            
            <p>
                Multitenant applications serve multiple customers (tenants) from a single codebase. 
                There are three common approaches to identifying tenants:
            </p>
            
            <table class="components">
                <tr>
                    <th style="width: 25%;">Approach</th>
                    <th style="width: 35%;">Example</th>
                    <th style="width: 40%;">Tradeoffs</th>
                </tr>
                <tr>
                    <td><strong>Subdomain</strong></td>
                    <td><code class="inline-code">acme.example.com</code></td>
                    <td>Requires wildcard DNS and SSL certificates. More complex infrastructure configuration and release management. Harder to develop locally.</td>
                </tr>
                <tr>
                    <td><strong>Path segment</strong></td>
                    <td><code class="inline-code">example.com/acme</code></td>
                    <td>Single domain, standard SSL. Works easily in local development.</td>
                </tr>
                <tr>
                    <td><strong>Query string</strong></td>
                    <td><code class="inline-code">example.com?tenant=acme</code></td>
                    <td>Fragile, easily lost. Not recommended.</td>
                </tr>
            </table>
            
            <p>
                This prototype uses the <strong>path segment</strong> approach because it offers the best 
                balance of simplicity, security, and developer experience.
            </p>

            <div class="note">
                <strong>Note:</strong> Historically, we have always used the subdomain approach for multitenant software
                architecture. This prototype demonstrates how an ASP.NET Web Forms application can be migrated to a path 
                approach. This is worthwhile to consider, because it is very difficult to share authentication cookies 
                between technology platforms such as React and ASP.NET Web Forms when one uses 
                <code class="inline-code">localhost</code> 
                and the other uses a different subdomain, such as 
                <code class="inline-code">local-abc.example.com</code>.
            </div>

            <p>
                This document explains how the prototype works, so you can see exactly how path segmentation is used to 
                resolve the organization context for an HTTP request. 
            </p>

            <p>
                Loom also shows how incoming requests on a subdomain are automatically rerouted to use the correct path,
                with the subdomain removed. To see this, you'll need the following IIS configuration:
            </p>

            <div class="rounded-shadow-box" style="margin: 20px;">
                <img src="/img/iis.png" alt="IIS" />
            </div>

            <h2>Request Flow</h2>
            
            <p>Here's what happens when a browser requests <code class="inline-code">/orange/about</code>:</p>
            
            <div class="diagram">Browser requests: /orange/about
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
+----------------------------------------------------------+</div>

            <h2>Key Components</h2>
            
            <table class="components">
                <tr>
                    <th style="width: 30%;">Component</th>
                    <th style="width: 70%;">Responsibility</th>
                </tr>
                <tr>
                    <td><code class="inline-code">Web.config</code><br/>(rewrite rules)</td>
                    <td>
                        Extracts the tenant slug from the URL path before ASP.NET sees the request. 
                        Stores it in a server variable and rewrites the URL to remove the tenant prefix.
                    </td>
                </tr>
                <tr>
                    <td><code class="inline-code">OrganizationResolver</code></td>
                    <td>
                        Reads the server variable, validates the tenant exists, and stores it in 
                        <code class="inline-code">HttpContext.Current.Items</code> for the duration of the request. 
                        If the requested tenant does not exist (or the tenant cannot be determined for the request) then
                        the client is redirected to an error page.
                    </td>
                </tr>
                <tr>
                    <td><code class="inline-code">HttpOrganizationContext</code></td>
                    <td>
                        Provides a clean interface for pages to access the current tenant.
                        Implements <code class="inline-code">IOrganizationContext</code> for testability.
                    </td>
                </tr>
                <tr>
                    <td><code class="inline-code">OrganizationCache</code></td>
                    <td>
                        In-memory cache of valid tenants and their settings. In production, this would 
                        load from a database on application start.
                    </td>
                </tr>
                <tr>
                    <td><code class="inline-code">OrganizationUrl</code></td>
                    <td>
                        Helper class for generating tenant-prefixed URLs in code-behind. 
                        Use <code class="inline-code">OrganizationUrl.Resolve("~/Page")</code> to generate correct paths.
                    </td>
                </tr>
                <tr>
                    <td><code class="inline-code">OrganizationUrlResponseFilter</code></td>
                    <td>
                        A response filter that automatically rewrites root-relative URLs in HTML output. 
                        This allows pages to use simple paths like <code class="inline-code">href="/about"</code> 
                        without manually prefixing the tenant. Long-term, all such "unresolved" paths should be removed
                        from the UI, and then this filter can be removed.
                    </td>
                </tr>
            </table>

            <h2>URL Rewrite Rules</h2>
            
            <p>Three rules in <code class="inline-code">Web.config</code> handle URL processing:</p>
            
            <h3>1. RemoveTrailingSlash</h3>
            <p>
                Canonicalizes URLs by redirecting <code class="inline-code">/orange/</code> to 
                <code class="inline-code">/orange</code>. This prevents duplicate content and ensures 
                consistent URLs throughout the application.
            </p>
            
            <h3>2. OrganizationRootRewrite</h3>
            <p>
                Handles requests to the tenant root, like <code class="inline-code">/orange</code>. 
                Rewrites to <code class="inline-code">Default.aspx</code> while capturing the slug.
            </p>
<pre><code>&lt;rule name="OrganizationRootRewrite" stopProcessing="false"&gt;
    &lt;match url="^([^/]+)/?$" /&gt;
    &lt;conditions&gt;
        &lt;add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" /&gt;
        &lt;add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" /&gt;
        &lt;add input="{R:1}" pattern="\.(ico|png|jpg|...)$" negate="true" /&gt;
    &lt;/conditions&gt;
    &lt;action type="Rewrite" url="Default" /&gt;
    &lt;serverVariables&gt;
        &lt;set name="ORGANIZATION_SLUG" value="{R:1}" /&gt;
    &lt;/serverVariables&gt;
&lt;/rule&gt;</code></pre>
            
            <h3>3. OrganizationPathRewrite</h3>
            <p>
                Handles all other tenant requests, like <code class="inline-code">/orange/about</code>. 
                Strips the tenant prefix and rewrites to the actual page path.
            </p>
<pre><code>&lt;rule name="OrganizationPathRewrite" stopProcessing="false"&gt;
    &lt;match url="^([^/]+)/(.*)$" /&gt;
    &lt;conditions&gt;
        &lt;add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" /&gt;
        &lt;add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" /&gt;
        &lt;add input="{R:1}" pattern="^(ContextMissing|ContextInvalid)$" negate="true" /&gt;
    &lt;/conditions&gt;
    &lt;action type="Rewrite" url="{R:2}" /&gt;
    &lt;serverVariables&gt;
        &lt;set name="ORGANIZATION_SLUG" value="{R:1}" /&gt;
    &lt;/serverVariables&gt;
&lt;/rule&gt;</code></pre>

            <div class="note">
                <strong>Note:</strong> The <code class="inline-code">ORGANIZATION_SLUG</code> server variable 
                must be added to IIS's allowed server variables list. This is configured at the server level, 
                not in <code class="inline-code">Web.config</code>. Use IIS Manager or run:<br/><br/>
                <code class="inline-code">appcmd.exe set config -section:system.webServer/rewrite/allowedServerVariables /+"[name='ORGANIZATION_SLUG']" /commit:apphost</code>
            </div>

            <h2>Response Filter</h2>
            
            <p>
                The <code class="inline-code">OrganizationUrlResponseFilter</code> intercepts HTML responses 
                and rewrites root-relative URLs to include the tenant prefix. This allows developers to write 
                simple markup without worrying about tenant paths:
            </p>
            
<pre><code>&lt;!-- What you write --&gt;
&lt;a href="/about"&gt;About&lt;/a&gt;
&lt;img src="/images/logo.png" /&gt;

&lt;!-- What the browser receives (for tenant "orange") --&gt;
&lt;a href="/orange/about"&gt;About&lt;/a&gt;
&lt;img src="/orange/images/logo.png" /&gt;</code></pre>

            <p>
                The filter uses a regex to match <code class="inline-code">href</code>, 
                <code class="inline-code">src</code>, and <code class="inline-code">action</code> attributes 
                with root-relative paths, skipping URLs that are already prefixed or are protocol-relative.
            </p>
            
            <p>
                Most important, this achieves backward-compatibility by eliminating the need to find and replace all 
                unresolved URLs in the existing UI. Unresolved URLs can be updated over time, and the application can
                run with both tenant-resolved and tenant-unresolved URLs.
            </p>

            <div class="warning">
                <strong>Limitation:</strong> The response filter only rewrites HTML attributes. JavaScript code 
                that constructs URLs dynamically must use the tenant prefix explicitly. Consider exposing the 
                current tenant slug to JavaScript via a data attribute or global variable.
            </div>

            <h2>Accessing Tenant Context</h2>
            
            <p>In page code-behind, use <code class="inline-code">HttpOrganizationContext</code>:</p>
            
<pre><code>public partial class MyPage : Page
{
    private IOrganizationContext _orgContext = new HttpOrganizationContext();

    protected void Page_Load(object sender, EventArgs e)
    {
        var tenantName = _orgContext.Settings.Name;
        var tenantSlug = _orgContext.Slug;
        
        // Use tenant information...
    }
}</code></pre>

            <p>To generate tenant-aware URLs in code:</p>
            
<pre><code>// Returns "/orange/reports" (assuming current tenant is "orange")
var url = OrganizationUrl.Resolve("~/reports");

// Force a specific tenant
var url = OrganizationUrl.Resolve("~/reports", "blue");</code></pre>

            <h2>Static Files</h2>
            
            <p>
                Static files (CSS, JavaScript, images) in directories like <code class="inline-code">/css</code> 
                and <code class="inline-code">/img</code> are served directly by IIS without passing through 
                the rewrite rules. This is because the rewrite conditions check 
                <code class="inline-code">{REQUEST_FILENAME}</code> and skip actual files.
            </p>
            
            <p>
                For tenant-specific static files, you have two options:
            </p>
            
            <ol>
                <li>Store them in a path that includes the tenant: <code class="inline-code">/public/orange/logo.png</code></li>
                <li>Serve them dynamically through an HTTP handler that reads from tenant-specific storage</li>
            </ol>

            <h2>Error Handling</h2>
            
            <p>Two error pages handle tenant resolution failures:</p>
            
            <ul>
                <li><strong>context-missing.aspx</strong> - Shown when no tenant slug is present in the URL</li>
                <li><strong>context-invalid.aspx</strong> - Shown when the tenant slug doesn't match any known tenant</li>
            </ul>
            
            <p>
                These pages are excluded from the rewrite rules and operate under the special 
                <code class="inline-code">empty</code> tenant context.
            </p>

            <h2>Try It Out</h2>
            
            <p>Navigate to different tenant contexts:</p>
            
            <ul>
                <li><a href="/red">Red Tenant</a></li>
                <li><a href="/orange">Orange Tenant</a></li>
                <li><a href="/blue">Blue Tenant</a></li>
                <li><a href="/organizations/search">View the list of all organizations</a></li>
            </ul>
            
            <p>Test error handling:</p>
            
            <ul>
                <li><a href="/nonexistent" target="_blank">/nonexistent</a> - Invalid tenant</li>
            </ul>

        </div>
    </form>
</body>
</html>
