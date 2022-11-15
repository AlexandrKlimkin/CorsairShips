<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MessageBox.aspx.cs" Inherits="Backend.Admin.WebForm1" %>
<%@ Import Namespace="PestelLib.ServerCommon.Messaging" %>
<%@ Import Namespace="Newtonsoft.Json" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" href="//code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
    <script src="https://code.jquery.com/jquery-1.12.4.js"></script>
    <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.js"></script>
    <script>
        var rewardId = 0;
        $(function () {
            <%
                var geoSource = ISO3166.Country.List.Select(_ => _.Name).ToArray();
                var geoJson = JsonConvert.SerializeObject(geoSource);
                var chestRewardsJson = JsonConvert.SerializeObject(ChestRewardIds);
            %>
            var geoVals = <%=geoJson%>;
            var chestRewardsVals = <%=chestRewardsJson%>;

            var chestRewardTemplate = '<select name="chestReward">';
            for (var i = 0; i < chestRewardsVals.length; ++i) {
                var reward = chestRewardsVals[i];
                chestRewardTemplate += '<option>' + reward + '</option>';
            }
            chestRewardTemplate += '</select>';
            var customRewardTemplate = '<label>Id</label><input type="text" name="customRewardId"/><label>Amount</label><input type="text" name="customRewardAmount"/>';

            function split( val ) {
                return val.split( /,\s*/ );
            }
            function extractLast( term ) {
                return split( term ).pop();
            }
            $("#txtGeoFilter")
                .on( "keydown", function( event ) {
                    if ( event.keyCode === $.ui.keyCode.TAB &&
                        $( this ).autocomplete( "instance" ).menu.active ) {
                        event.preventDefault();
                    }
                })
                .autocomplete({
                    minLength: 0,
                    source: function( request, response ) {
                        // delegate back to autocomplete, but extract the last term
                        response( $.ui.autocomplete.filter(
                            geoVals, extractLast( request.term ) ) );
                    },
                    focus: function() {
                        // prevent value inserted on focus
                        return false;
                    },
                    select: function( event, ui ) {
                        var terms = split( this.value );
                        // remove the current input
                        terms.pop();
                        // add the selected item
                        terms.push( ui.item.value );
                        // add placeholder to get the comma-and-space at the end
                        terms.push( "" );
                        this.value = terms.join( ", " );
                        return false;
                    }
                });

            function show_ui() {
                var sel = $("#MessageType option:selected").val();
                if (sel == 'Private message') {
                    $("#broadcast_div").hide();
                    $("#private_div").show();
                } else {
                    $("#broadcast_div").show();
                    $("#private_div").hide();
                }
            }
            $("#MessageType").change(function () {
                show_ui();
            });

            function wrapRaward(reward) {
                var result =
                    `<div id="rewardDiv${rewardId}">${reward}<button type="button" onclick="$('#rewardDiv${rewardId}').remove()">Remove</button></div>`;
                ++rewardId;
                return result;
            }

            show_ui();
            $('#addChestRewardButton').click(function() {
                $('#rawardsContainer').append(wrapRaward(chestRewardTemplate));
            });
            $('#addCustomRewardButton').click(function() {
                $('#rawardsContainer').append(wrapRaward(customRewardTemplate));
            });
            $( ".datepicker" ).datepicker({
                changeMonth: true,
                changeYear: true,
                dateFormat: 'yy-mm-dd'
            });
        });
    </script>
