<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="Loom.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Loom</title>
    <link href="/css/theme.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div id="main">

            <nav>
                <a href="/about">About</a>
            </nav>

            <h1>Loom</h1>
            <div class="breadcrumbs">Home</div>

            <p>
                Loom is a prototype demonstrating path-based multitenancy for ASP.NET Web Forms
                applications on .NET Framework 4.8. The first segment of every URL identifies the
                tenant (organization), and all tenant-scoped logic operates within that tenant's
                context. Top-level paths that match a reserved name (such as
                <code class="inline-code">/about</code> or <code class="inline-code">/healthz</code>)
                resolve in app-scope with no tenant context, following the GitHub convention.
            </p>

            <h2>Try it</h2>

            <ul>
                <li><a href="/about">App-scope: about Loom</a></li>
                <li><a href="/red">Tenant: navigate to the Red organization</a></li>
                <li><a href="/orange">Tenant: navigate to the Orange organization</a></li>
                <li><a href="/blue">Tenant: navigate to the Blue organization</a></li>
            </ul>

        </div>
    </form>
</body>
</html>
