<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="about.aspx.cs" Inherits="Loom.About" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>About Loom</title>
    <link href="/css/theme.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div id="main">

            <nav>
                <a href="/">Home</a>
            </nav>

            <h1>About Loom</h1>
            <div class="breadcrumbs">Home / About</div>

            <p>
                Loom is a prototype that demonstrates path-based multitenancy for ASP.NET Web Forms
                running on .NET Framework 4.8. It uses the GitHub URL convention: the first URL
                segment is either a tenant slug or a reserved app-scope path.
            </p>

            <p>
                This page is the <em>app-scope</em> about page. The tenant-scoped about page lives at
                <code class="inline-code">/{slug}/about</code> and shows information about a specific
                organization.
            </p>

        </div>
    </form>
</body>
</html>
