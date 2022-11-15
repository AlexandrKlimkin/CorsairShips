<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReplaceWithRandomUser.aspx.cs" Inherits="Backend.Admin.ReplaceWithRandomUser" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->
<div id="generated-footer"> :: <a href="AccessControl.aspx">AccessControl</a> :: <a href="AddPromo.aspx">AddPromo</a> :: <a href="ClassicLeaderboardsGenerateTestUsers.aspx">ClassicLeaderboardsGenerateTestUsers</a> :: <a href="DeleteRecordByValue.aspx">DeleteRecordByValue</a> :: <a href="DeleteUser.aspx">DeleteUser</a> :: <a href="DownloadUserState.aspx">DownloadUserState</a> :: <a href="GlobalBan.aspx">GlobalBan</a> :: <a href="GlobalConflictScheduler.aspx">GlobalConflictScheduler</a> :: <a href="GlobalConflictStorage.aspx">GlobalConflictStorage</a> :: <a href="Index.html">Index</a> :: <a href="Leagues.aspx">Leagues</a> :: <a href="LeaguesBan.aspx">LeaguesBan</a> :: <a href="ProfileViewer.aspx">ProfileViewer</a> :: <a href="QuartzControl.aspx">QuartzControl</a> :: ReplaceWithRandomUser :: <a href="ServerUpdate.aspx">ServerUpdate</a> :: <a href="SharedLogicTest.aspx">SharedLogicTest</a> :: <a href="UploadUserState.aspx">UploadUserState</a> :: </div>
<!-- END OF AUTO GENERATED CONTENT (see crumps.py) -->
<form id="form1" runat="server">
        <label>State of the "My Player Id" will be replaced with a random user's state</label>
        <div>
            <asp:Label ID="DbState" runat="server"></asp:Label>
        </div>
        <div>
            <label>My Player Id</label>
            <asp:TextBox ID="PlayerIdBox" runat="server" Width="313px"></asp:TextBox>
            <label>min state size (kb)</label>
            <asp:TextBox ID="MinStateSize" runat="server" Width="50px" Text="6"></asp:TextBox>
            <asp:Button ID="ReplaceButton" runat="server" Text="Replace" />
        </div>
        <asp:Label ID="Status" runat="server"></asp:Label>
    </form>
</body>
</html>
