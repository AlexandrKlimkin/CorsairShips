<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PurchaseItem.aspx.cs" Inherits="Backend.Admin.PurchaseItem" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        #Text1 {
            width: 315px;
        }
        #TextArea1 {
            height: 536px;
        }
    </style>
</head>
<body>
<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->
<div id="generated-footer"> :: <a href="DeleteUser.aspx">DeleteUser</a> :: <a href="Feedback.aspx">Feedback</a> :: <a href="GlobalBan.aspx">GlobalBan</a> :: <a href="Index.html">Index</a> :: <a href="MessageBox.aspx">MessageBox</a> :: <a href="ProfileViewer.aspx">ProfileViewer</a> :: PurchaseItem :: <a href="ReportsViewer.aspx">ReportsViewer</a> :: </div>
<!-- END OF AUTO GENERATED CONTENT (see crumps.py) -->
<form id="form1" runat="server">
        <div>
            <table style="width:100%;">
                <tr>
                    <td>
                        <asp:TextBox ID="guid" runat="server" Text="type GUID here" Width="317px"></asp:TextBox>
                        <br />
                        <asp:TextBox ID="receipt" runat="server" Text="type GPA or receipt here" Width="317px"></asp:TextBox>
                        <br />
                        <asp:ListBox ID="skuList" runat="server" Height="476px" OnSelectedIndexChanged="ListBox1_SelectedIndexChanged" Width="328px"></asp:ListBox>
                        <br />
                        <asp:Button ID="giveItem" runat="server" Text="Give item" Width="324px" />
                        <br />
                        <asp:Label ID="statusLine" runat="server" Text=""/>
                    </td>
                    <td>
                        <asp:TextBox ID="transactions" runat="server" Height="500px" style="margin-top: 0px" TextMode="MultiLine" Width="600px"></asp:TextBox>
                </tr>
            </table>

        </div>
    </form>
</body>
</html>
