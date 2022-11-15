<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Leagues.aspx.cs" Inherits="Backend.Admin.Leagues" %>
<%@ Import Namespace="ServerLib" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->
<div id="generated-footer"> :: <a href="AccessControl.aspx">AccessControl</a> :: <a href="AddPromo.aspx">AddPromo</a> :: <a href="ClassicLeaderboardsGenerateTestUsers.aspx">ClassicLeaderboardsGenerateTestUsers</a> :: <a href="DeleteRecordByValue.aspx">DeleteRecordByValue</a> :: <a href="DeleteUser.aspx">DeleteUser</a> :: <a href="DownloadUserState.aspx">DownloadUserState</a> :: <a href="GlobalBan.aspx">GlobalBan</a> :: <a href="GlobalConflictScheduler.aspx">GlobalConflictScheduler</a> :: <a href="GlobalConflictStorage.aspx">GlobalConflictStorage</a> :: <a href="Index.html">Index</a> :: Leagues :: <a href="LeaguesBan.aspx">LeaguesBan</a> :: <a href="ProfileViewer.aspx">ProfileViewer</a> :: <a href="QuartzControl.aspx">QuartzControl</a> :: <a href="ReplaceWithRandomUser.aspx">ReplaceWithRandomUser</a> :: <a href="ServerUpdate.aspx">ServerUpdate</a> :: <a href="SharedLogicTest.aspx">SharedLogicTest</a> :: <a href="UploadUserState.aspx">UploadUserState</a> :: </div>
<!-- END OF AUTO GENERATED CONTENT (see crumps.py) -->
<form id="form1" runat="server">
        <div>
            <asp:Label runat="server" ID="lblBotsUpdateInfo"></asp:Label>
            <asp:Button runat="server" ID="btnRecalcBots" Text="Update Now"/>
            <div>
            <label>Season id: </label>
            <asp:Label runat="server" ID="lblSeasonId"></asp:Label>
            </div>
        <div>
            <label>Season period: </label>
            <asp:Label runat="server" ID="lblCycleTime"></asp:Label>
        </div>
        <div>
            <label>Season start: </label>
            <asp:Label runat="server" ID="lblSeasonStart"></asp:Label>
        </div>
        <div>
            <label>Season end: </label>
            <asp:Label runat="server" ID="lblSeasonEnd"></asp:Label>
        </div>
            <% if (AppSettings.Default.LeagueDebug)
               { %>
                <div>
                    <asp:Button runat="server" ID="btnEndSeason" Text="End Season"/>
                </div>
                <div  style="margin-top: 20px">
                    <asp:Label runat="server" ID="lblStatus"></asp:Label>
                </div>
                <div>
                    <label>PlayerId: </label>
                    <asp:TextBox runat="server" ID="txtPlayerId"></asp:TextBox>
                </div>
                <div>
                    <label>Add score:</label>
                    <asp:TextBox runat="server" ID="txtScoreAmount" TextMode="Number" Text="100"></asp:TextBox>
                    <asp:Button runat="server" ID="btnAddScore" Text="Add"/>
                </div>
                <div>
                    <asp:Button runat="server" ID="btnRemovePlayer" Text="Remove"/>
                </div>
            <% } %>
        </div>
        <div>
            <asp:Label runat="server" ID="lblGlobalTop"></asp:Label>
        </div>
    </form>
</body>
</html>
