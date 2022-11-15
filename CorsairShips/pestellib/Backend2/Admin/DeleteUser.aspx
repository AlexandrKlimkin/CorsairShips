<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="../CM/DeleteUser.aspx.cs" Inherits="Server.Admin.DeleteUser" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->
<div id="generated-footer"> :: <a href="AccessControl.aspx">AccessControl</a> :: <a href="AddPromo.aspx">AddPromo</a> :: <a href="ClassicLeaderboardsGenerateTestUsers.aspx">ClassicLeaderboardsGenerateTestUsers</a> :: <a href="DeleteRecordByValue.aspx">DeleteRecordByValue</a> :: DeleteUser :: <a href="DownloadUserState.aspx">DownloadUserState</a> :: <a href="GlobalBan.aspx">GlobalBan</a> :: <a href="GlobalConflictScheduler.aspx">GlobalConflictScheduler</a> :: <a href="GlobalConflictStorage.aspx">GlobalConflictStorage</a> :: <a href="Index.html">Index</a> :: <a href="Leagues.aspx">Leagues</a> :: <a href="LeaguesBan.aspx">LeaguesBan</a> :: <a href="ProfileViewer.aspx">ProfileViewer</a> :: <a href="QuartzControl.aspx">QuartzControl</a> :: <a href="ReplaceWithRandomUser.aspx">ReplaceWithRandomUser</a> :: <a href="ServerUpdate.aspx">ServerUpdate</a> :: <a href="SharedLogicTest.aspx">SharedLogicTest</a> :: <a href="UploadUserState.aspx">UploadUserState</a> :: </div>
<!-- END OF AUTO GENERATED CONTENT (see crumps.py) -->
<form id="form1" runat="server">
    <table style="width: 100%;">
        <tr>
            <td>GUID</td>
            <td>Google play id</td>
            <td>Device id</td>
            <td></td>
        </tr>
        <tr>
            <td><asp:TextBox ID="guid" runat="server" Text="" Width="100%"></asp:TextBox></td>
            <td><asp:TextBox ID="googlePlayId" runat="server" Text="" Width="100%"></asp:TextBox></td>
            <td><asp:TextBox ID="deviceId" runat="server" Text="" Width="100%"></asp:TextBox></td>
        </tr>
        </table>
    <asp:Button ID="deleteButton" runat="server" Text="Delete user profile" />
    <asp:Label ID="statusLine" runat="server" Text=""/>
    </form>
</body>
</html>
