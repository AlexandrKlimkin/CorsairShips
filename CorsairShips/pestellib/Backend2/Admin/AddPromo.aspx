<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AddPromo.aspx.cs" Inherits="Server.Admin.AddPromo" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->
<div id="generated-footer"> :: <a href="AccessControl.aspx">AccessControl</a> :: AddPromo :: <a href="ClassicLeaderboardsGenerateTestUsers.aspx">ClassicLeaderboardsGenerateTestUsers</a> :: <a href="DeleteRecordByValue.aspx">DeleteRecordByValue</a> :: <a href="DeleteUser.aspx">DeleteUser</a> :: <a href="DownloadUserState.aspx">DownloadUserState</a> :: <a href="GlobalBan.aspx">GlobalBan</a> :: <a href="GlobalConflictScheduler.aspx">GlobalConflictScheduler</a> :: <a href="GlobalConflictStorage.aspx">GlobalConflictStorage</a> :: <a href="Index.html">Index</a> :: <a href="Leagues.aspx">Leagues</a> :: <a href="LeaguesBan.aspx">LeaguesBan</a> :: <a href="ProfileViewer.aspx">ProfileViewer</a> :: <a href="QuartzControl.aspx">QuartzControl</a> :: <a href="ReplaceWithRandomUser.aspx">ReplaceWithRandomUser</a> :: <a href="ServerUpdate.aspx">ServerUpdate</a> :: <a href="SharedLogicTest.aspx">SharedLogicTest</a> :: <a href="UploadUserState.aspx">UploadUserState</a> :: </div>
<!-- END OF AUTO GENERATED CONTENT (see crumps.py) -->
<form id="form1" runat="server">
        <table>
            <tr>
                <td>Promo code</td>
                <td>Function</td>
                <td>Parameter</td>
                <td>Count</td>
                <td></td>
            </tr>
            <tr>
                <td><asp:TextBox ID="promoCode" runat="server" Text="XXXX-XXXX"></asp:TextBox></td>
                <td>
                    <asp:TextBox ID="function" runat="server" Width="200px"></asp:TextBox>
                </td>
                <td><asp:TextBox ID="parameter" runat="server" Text=""></asp:TextBox></td>
                <td><asp:TextBox ID="count" runat="server" Text="100"></asp:TextBox></td>
                <td><asp:Button ID="registerNewPromo" runat="server" Text="Register new promo" /></td>
            </tr>
        </table>
        <asp:Label ID="statusLine" runat="server" Text=""/>
    <div>
    
    </div>
    </form>
</body>
</html>
