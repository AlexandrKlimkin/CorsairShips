<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GlobalBan.aspx.cs" Inherits="Backend.Admin.GlobalBan" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->
<div id="generated-footer"> :: <a href="DeleteUser.aspx">DeleteUser</a> :: <a href="Feedback.aspx">Feedback</a> :: GlobalBan :: <a href="Index.html">Index</a> :: <a href="MessageBox.aspx">MessageBox</a> :: <a href="ProfileViewer.aspx">ProfileViewer</a> :: <a href="PurchaseItem.aspx">PurchaseItem</a> :: <a href="ReportsViewer.aspx">ReportsViewer</a> :: </div>
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
