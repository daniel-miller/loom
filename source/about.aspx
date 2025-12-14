<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="about.aspx.cs" Inherits="Loom.About" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>About</title>
    <link href="./css/theme.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div id="main">
            <h1 runat="server" id="MainHeading">About</h1>
            <div class="breadcrumbs"><a href="/">Home</a> / About</div>
            <hr />
            <p>
                Here is an image with a root-relative path. 
                Notice the URL does not need to include the organization slug.
            </p>
            <div>
                <img src="/img/avatar.png" alt="Avatar" />
                <div><code>/img/avatar.png</code></div>
            </div>
            <hr />
            <p>
                Here are some anchors with root-relative paths.
                Notice the organization slug is missing from the <strong>href</strong> attribute value. 
                The <strong>Organization Url Response Filter</strong> modifies the HTTP response automatically,
                ensuring such URLs are resolved before delivering content to the client.
            </p>
            <div>
                <code><a href="/about">/about</a></code>
            </div>
            <div>
                <code><a href="/organizations/search">/organizations/search</a></code>
            </div>
            
            <div class="note">
                <div>
                    View the HTML source to see this more clearly:
                </div>
                <div class="rounded-shadow-box" style="margin: 20px;">
                    <img src="/img/code.png" alt="Code" />
                </div>
            </div>

        </div>
    </form>
</body>
</html>
