<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ServerUpdate.aspx.cs" Inherits="Server.Admin.ServerUpdate" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .auto-style1 {
            width: 117px;
        }
        .auto-style2 {
            width: 83px;
        }
        .auto-style3 {
            width: 47px;
        }
    </style>
</head>
<body>
<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->
<div id="generated-footer"> :: <a href="AccessControl.aspx">AccessControl</a> :: <a href="AddPromo.aspx">AddPromo</a> :: <a href="ClassicLeaderboardsGenerateTestUsers.aspx">ClassicLeaderboardsGenerateTestUsers</a> :: <a href="DeleteRecordByValue.aspx">DeleteRecordByValue</a> :: <a href="DeleteUser.aspx">DeleteUser</a> :: <a href="DownloadUserState.aspx">DownloadUserState</a> :: <a href="GlobalBan.aspx">GlobalBan</a> :: <a href="GlobalConflictScheduler.aspx">GlobalConflictScheduler</a> :: <a href="GlobalConflictStorage.aspx">GlobalConflictStorage</a> :: <a href="Index.html">Index</a> :: <a href="Leagues.aspx">Leagues</a> :: <a href="LeaguesBan.aspx">LeaguesBan</a> :: <a href="ProfileViewer.aspx">ProfileViewer</a> :: <a href="QuartzControl.aspx">QuartzControl</a> :: <a href="ReplaceWithRandomUser.aspx">ReplaceWithRandomUser</a> :: ServerUpdate :: <a href="SharedLogicTest.aspx">SharedLogicTest</a> :: <a href="UploadUserState.aspx">UploadUserState</a> :: </div>
<!-- END OF AUTO GENERATED CONTENT (see crumps.py) -->
<form id="form1" runat="server">
    <div style="vertical-align: top">
        <table style="width: 100%;">
            <tr>
                <td>
                    <asp:Label ID="lblCurMaintenance" runat="server"></asp:Label>
                </td>
                <td>&nbsp;<td>
                &nbsp;</tr>
            <tr>
                <td><asp:Button ID="StartUpdateTimer" runat="server" Text="Start update timer" /></td>
                <td><asp:Label ID="Label1" runat="server" Text="User will see a timer for 15 minutes. After that, game will be blocked, until you press button 'Remove update timer'"></asp:Label><td>
            </tr>
            <tr>
                <td><asp:Button ID="RemoveUpdateTimer" runat="server" Text="Remove update timer" /></td>
            </tr>
            <tr>
                <td><asp:Button ID="BlockServerImmediate" runat="server" Text="Block server immediate" /></td>
                <td><asp:Label ID="Label2" runat="server" Text="It will block server immediately. You should not use it in most cases."></asp:Label></td>
            </tr>
            <tr>
                <td>
                    <asp:Calendar ID="calDate" runat="server" BackColor="White" BorderColor="#3366CC" BorderWidth="1px" CellPadding="1" DayNameFormat="Shortest" Font-Names="Verdana" Font-Size="8pt" ForeColor="#003399" Height="200px" TitleFormat="Month" Width="220px">
                        <DayHeaderStyle BackColor="#99CCCC" ForeColor="#336666" Height="1px" />
                        <NextPrevStyle Font-Size="8pt" ForeColor="#CCCCFF" />
                        <OtherMonthDayStyle ForeColor="#999999" />
                        <SelectedDayStyle BackColor="#009999" Font-Bold="True" ForeColor="#CCFF99" />
                        <SelectorStyle BackColor="#99CCCC" ForeColor="#336666" />
                        <TitleStyle BackColor="#003399" BorderColor="#3366CC" BorderWidth="1px" Font-Bold="True" Font-Size="10pt" ForeColor="#CCCCFF" Height="25px" />
                        <TodayDayStyle BackColor="#99CCCC" ForeColor="White" />
                        <WeekendDayStyle BackColor="#CCCCFF" />
                    </asp:Calendar>
                    <table style="width:100%;">
                        <tr>
                            <td class="auto-style1">
                                <asp:Label ID="Label3" runat="server" Text="Hours"></asp:Label>
                            </td>
                            <td class="auto-style2">
                                <asp:DropDownList ID="ddHour" runat="server" AutoPostBack="True">
                                </asp:DropDownList>
                            </td>
                            <td class="auto-style3">
                                <asp:Label ID="Label4" runat="server" Text="Minutes"></asp:Label>
                            </td>
                            <td>
                                <asp:DropDownList ID="ddMinute" runat="server" AutoPostBack="True">
                                </asp:DropDownList>
                            </td>
                        </tr>
                    </table>
                    <asp:Label ID="lblTime" runat="server"></asp:Label>
                    <br />
                    <asp:Button ID="btnSchedule" runat="server" Text="Schedule" />
                </td>
                <td>Schedule maintenance to specific time</td>
            </tr>
        </table>
    </div>
    </form>
</body>
</html>
