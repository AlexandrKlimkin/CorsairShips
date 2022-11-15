<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ProfileViewer.aspx.cs" Inherits="Backend2.Admin.ProfileViewer" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->
<div id="generated-footer"> :: <a href="DeleteUser.aspx">DeleteUser</a> :: <a href="Feedback.aspx">Feedback</a> :: <a href="GlobalBan.aspx">GlobalBan</a> :: <a href="Index.html">Index</a> :: <a href="MessageBox.aspx">MessageBox</a> :: ProfileViewer :: <a href="PurchaseItem.aspx">PurchaseItem</a> :: <a href="ReportsViewer.aspx">ReportsViewer</a> :: </div>
<!-- END OF AUTO GENERATED CONTENT (see crumps.py) -->
<form id="form1" runat="server">
        <div runat="server" id="ErrorDiv">
            <asp:Label runat="server" id="ErrorDesc" ForeColor="red"></asp:Label>
        </div>
        <div runat="server" id="LoadDiv">
            <input name="action" type="hidden" value="Load"/>
            <label>Player Id</label>
            <asp:TextBox ID="PlayerIdBox" runat="server" Width="313px"></asp:TextBox>
            <label>Lock period</label>
            <asp:TextBox ID="LockPeriodBox" runat="server">00:15:00</asp:TextBox>
            <asp:Button ID="LoadButton" runat="server" Text="Load" />
            <asp:CheckBox ID="UpdateState" runat="server" Text="Force update user state" />
        </div>
        <asp:Label ID="Status" runat="server"></asp:Label>
        <div runat="server" id="SaveDiv">
            <label>Profile lock expires at: </label>
            <asp:Label ID="LockExpire" runat="server" Text="...time..."></asp:Label>
            <br/>
            <input name="action" type="hidden" value="Save"/>
            <input asp-for="PlayerId" type="hidden"/>
            <label>Displaying state for </label>
            <asp:Label ID="PlayerId2" runat="server" Text="guid"></asp:Label>
            <br/>
            <label>Save as:</label>
            <asp:TextBox ID="SaveToPlayerIdBox" runat="server" Width="354px"></asp:TextBox>
            <br/>
            <asp:Button ID="SaveButton" runat="server" Text="Save" />
            <br/>
            <asp:Table ID="Table1" runat="server" Height="100%" Width="100%" ClientIDMode="Predictable">
            </asp:Table>
        </div>
    </form>
</body>
</html>
