<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GlobalConflictStorage.aspx.cs" Inherits="Backend.Admin.GlobalConflictStorage" %>
<%@ Import Namespace="ServerLib" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->
<div id="generated-footer"> :: <a href="AccessControl.aspx">AccessControl</a> :: <a href="AddPromo.aspx">AddPromo</a> :: <a href="ClassicLeaderboardsGenerateTestUsers.aspx">ClassicLeaderboardsGenerateTestUsers</a> :: <a href="DeleteRecordByValue.aspx">DeleteRecordByValue</a> :: <a href="DeleteUser.aspx">DeleteUser</a> :: <a href="DownloadUserState.aspx">DownloadUserState</a> :: <a href="GlobalBan.aspx">GlobalBan</a> :: <a href="GlobalConflictScheduler.aspx">GlobalConflictScheduler</a> :: GlobalConflictStorage :: <a href="Index.html">Index</a> :: <a href="Leagues.aspx">Leagues</a> :: <a href="LeaguesBan.aspx">LeaguesBan</a> :: <a href="ProfileViewer.aspx">ProfileViewer</a> :: <a href="QuartzControl.aspx">QuartzControl</a> :: <a href="ReplaceWithRandomUser.aspx">ReplaceWithRandomUser</a> :: <a href="ServerUpdate.aspx">ServerUpdate</a> :: <a href="SharedLogicTest.aspx">SharedLogicTest</a> :: <a href="UploadUserState.aspx">UploadUserState</a> :: </div>
<!-- END OF AUTO GENERATED CONTENT (see crumps.py) -->
<% if(!AppSettings.Default.GloabalConflict) { %>
<h1>Global Conflict module switched off</h1>
        <p>Set GlobalConflict=true in appsettings.json to enable</p>
<%     }
       else {%>
    <form id="form1" runat="server">
        <div>
            <div>
                <asp:Label runat="server" ID="lblDbInfo"></asp:Label>
            </div>
            <div>
                <asp:Label runat="server" ID="lblStatus"></asp:Label>
            </div>
            <div>
                <label>Upload global conflict prototypes:</label>
                <asp:FileUpload runat="server" ID="btnUploadBrowse"/>
                <asp:Button runat="server" ID="btnUpload" Text="Upload"/>
            </div>
            <div>
                <asp:Table runat="server" ID="StoredPrototypes" BorderWidth="1"></asp:Table>
            </div>
        </div>
    </form>
<% } %>
</body>
</html>
