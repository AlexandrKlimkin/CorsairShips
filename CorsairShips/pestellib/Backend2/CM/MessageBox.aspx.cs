using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using BackendCommon.Code.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PestelLib.ServerCommon.Messaging;
using ServerLib.Modules.Messages;
using ServerLib.Modules.ServerMessages;
using ServerShared.Messaging;
using UnityDI;
using BackendCommon.Code.Utils;

namespace Backend.Admin
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        public const string MessageTypeSelectorName = "selMessageType";
        public const string MessageTypePrivate = "Private message";
        public const string MessageTypeBroadcast = "Broadcast message";
        public const string MessageTypeWelcomeLetter = "Welcome letter";

        private Dictionary<string, bool> _privateSelected = new Dictionary<string, bool>();
        private Dictionary<string, bool> _broadcastSelected = new Dictionary<string, bool>();
        public string[] ChestRewardIds;
        private BroadcastMessageStorage _broadcastMessageStorage;

        private void RefreshBroadcastsTable()
        {
            var lastMessages = _broadcastMessageStorage.GetLastMessages(20);
            CreateBroadcastMessagesTable(BroadcastMessagesTable, lastMessages);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _broadcastMessageStorage = ContainerHolder.Container.Resolve<BroadcastMessageStorage>();
            RefreshBroadcastsTable();
            //_broadcastMessages = MessageUtils.ListBroadcastMessages(false).ToArray();
            //PrivateDbStatus.Text = $"PrivateDB contains {_privateMessages.Length} players inboxes";
            //BroadcastDbStatus.Text = $"BroadcastDb contains {_broadcastMessages.Length} messages";

            //CreatePrivateMessagesTable((id) => $"pcb_{id}", PrivateMessagesTable, _privateMessages);
            //CreatePrivateMessagesTable((id) => $"bcb_{id}", BroadcastMessagesTable, _broadcastMessages.Select(_ => ("All", _)).ToArray());

            btnSendMessage.Click += BtnSendMessageOnClick;

            if(ChestRewardIds == null) LoadRewards();
        }

        private void TextFileResponse(string text, string filename)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            Response.Clear();
            Response.ClearHeaders();
            Response.ClearContent();
            Response.AddHeader("Content-Disposition", $"attachment; filename={filename}");
            Response.AddHeader("Content-Length", data.Length.ToString());
            Response.ContentType = "text/plain";
            Response.Flush();
            Response.BinaryWrite(data);
            Response.End();
        }

        private void CreateBroadcastMessagesTable(Table table, params BroadcastMessage[] messages)
        {
            const int Border = 1;
            table.Rows.Clear();
            var row = new TableRow();
            var header = new[] { "Time", "SerialId", "Welcome", "Expire", "Reads", "Filters", "MessageType", "Message", "Remove filters" };
            foreach (var h in header)
            {
                var head = new TableCell();
                head.Text = h;
                head.BorderWidth = Border;
                row.Cells.Add(head);
            }

            table.Rows.Add(row);

            foreach (var m in messages)
            {
                row = new TableRow();
                var time = new TableCell();
                var serial = new TableCell();
                var expire = new TableCell();
                var reads = new TableCell();
                var filters = new TableCell();
                var messageType = new TableCell();
                var messageData = new TableCell();
                var remFilters = new TableCell();
                var welcomeMessage = new TableCell();
                time.BorderWidth = Border;
                serial.BorderWidth = Border;
                expire.BorderWidth = Border;
                reads.BorderWidth = Border;
                filters.BorderWidth = Border;
                messageType.BorderWidth = Border;
                messageData.BorderWidth = Border;
                remFilters.BorderWidth = Border;
                welcomeMessage.BorderWidth = Border;
                row.Cells.Add(time);
                row.Cells.Add(serial);
                row.Cells.Add(welcomeMessage);
                row.Cells.Add(expire);
                row.Cells.Add(reads);
                row.Cells.Add(filters);
                row.Cells.Add(messageType);
                row.Cells.Add(messageData);
                row.Cells.Add(remFilters);

                if (m.WelcomeLetter)
                    welcomeMessage.Text = "True";
                if (m.Time > DateTime.MinValue)
                    time.Text = m.Time.ToString(CultureInfo.InvariantCulture);
                else
                    time.Text = "Now";
                serial.Text = m.SerialId.ToString();
                if (m.ExpireTime < DateTime.MaxValue)
                    expire.Text = m.ExpireTime.ToString(CultureInfo.InvariantCulture);
                else
                    expire.Text = "Never";
                messageType.Text = m.Message.MessageType;
                reads.Text = m.ReadCount.ToString();

                var downloadFilters = new Button();
                downloadFilters.Text = "Download";
                downloadFilters.Click += (sender, args) =>
                {
                    TextFileResponse(JsonConvert.SerializeObject(m, Formatting.Indented), $"filters_{m.SerialId}.txt");
                };
                filters.Controls.Add(downloadFilters);

                var downloadMessage = new Button();
                downloadMessage.Text = "Download";
                downloadMessage.Click += (sender, args) =>
                {
                    if (m.Message.MessageType == typeof(LocalizedMessage).Name)
                    {
                        var localizedMessage = LocalizedMessageHelper.FromServerMessage(m.Message);
                        TextFileResponse(JsonConvert.SerializeObject(localizedMessage, Formatting.Indented), $"message_{m.SerialId}.txt");
                    }
                };
                messageData.Controls.Add(downloadMessage);

                if (m.PlayerIds != null && m.PlayerIds.Length > 0)
                {
                    var remPlayerFilter = new Button();
                    remPlayerFilter.Text = "Player filter";
                    remPlayerFilter.Click += (sender, args) =>
                        {
                            var r = _broadcastMessageStorage.ChangePlayerFilter(m.SerialId, null).Result;
                            if (!r)
                                Status.Text = $"Can't reset player filter for message {m.SerialId}.";
                            else
                                Response.Redirect("MessageBox.aspx");
                        };
                    remFilters.Controls.Add(remPlayerFilter);
                }
                else
                    remFilters.Text = "";

                table.Rows.Add(row);
            }
        }

        private void LoadRewards()
        {
            var filePath = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\ChestRewards.json";
            var fileBytes = File.ReadAllText(filePath);
            var chestRewardsRows = JsonConvert.DeserializeObject<List<JObject>>(fileBytes);
            var chestRewardIds = chestRewardsRows.Select(x => x.GetValue("Id").Value<string>());
            ChestRewardIds = chestRewardIds.Distinct().ToArray();
        }

        private DateTime ReadDate(string prefix, DateTime defaultValue)
        {
            var dateText = Request.Form[$"{prefix}Date"];
            if (string.IsNullOrEmpty(dateText))
                return defaultValue;
            if(!DateTime.TryParse(dateText + " 0:00:00Z", null, DateTimeStyles.AdjustToUniversal, out var dt))
                return defaultValue;
            
            var hourText = Request.Form[$"{prefix}Hour"];
            var minuteText = Request.Form[$"{prefix}Minute"];
            var timeZoneText = Request.Form["timeZone"];

            int.TryParse(hourText, out var hour);
            int.TryParse(minuteText, out var minute);
            int.TryParse(timeZoneText, out var tz);
            var offset = TimeSpan.FromHours(hour).Add(TimeSpan.FromMinutes(minute));
            offset = offset.Add(TimeSpan.FromMinutes(tz));

            return dt.Add(offset);
        }

        private void BtnSendMessageOnClick(object sender, EventArgs eventArgs)
        {
            var sep = new[] {','};
            var result = new LocalizedMessage();
            result.Id = DateTime.UtcNow.Ticks;
            var messageType = Request.Form[MessageTypeSelectorName];
            result.ChestRewards = (Request.Form["chestReward"] ?? "").Split(sep, StringSplitOptions.RemoveEmptyEntries);
            var customRewardIds = (Request.Form["customRewardId"] ?? "").Split(sep, StringSplitOptions.RemoveEmptyEntries);
            var customRewardAmounts = (Request.Form["customRewardAmount"] ?? "").Split(sep, StringSplitOptions.RemoveEmptyEntries);
            var customRewards = new List<LocalizedMessageCustomReward>();
            if (customRewardIds.Length > 0)
            {
                for(var i = 0; i < customRewardIds.Length; ++i)
                {
                    var id = customRewardIds[i];
                    var amount = customRewardAmounts[i];
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        Status.Text = "Custom reward id not set.";
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(amount))
                    {
                        Status.Text = "Custom reward amount not set.";
                        return;
                    }

                    if (!int.TryParse(amount, out var amountInt))
                    {
                        Status.Text = "Custom reward amount must be an integer.";
                        return;
                    }

                    var customReward = new LocalizedMessageCustomReward();
                    customReward.Id = id;
                    customReward.Amount = amountInt;
                    customRewards.Add(customReward);
                }
            }

            result.CustomRewards = customRewards.ToArray();

            if (!PackMessage(result))
            {
                return; // error
            }
            result.WelcomeMessage = messageType == MessageTypeWelcomeLetter;
            result.DeliveryDate = ReadDate("delivery", DateTime.MinValue);
            if(result.DeliveryDate == DateTime.MinValue)
                result.DeliveryDate = DateTime.UtcNow;
            if (messageType == MessageTypePrivate)
                SendPrivateMessage(result);
            else if(messageType == MessageTypeBroadcast || messageType == MessageTypeWelcomeLetter)
                SendBroadcastMessage(result);

            if(string.IsNullOrEmpty(Status.Text))
                //RefreshBroadcastsTable();
                Response.Redirect("MessageBox.aspx");
        }

        private void SendPrivateMessage(LocalizedMessage message)
        {
            var playerId = PlayerIdHelper.FromString(txtPlayerId.Text);
            if (playerId == Guid.Empty)
            {
                Status.Text = $"Wrong player id format.";
                return;
            }

            if (!StateLoader.Storage.UserExist(playerId))
            {
                Status.Text = $"Player {playerId} not found.";
                return;
            }

            if (message.DeliveryDate > DateTime.MinValue)
            {
                var filter = new BroadcastMessageFilter();
                filter[PlayerIdsFilter.Name] = new[] {playerId.ToString()};
                var serverMessage = LocalizedMessageHelper.ToServerMessage(message);
                _broadcastMessageStorage.PushMessage(message.Id, message.DeliveryDate, DateTime.MaxValue,  serverMessage, filter);
            }
            else
            {
                var serverMessage = LocalizedMessageHelper.ToServerMessage(message);
                ServerMessageUtils.SendMessage(serverMessage, playerId);
            }
        }

        private void SendBroadcastMessage(LocalizedMessage message)
        {
            if (message.DeliveryDate > DateTime.MinValue)
            {
                message.Id = message.DeliveryDate.Ticks;
            }
            var playerIdsText = txtPlayerIds.Text.Split(new[] {' ', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            var playerIds = new List<Guid>();
            var filter = new BroadcastMessageFilter();
            foreach (var playerId in playerIdsText)
            {
                var guid = PlayerIdHelper.FromString(playerId);
                if (guid == Guid.Empty)
                {
                    Status.Text = $"Wrong player id format in '{playerId}'.";
                    return;
                }

                if (!StateLoader.Storage.UserExist(guid))
                {
                    Status.Text = $"Player {guid} not found.";
                    return;
                }

                playerIds.Add(guid);
            }

            if (playerIds.Count > 0)
                filter[PlayerIdsFilter.Name] = playerIds.Select(_ => _.ToString()).ToArray();

            var slFilter = txtSlFilter.Text.Trim();
            if (!string.IsNullOrEmpty(slFilter))
            {
                var slFilterItems = slFilter.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                if(slFilterItems.Length > 0)
                    filter[SharedLogicVersion.Name] = slFilterItems;
            }

            var sysLangs = new List<string>();
            if (cbSysLangOthers.Checked) sysLangs.Add("Others");
            if (cbSysLangEnglish.Checked) sysLangs.Add(UnitySystemLanguage.English.ToString());
            if (cbSysLangRussian.Checked) sysLangs.Add(UnitySystemLanguage.Russian.ToString());
            if (cbSysLangFrench.Checked) sysLangs.Add(UnitySystemLanguage.French.ToString());
            if (cbSysLangItalian.Checked) sysLangs.Add(UnitySystemLanguage.Italian.ToString());
            if (cbSysLangGerman.Checked) sysLangs.Add(UnitySystemLanguage.German.ToString());
            if (cbSysLangSpanish.Checked) sysLangs.Add(UnitySystemLanguage.Spanish.ToString());
            if (cbSysLangJapanese.Checked) sysLangs.Add(UnitySystemLanguage.Japanese.ToString());
            if (cbSysLangKorean.Checked) sysLangs.Add(UnitySystemLanguage.Korean.ToString());
            if (cbSysLangPortuguese.Checked) sysLangs.Add(UnitySystemLanguage.Portuguese.ToString());
            if (cbSysLangChineseTraditional.Checked) sysLangs.Add(UnitySystemLanguage.ChineseTraditional.ToString());

            if (sysLangs.Count < 11)
            {
                filter[SystemLanguageFilter.Name] = sysLangs.ToArray();
            }

            var geoFilter = txtGeoFilter.Text.Trim();
            if (!string.IsNullOrEmpty(geoFilter))
            {
                var geoFilterItems = geoFilter.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                var geoFilterItemsTw = geoFilterItems
                    .Select(_ => ISO3166.Country.List.First(c => c.Name == _.Trim()).TwoLetterCode).ToArray();
                filter[GeoFilter.Name] = geoFilterItemsTw;
            }

            if (rbAbFilter.SelectedValue != "None")
            {
                filter[AbTestingGroupFilter.Name] = new[] {rbAbFilter.SelectedValue};
            }

            var expiry = ReadDate("expiry", DateTime.MaxValue);
            var serverMessage = LocalizedMessageHelper.ToServerMessage(message);
            _broadcastMessageStorage.PushMessage(message.Id, message.DeliveryDate, expiry, serverMessage, filter, message.WelcomeMessage);
        }

        private LocalizedMessageText CreateMessageText(string data, UnitySystemLanguage lang)
        {
            var result = new LocalizedMessageText();
            result.Text = string.Empty;
            var parts = data.Split('\n');
            if (parts.Length > 1)
            {
                result.Title = parts[0].Trim();
                result.Text = string.Join("\n", parts.Skip(1));
            }
            else
            {
                result.Title = data;
            }
            result.Locale = lang;
            return result;
        }

        private bool PackMessage(LocalizedMessage message)
        {
            var hasData = false;
            var localizationList = new List<LocalizedMessageText>();
            var messageText = CreateMessageText(txtMessageEnglish.Text.Trim(), UnitySystemLanguage.English);
            if (!string.IsNullOrEmpty(messageText.Text) || !string.IsNullOrEmpty(messageText.Title))
            {
                localizationList.Add(messageText);
                hasData = true;
            }
            messageText = CreateMessageText(txtMessageRussian.Text.Trim(), UnitySystemLanguage.Russian);
            if (!string.IsNullOrEmpty(messageText.Text) || !string.IsNullOrEmpty(messageText.Title))
            {
                localizationList.Add(messageText);
                hasData = true;
            }
            messageText = CreateMessageText(txtMessageFrench.Text.Trim(), UnitySystemLanguage.French);
            if (!string.IsNullOrEmpty(messageText.Text) || !string.IsNullOrEmpty(messageText.Title))
            {
                localizationList.Add(messageText);
                hasData = true;
            }
            messageText = CreateMessageText(txtMessageItalian.Text.Trim(), UnitySystemLanguage.Italian);
            if (!string.IsNullOrEmpty(messageText.Text) || !string.IsNullOrEmpty(messageText.Title))
            {
                localizationList.Add(messageText);
                hasData = true;
            }
            messageText = CreateMessageText(txtMessageGerman.Text.Trim(), UnitySystemLanguage.German);
            if (!string.IsNullOrEmpty(messageText.Text) || !string.IsNullOrEmpty(messageText.Title))
            {
                localizationList.Add(messageText);
                hasData = true;
            }
            messageText = CreateMessageText(txtMessageSpanish.Text.Trim(), UnitySystemLanguage.Spanish);
            if (!string.IsNullOrEmpty(messageText.Text) || !string.IsNullOrEmpty(messageText.Title))
            {
                localizationList.Add(messageText);
                hasData = true;
            }
            messageText = CreateMessageText(txtMessageKorean.Text.Trim(), UnitySystemLanguage.Korean);
            if (!string.IsNullOrEmpty(messageText.Text) || !string.IsNullOrEmpty(messageText.Title))
            {
                localizationList.Add(messageText);
                hasData = true;
            }
            messageText = CreateMessageText(txtMessageJapanese.Text.Trim(), UnitySystemLanguage.Japanese);
            if (!string.IsNullOrEmpty(messageText.Text) || !string.IsNullOrEmpty(messageText.Title))
            {
                localizationList.Add(messageText);
                hasData = true;
            }
            messageText = CreateMessageText(txtMessagePortuguese.Text.Trim(), UnitySystemLanguage.Portuguese);
            if (!string.IsNullOrEmpty(messageText.Text) || !string.IsNullOrEmpty(messageText.Title))
            {
                localizationList.Add(messageText);
                hasData = true;
            }
            messageText = CreateMessageText(txtMessageChineseTraditional.Text.Trim(), UnitySystemLanguage.ChineseTraditional);
            if (!string.IsNullOrEmpty(messageText.Text) || !string.IsNullOrEmpty(messageText.Title))
            {
                localizationList.Add(messageText);
                hasData = true;
            }

            message.Message = localizationList.ToArray();

            if (!hasData)
            {
                Status.Text = "Message text is empty.";
            }

            return hasData;
        }

        private string DataToSring(byte[] data)
        {
            if (data.All(_ => _ < 128))
                return Encoding.ASCII.GetString(data);
            return new string(data.SelectMany(_ => _.ToString("x")).ToArray());
        }
    }
}