<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UploadUserState.aspx.cs" Inherits="Backend.Admin.UploadUserState" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->
<div id="generated-footer"> :: <a href="AccessControl.aspx">AccessControl</a> :: <a href="AddPromo.aspx">AddPromo</a> :: <a href="ClassicLeaderboardsGenerateTestUsers.aspx">ClassicLeaderboardsGenerateTestUsers</a> :: <a href="DeleteRecordByValue.aspx">DeleteRecordByValue</a> :: <a href="DeleteUser.aspx">DeleteUser</a> :: <a href="DownloadUserState.aspx">DownloadUserState</a> :: <a href="GlobalBan.aspx">GlobalBan</a> :: <a href="GlobalConflictScheduler.aspx">GlobalConflictScheduler</a> :: <a href="GlobalConflictStorage.aspx">GlobalConflictStorage</a> :: <a href="Index.html">Index</a> :: <a href="Leagues.aspx">Leagues</a> :: <a href="LeaguesBan.aspx">LeaguesBan</a> :: <a href="ProfileViewer.aspx">ProfileViewer</a> :: <a href="QuartzControl.aspx">QuartzControl</a> :: <a href="ReplaceWithRandomUser.aspx">ReplaceWithRandomUser</a> :: <a href="ServerUpdate.aspx">ServerUpdate</a> :: <a href="SharedLogicTest.aspx">SharedLogicTest</a> :: UploadUserState :: </div>
<!-- END OF AUTO GENERATED CONTENT (see crumps.py) -->
<form id="form1" runat="server">
        <div>
    
            <table style="width:100%;">
                <tr>
                    <td>
                        <asp:TextBox ID="userId" runat="server" Width="100%" placeholder="Type user id here..."></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:FileUpload ID="FileUploadForm" runat="server" Width="100%"/>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Button ID="buttonUpload" runat="server" Text="Upload" Width="100%" />
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Label ID="statusLine" runat="server" Text="Label"></asp:Label>
                    </td>
                </tr>
            </table>
    
        </div>
    </form>
</body>
</html>