</head>
<body>
<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->
<div id="generated-footer"> :: <a href="DeleteUser.aspx">DeleteUser</a> :: <a href="Feedback.aspx">Feedback</a> :: <a href="GlobalBan.aspx">GlobalBan</a> :: <a href="Index.html">Index</a> :: MessageBox :: <a href="ProfileViewer.aspx">ProfileViewer</a> :: <a href="PurchaseItem.aspx">PurchaseItem</a> :: <a href="ReportsViewer.aspx">ReportsViewer</a> :: </div>
<!-- END OF AUTO GENERATED CONTENT (see crumps.py) -->
<form id="form1" runat="server">
        <div>
            <asp:Label runat="server" ID="Status" ForeColor="#ff0000"></asp:Label>
        </div>
        <div>
            <asp:Label runat="server" ID="PrivateDbStatus"></asp:Label>
        </div>
        <div>
            <asp:Label runat="server" ID="BroadcastDbStatus"></asp:Label>
        </div>
        <div>
            <asp:Table runat="server" ID="PrivateMessagesTable"></asp:Table>
        </div>
        <div>
            <label>Last 20 broadcast messages:</label>
        </div>
        <div>
            <asp:Table runat="server" ID="BroadcastMessagesTable" BorderWidth="1"></asp:Table>
        </div>
        <label>Message type</label>
        <div>
            <select id="MessageType" name="<%=MessageTypeSelectorName%>">
                <option selected><%=MessageTypePrivate%></option>
                <option><%=MessageTypeBroadcast%></option>
                <option><%=MessageTypeWelcomeLetter%></option>
            </select>
        </div>
    <div>
        <label>Delivery Date:</label> 
        <div><input type="text" class="datepicker" name="deliveryDate"/></div>
        <label>Delivery Time:</label> 
        <div>
        <select name="deliveryHour">
            <%
                for (var i = 0; i < 24; ++i)
                {
                    %>
                    <option><%=i.ToString("00")%></option>
                    <%
                }
            %>
        </select>
        <select name="deliveryMinute">
            <%
                for (var i = 0; i < 60; ++i)
                {
            %>
                <option><%=i.ToString("00")%></option>
            <%
                }
            %>
        </select>
        </div>
        <label>Time zone (you are just set time in that time zone?):</label>
        <div>
        <select name="timeZone"><option value="0">GMT</option><option value="-180">MSK (GMT+3)</option></select>
        </div>
    </div>
    <div id="private_div">
        <div>
            <label>Player ID:</label>
            <asp:TextBox runat="server" ID="txtPlayerId"></asp:TextBox>
        </div>
    </div>
    <div id="broadcast_div">
        <div>
            <label>Expiry Date:</label> 
            <div><input type="text" class="datepicker" name="expiryDate"/></div>
            <label>Expiry Time:</label> 
            <div>
                <select name="expiryHour">
                    <%
                        for (var i = 0; i < 24; ++i)
                        {
                    %>
                        <option><%=i.ToString("00")%></option>
                    <%
                    }
                    %>
                </select>
                <select name="expiryMinute">
                    <%
                        for (var i = 0; i < 60; ++i)
                        {
                    %>
                        <option><%=i.ToString("00")%></option>
                    <%
                    }
                    %>
                </select>
            </div>
        </div>
        <div>
            <label>Player ID(s) (separate ids with new line or space):</label>
        </div>
        <asp:TextBox runat="server" TextMode="MultiLine" ID="txtPlayerIds"></asp:TextBox>
        <div>
            <label>Users with any following shared logic version will receive a message (separate multiple values with space eg.'245 250'):</label>
        </div>
        <div>
            <asp:TextBox runat="server" ID="txtSlFilter" Columns="200"></asp:TextBox>
        </div>
        <div>
            <label>Users with any following system language will receive a message:</label>
        </div>
        <div>
            <asp:CheckBox runat="server" Text="Others" ID="cbSysLangOthers" Checked="True"/>
            <asp:CheckBox runat="server" Text="English" ID="cbSysLangEnglish" Checked="True"/>
            <asp:CheckBox runat="server" Text="Russian" ID="cbSysLangRussian" Checked="True"/>
            <asp:CheckBox runat="server" Text="French" ID="cbSysLangFrench" Checked="True"/>
            <asp:CheckBox runat="server" Text="Italian" ID="cbSysLangItalian" Checked="True"/>
            <asp:CheckBox runat="server" Text="German" ID="cbSysLangGerman" Checked="True"/>
            <asp:CheckBox runat="server" Text="Spanish" ID="cbSysLangSpanish" Checked="True"/>
            <asp:CheckBox runat="server" Text="Japanese" ID="cbSysLangJapanese" Checked="True"/>
            <asp:CheckBox runat="server" Text="Korean" ID="cbSysLangKorean" Checked="True"/>
            <asp:CheckBox runat="server" Text="Portuguese" ID="cbSysLangPortuguese" Checked="True"/>
            <asp:CheckBox runat="server" Text="ChineseTraditional" ID="cbSysLangChineseTraditional" Checked="True"/>
        </div>
        <div>
            <label>User at any of following locations will receive a message (start typing country name):</label>
        </div>
        <div>
            <asp:TextBox runat="server" ID="txtGeoFilter" Columns="200"></asp:TextBox>
        </div>
        <div>
            <label>Users with following AB testing group will receive a message:</label>
        </div>
        <asp:RadioButtonList runat="server" ID="rbAbFilter">
            <asp:ListItem Text="None" Selected="True"></asp:ListItem>
            <asp:ListItem Text="Group A"></asp:ListItem>
            <asp:ListItem Text="Group B"></asp:ListItem>
        </asp:RadioButtonList>
    </div>
    <div>
        <label>Enter localized message text. First line will become a header (leave first line empty for no header).</label>
    </div>
    <div>
        <label>English text:</label>
    </div>
        <asp:TextBox runat="server" TextMode="MultiLine" ID="txtMessageEnglish"></asp:TextBox>
    <div>
        <label>Russian text:</label>
    </div>
    <asp:TextBox runat="server" TextMode="MultiLine" ID="txtMessageRussian"></asp:TextBox>
    <div>
        <label>French text:</label>
    </div>
    <asp:TextBox runat="server" TextMode="MultiLine" ID="txtMessageFrench"></asp:TextBox>
    <div>
        <label>Italian text:</label>
    </div>
    <asp:TextBox runat="server" TextMode="MultiLine" ID="txtMessageItalian"></asp:TextBox>
    <div>
        <label>German text:</label>
    </div>
    <asp:TextBox runat="server" TextMode="MultiLine" ID="txtMessageGerman"></asp:TextBox>
    <div>
        <label>Spanish text:</label>
    </div>
    <asp:TextBox runat="server" TextMode="MultiLine" ID="txtMessageSpanish"></asp:TextBox>
    <div>
        <label>Korean text:</label>
    </div>
    <asp:TextBox runat="server" TextMode="MultiLine" ID="txtMessageKorean"></asp:TextBox>
    <div>
        <label>Japanese text:</label>
    </div>
    <asp:TextBox runat="server" TextMode="MultiLine" ID="txtMessageJapanese"></asp:TextBox>
    <div>
        <label>Portuguese text:</label>
    </div>
    <asp:TextBox runat="server" TextMode="MultiLine" ID="txtMessagePortuguese"></asp:TextBox>
    <div>
        <label>ChineseTraditional text:</label>
    </div>
    <asp:TextBox runat="server" TextMode="MultiLine" ID="txtMessageChineseTraditional"></asp:TextBox>
    <div>
        <label><b>Rewards:</b></label>
    </div>
    <div>
        <button id="addChestRewardButton" type="button">Add Chest Reward</button>
        <button id="addCustomRewardButton" type="button">Add Custom Reward</button>
        <div id="rawardsContainer">

        </div>
    </div>
    <div>
        <asp:Button runat="server" ID="btnSendMessage" Text="Send"/>
    </div>
    </form>
</body>
</html>
