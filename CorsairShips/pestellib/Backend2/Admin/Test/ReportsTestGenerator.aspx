<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportsTestGenerator.aspx.cs" Inherits="Backend.Admin.Test.ReportsTestGenerator" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <table >
                <tr>
                    <td>Amount of reported players</td>
                    <td><asp:TextBox ID="reportedPlayers" runat="server" Text="">50</asp:TextBox></td>
                </tr>
                <tr>
                    <td>Min reports</td>
                    <td><asp:TextBox ID="minReports" runat="server" Text="">1</asp:TextBox></td>
                </tr>
                <tr>
                    <td>Max reports</td>
                    <td><asp:TextBox ID="maxReports" runat="server" Text="">150</asp:TextBox></td>
                </tr>
                <tr>
                    <td>System reports percent</td>
                    <td><asp:TextBox ID="systemReportsPercent" runat="server" Text="">25</asp:TextBox></td>
                </tr>
                <tr>
                    <td>Min sessions</td>
                    <td><asp:TextBox ID="minSession" runat="server" Text="">0</asp:TextBox></td>
                </tr>
                <tr>
                    <td>Max sessions</td>
                    <td><asp:TextBox ID="maxSession" runat="server" Text="">50</asp:TextBox></td>
                </tr>
            </table>
            <asp:Button ID="generateButton" runat="server" Text="Generate reports" Width="300px" OnClick="generateButton_OnClick"/>
            <br/>
            <asp:Label ID="statusLine" runat="server" Text=""/>
        </div>
    </form>
</body>
</html>
