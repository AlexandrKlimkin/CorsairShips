<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SharedLogicTest.aspx.cs" Inherits="Backend.Admin.SharedLogicTest" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->
<div id="generated-footer"> :: <a href="AccessControl.aspx">AccessControl</a> :: <a href="AddPromo.aspx">AddPromo</a> :: <a href="ClassicLeaderboardsGenerateTestUsers.aspx">ClassicLeaderboardsGenerateTestUsers</a> :: <a href="DeleteRecordByValue.aspx">DeleteRecordByValue</a> :: <a href="DeleteUser.aspx">DeleteUser</a> :: <a href="DownloadUserState.aspx">DownloadUserState</a> :: <a href="GlobalBan.aspx">GlobalBan</a> :: <a href="GlobalConflictScheduler.aspx">GlobalConflictScheduler</a> :: <a href="GlobalConflictStorage.aspx">GlobalConflictStorage</a> :: <a href="Index.html">Index</a> :: <a href="Leagues.aspx">Leagues</a> :: <a href="LeaguesBan.aspx">LeaguesBan</a> :: <a href="ProfileViewer.aspx">ProfileViewer</a> :: <a href="QuartzControl.aspx">QuartzControl</a> :: <a href="ReplaceWithRandomUser.aspx">ReplaceWithRandomUser</a> :: <a href="ServerUpdate.aspx">ServerUpdate</a> :: SharedLogicTest :: <a href="UploadUserState.aspx">UploadUserState</a> :: </div>
<!-- END OF AUTO GENERATED CONTENT (see crumps.py) -->
<form id="form1" runat="server">
    <table style="width: 100%;">
        <tr>
            <td>request.bin</td>
            <td><asp:FileUpload ID="FileUploadControlRequest" runat="server" accept=".bin" /></td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>state.bin</td>
            <td><asp:FileUpload ID="FileUploadControlState" runat="server" accept=".bin" /></td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td><asp:Button runat="server" id="UploadButton" text="Upload" onclick="UploadButton_Click" /></td>
        </tr>
    </table>
    <asp:Label ID="StatusLabel" runat="server" text="Upload status:" ></asp:Label>
    
</form>
</body>
</html>
