<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Feedback.aspx.cs" Inherits="Backend.CM.Feedback" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->
<div id="generated-footer"> :: <a href="DeleteUser.aspx">DeleteUser</a> :: Feedback :: <a href="GlobalBan.aspx">GlobalBan</a> :: <a href="Index.html">Index</a> :: <a href="MessageBox.aspx">MessageBox</a> :: <a href="ProfileViewer.aspx">ProfileViewer</a> :: <a href="PurchaseItem.aspx">PurchaseItem</a> :: <a href="ReportsViewer.aspx">ReportsViewer</a> :: </div>
<!-- END OF AUTO GENERATED CONTENT (see crumps.py) -->
<form id="form1" runat="server">
        <div>
            <asp:Label runat="server" ID="lblFeedbackDbStatus"/>
            
            <asp:TextBox runat="server" ID="txtDateFrom" />
            <asp:TextBox runat="server" ID="txtDateTo" />

            <asp:Button runat="server" ID="btnDownload" Text="Download" UseSubmitBehavior="False"/>
        </div>
        <div>
            <asp:Label runat="server" ID="lblStatus"/>
        </div>
        <% if (ShowDebug)
           { %>
            <div>
                <label>DEBUG:</label>
                <table>
                    <tr>
                        <td>email: </td>
                        <td>caption: </td>
                        <td>description: </td>
                    </tr>
                    <tr>
                        <td><asp:TextBox runat="server" ID="txtNewFeedbackEmail"></asp:TextBox></td>
                        <td><asp:TextBox runat="server" ID="txtNewFeedbackCaption"></asp:TextBox></td>
                        <td><asp:TextBox runat="server" ID="txtNewFeedbackDescription"></asp:TextBox></td>
                    </tr>
                </table>
                <asp:Button runat="server" ID="btnSendFeedback" Text="SendFeedback"/>
            </div>
        <% } %>
    </form>
</body>
</html>
