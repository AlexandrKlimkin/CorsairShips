<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ClassicLeaderboardsGenerateTestUsers.aspx.cs" Inherits="Backend.Admin.ClassicLeaderboardsGenerateTestUsers" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->
<div id="generated-footer"> :: <a href="AccessControl.aspx">AccessControl</a> :: <a href="AddPromo.aspx">AddPromo</a> :: ClassicLeaderboardsGenerateTestUsers :: <a href="DeleteRecordByValue.aspx">DeleteRecordByValue</a> :: <a href="DeleteUser.aspx">DeleteUser</a> :: <a href="DownloadUserState.aspx">DownloadUserState</a> :: <a href="GlobalBan.aspx">GlobalBan</a> :: <a href="GlobalConflictScheduler.aspx">GlobalConflictScheduler</a> :: <a href="GlobalConflictStorage.aspx">GlobalConflictStorage</a> :: <a href="Index.html">Index</a> :: <a href="Leagues.aspx">Leagues</a> :: <a href="LeaguesBan.aspx">LeaguesBan</a> :: <a href="ProfileViewer.aspx">ProfileViewer</a> :: <a href="QuartzControl.aspx">QuartzControl</a> :: <a href="ReplaceWithRandomUser.aspx">ReplaceWithRandomUser</a> :: <a href="ServerUpdate.aspx">ServerUpdate</a> :: <a href="SharedLogicTest.aspx">SharedLogicTest</a> :: <a href="UploadUserState.aspx">UploadUserState</a> :: </div>
<!-- END OF AUTO GENERATED CONTENT (see crumps.py) -->
<form id="form1" runat="server">
    <table style="width: 100%;">
        <tr>
            <td>Amount of users (max == 10000 per button click)</td>
            <td>Min score</td>
            <td>Max score</td>
            <td>Type ("HonorPoints" or smth)</td>
            <td></td>
        </tr>
        <tr>
            <td><asp:TextBox ID="amount" runat="server" Text="" Width="100%">10</asp:TextBox></td>
            <td><asp:TextBox ID="min" runat="server" Text="" Width="100%">100</asp:TextBox></td>
            <td><asp:TextBox ID="max" runat="server" Text="" Width="100%">10000</asp:TextBox></td>
            <td><asp:TextBox ID="type" runat="server" Text="" Width="100%">HonorPoints</asp:TextBox></td>
        </tr>
    </table>
    <asp:Button ID="generateButton" runat="server" Text="Generate users" />
    <asp:Label ID="statusLine" runat="server" Text=""/>
</form>
</body>
</html>
