<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="context-invalid.aspx.cs" Inherits="Loom.ContextInvalid" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Invalid Context</title>
    <link href="./css/theme.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>Invalid organization context</h1>
            <p>Sorry, there is no tenant account registered for <%: InvalidOrganizationSlug %></p>
        </div>
    </form>
</body>
</html>
