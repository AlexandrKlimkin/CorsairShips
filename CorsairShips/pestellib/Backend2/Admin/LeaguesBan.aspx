<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LeaguesBan.aspx.cs" Inherits="Backend.Admin.LeaguesBan" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->
<div id="generated-footer"> :: <a href="AccessControl.aspx">AccessControl</a> :: <a href="AddPromo.aspx">AddPromo</a> :: <a href="ClassicLeaderboardsGenerateTestUsers.aspx">ClassicLeaderboardsGenerateTestUsers</a> :: <a href="DeleteRecordByValue.aspx">DeleteRecordByValue</a> :: <a href="DeleteUser.aspx">DeleteUser</a> :: <a href="DownloadUserState.aspx">DownloadUserState</a> :: <a href="GlobalBan.aspx">GlobalBan</a> :: <a href="GlobalConflictScheduler.aspx">GlobalConflictScheduler</a> :: <a href="GlobalConflictStorage.aspx">GlobalConflictStorage</a> :: <a href="Index.html">Index</a> :: <a href="Leagues.aspx">Leagues</a> :: LeaguesBan :: <a href="ProfileViewer.aspx">ProfileViewer</a> :: <a href="QuartzControl.aspx">QuartzControl</a> :: <a href="ReplaceWithRandomUser.aspx">ReplaceWithRandomUser</a> :: <a href="ServerUpdate.aspx">ServerUpdate</a> :: <a href="SharedLogicTest.aspx">SharedLogicTest</a> :: <a href="UploadUserState.aspx">UploadUserState</a> :: </div>
<!-- END OF AUTO GENERATED CONTENT (see crumps.py) -->
<form id="form1" runat="server">
        <div>
            <asp:Label runat="server" ID="lblStatus"></asp:Label>
        </div>
        <div>
            <label>Exact score value:</label>
            <asp:TextBox runat="server" ID="txtScore"></asp:TextBox>
            <asp:Button runat="server" Text="Ban" ID="btnBan"/>
        </div>
    <div>
        <label>Player id:</label>
        <asp:TextBox runat="server" ID="txtPlayerId"></asp:TextBox>
        <asp:Button runat="server" Text="Ban" ID="btnBanById"/>
    </div>
    </form>
</body>
</html>
