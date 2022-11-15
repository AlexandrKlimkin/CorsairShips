<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportsViewer.aspx.cs" Inherits="Backend.CM.ReportsViewer"  Async="true"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->
<div id="generated-footer"> :: <a href="DeleteUser.aspx">DeleteUser</a> :: <a href="Feedback.aspx">Feedback</a> :: <a href="GlobalBan.aspx">GlobalBan</a> :: <a href="Index.html">Index</a> :: <a href="MessageBox.aspx">MessageBox</a> :: <a href="ProfileViewer.aspx">ProfileViewer</a> :: <a href="PurchaseItem.aspx">PurchaseItem</a> :: ReportsViewer :: </div>
<!-- END OF AUTO GENERATED CONTENT (see crumps.py) -->
<form id="form1" runat="server">
        <div>
            <asp:TextBox runat="server" ID="playerId" Columns="36" ReadOnly="False"/> 
            <asp:Button ID="loadUserReports" runat="server" Text="Load reports" OnClick="loadUserReports_OnClick"/>
            <asp:Button ID="addToWhitelist" runat="server" Text="Add to whitelist" OnClick="addToWhitelist_OnClick"/>
            <asp:Button ID="removeFromWhitelist" runat="server" Text="Remove from whitelist" OnClick="removeFromWhitelist_OnClick"/>
            <br/>
            <asp:Label ID="playerInfo" runat="server" Text=""/>
            <br/>
            <asp:Button ID="banSelected" runat="server" OnClick="banSelected_OnClick" Text="Ban selected player" Width="400" />
            <asp:Button ID="forgiveSelected" runat="server" OnClick="forgiveSelected_OnClick" Text="Forgive selected player" Width="400" />
            <asp:Button ID="viewProfile" runat="server" OnClick="viewProfile_OnClick" Text="View user profile" Width="400" />
            <br/>
            <asp:DropDownList runat="server" ID="chatBanPeriod">
                <asp:ListItem Text="3 days" Value="3" Selected="True"></asp:ListItem>
                <asp:ListItem Text="1 week" Value="7"></asp:ListItem>
                <asp:ListItem Text="1 month" Value="30"></asp:ListItem>
                <asp:ListItem Text="forever" Value="365"></asp:ListItem>
                <asp:ListItem Text="unban" Value="0"></asp:ListItem>
            </asp:DropDownList>
            <asp:Button runat="server" ID="chatBanSelected" OnClick="chatBanSelected_OnClick" Text="Chat ban"/>

            <table style="width:100%;">
                <tr>
                    <td>
                        <asp:ListBox ID="topCheaters" AutoPostBack="True" runat="server" Height="500" OnSelectedIndexChanged="ListBox1_SelectedIndexChanged" Width="600px"></asp:ListBox>
                    </td>
                    <td>
                        <div>
                            <asp:Table ID="reportsCaption" runat="server" GridLines="Both"  CellPadding="2" >
                                <asp:TableRow>
                                    <asp:TableCell Width="100px">Type</asp:TableCell>
                                    <asp:TableCell Width="200px">Description</asp:TableCell>
                                    <asp:TableCell>Sys</asp:TableCell>
                                    <asp:TableCell Width="700px">Game payload</asp:TableCell>
                                </asp:TableRow>
                            </asp:Table>
                        </div>
                        <div style="overflow:auto;height:500px" >
                        <asp:Table ID="reportsTable" runat="server" GridLines="Both"  CellPadding="2" >
                        </asp:Table>
                        </div>
                    </td>
                </tr>
            </table>
            <asp:Label ID="statusLine" runat="server" Text=""/>
        </div>
    </form>
</body>
</html>
