<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="search.aspx.cs" Inherits="Loom.Organizations.Search" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Organizations</title>
    <link href="../css/theme.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div id="main">
            <h1>Organizations</h1>
            <div class="breadcrumbs"><a href="/">Home</a> / Organizations</div>
            <p>Here's a list of all the available tenant accounts:</p>
            <table>
            <asp:Repeater runat="server" ID="OrganizationRepeater">
                <ItemTemplate>
                    <tr>
                        <td><span class="color-swatch" style="background-color: <%# Eval("Color") %>;"></span></td>
                        <td><%# Eval("Name") %></td>
                        <td><a href='/<%# Eval("Slug") %>/organizations/search'>/<%# Eval("Slug") %></a></td>
                        <td><%# IsCurrentOrganization((string)Eval("Slug")) ? "<span class=\"current-marker\">✓</span>" : "" %></td>
                    </tr>
                </ItemTemplate>
            </asp:Repeater>
            </table>
        </div>
    </form>
</body>
</html>
