<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="../CM/GlobalBan.aspx.cs" Inherits="Backend.Admin.GlobalBan" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->
<div id="generated-footer"> :: <a href="AccessControl.aspx">AccessControl</a> :: <a href="AddPromo.aspx">AddPromo</a> :: <a href="ClassicLeaderboardsGenerateTestUsers.aspx">ClassicLeaderboardsGenerateTestUsers</a> :: <a href="DeleteRecordByValue.aspx">DeleteRecordByValue</a> :: <a href="DeleteUser.aspx">DeleteUser</a> :: <a href="DownloadUserState.aspx">DownloadUserState</a> :: GlobalBan :: <a href="GlobalConflictScheduler.aspx">GlobalConflictScheduler</a> :: <a href="GlobalConflictStorage.aspx">GlobalConflictStorage</a> :: <a href="Index.html">Index</a> :: <a href="Leagues.aspx">Leagues</a> :: <a href="LeaguesBan.aspx">LeaguesBan</a> :: <a href="ProfileViewer.aspx">ProfileViewer</a> :: <a href="QuartzControl.aspx">QuartzControl</a> :: <a href="ReplaceWithRandomUser.aspx">ReplaceWithRandomUser</a> :: <a href="ServerUpdate.aspx">ServerUpdate</a> :: <a href="SharedLogicTest.aspx">SharedLogicTest</a> :: <a href="UploadUserState.aspx">UploadUserState</a> :: </div>
<!-- END OF AUTO GENERATED CONTENT (see crumps.py) -->
<form id="form1" runat="server">
        <div>
            <asp:Label runat="server" ID="lblStatus"></asp:Label>
        </div>
        <div>
            <asp:TextBox runat="server" ID="tbUserId"></asp:TextBox>
            <asp:TextBox runat="server" ID="tbReason" placeholder="Ban reason"></asp:TextBox>
            <asp:Button runat="server" ID="btnBan" Text="Ban"/>
            <asp:Button runat="server" ID="btnUnban" Text="Unban"/>
        </div>
        <div>
            <asp:Label runat="server" ID="lblGlobalBanList"></asp:Label>
        </div>
    </form>
</body>
</html>
