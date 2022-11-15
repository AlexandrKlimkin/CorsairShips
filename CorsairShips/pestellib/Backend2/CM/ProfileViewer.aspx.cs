using PestelLib.SharedLogicBase;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using BackendCommon.Code.Data;
using log4net;
using MessagePack;
using Newtonsoft.Json;
using S;
using BackendCommon.Code.Utils;
using UnityDI;
using BackendCommon.Code;

namespace Backend2.Admin
{
    public class HtmlEscape
    {
        public static string EscapeTags(string value)
        {
            return value.Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }

        public static string RollbackTags(string value)
        {
            return value
                .Replace("&lt;", "<")
                .Replace("&gt;", ">");
        }
    }

    public struct ModuleState
    {
        public Type ModuleT;
        public Type StateT;
        public string JsonState;
        public byte[] RawState;
    }

    public static class SharedModulesLoader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SharedModulesLoader));

        private static Type objType = typeof(object);
        private static Type sharedLogicModuleT = typeof(ISharedLogicModule);
        private static MethodInfo messagePackDeserializeMethod;
        public static ModuleState[] SharedLogicModules { get; private set; }

        static SharedModulesLoader()
        {
            var fileName = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\PestelLib.ConcreteSharedLogic.dll";
            if (!File.Exists(fileName))
            {
                return;
            }

            messagePackDeserializeMethod = typeof(MessagePackSerializer).GetMethod("Deserialize", new Type[] { typeof(byte[]) });
            var types = new List<ModuleState>();

            var assembly = Assembly.LoadFrom(fileName);
            foreach (var t in assembly.GetTypes())
            {
                if (!sharedLogicModuleT.IsAssignableFrom(t))
                    continue;
                try
                {
                    var stateT = GetFirstParent(t).GenericTypeArguments.First();
                    var ms = new ModuleState
                    {
                        ModuleT = t,
                        StateT = stateT
                    };
                    types.Add(ms);
                }
                catch
                {
                }
            }

            SharedLogicModules = types.ToArray();
        }

        private static Type GetFirstParent(Type t)
        {
            while (t.BaseType != objType)
            {
                t = t.BaseType;
            }

            return t;
        }

        public static object MessagePackDeserialize(string moduleName, byte[] state)
        {
            ModuleState moduleState;
            try
            {
                moduleState = SharedLogicModules.First(_ => _.ModuleT.Name == moduleName);
            }
            catch (ArgumentNullException)
            {
                log.Error("Wrong argument for module: " + moduleName + " state is null=" + (state == null) + " json: " + JsonConvert.SerializeObject(SharedLogicModules));
                return null;
                //throw new Exception("Wrong argument for module: " + moduleName + " state is null=" + (state == null));
            }

            var method = messagePackDeserializeMethod.MakeGenericMethod(moduleState.StateT);
            var result = method.Invoke(null, new[] { state });
            return result;
        }

        public static ModuleState GetModule(string name)
        {
            return SharedLogicModules.FirstOrDefault(_ => _.ModuleT.Name == name);
        }
    }

    public partial class ProfileViewer : System.Web.UI.Page
    {
        public Dictionary<string, string> _state;
        public static UserStorage _storage = new UserStorage();
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() {TypeNameHandling = TypeNameHandling.Auto};

        public ProfileViewer()
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack && ViewState["UserId"] != null)
            {
                PlayerIdBox.Text = ViewState["UserId"].ToString();
                LoadProfile();
            }
            
            LoadButton.Click += (o, args) => { LoadProfile(); };
            SaveButton.Click += (o, args) => { SaveProfile(); };

            SaveDiv.Visible = false;
            ErrorDiv.Visible = false;

            if (this.Request.QueryString.AllKeys.Contains("UserId"))
            {
                ViewState["UserId"] = Request.QueryString["UserId"];
                PlayerIdBox.Text = Request.QueryString["UserId"];
                LoadProfile();
            }
        }
        
        private void SaveProfile()
        {
            _populateTable = false;
            ViewState["UserId"] = null;

            Dictionary<string, ModuleState> states = new Dictionary<string, ModuleState>();

            foreach (TableRow row in Table1.Rows)
            {
                var label = (Label)row.Cells[0].Controls[0];
                var textField = (TextBox)row.Cells[1].Controls[0];

                var module = SharedModulesLoader.GetModule(label.Text.Replace("&nbsp", ""));
                if (module.ModuleT != default(Type))
                {
                    module.JsonState = HtmlEscape.RollbackTags(textField.Text);
                    states[label.Text.Replace("&nbsp", "")] = module;
                }
            }

            var stateRaw = _storage.GetUserState(PlayerId);
            var state = MessagePackSerializer.Deserialize<UserProfile>(stateRaw);
            if (SaveToPlayerId != PlayerId)
                _storage.LockPlayer(SaveToPlayerId, LockPeriod);
            state.UserId = SaveToPlayerId.ToByteArray();
            state.SharedLogicVersion = 0;
            foreach (var kv in states)
            {
                if (!state.ModulesDict.ContainsKey(kv.Key))
                {
                    Status.Text += $"Original profile {PlayerId} dont contain state for module {kv.Key}\n";
                }
                var m = kv.Value;
                var objState = JsonConvert.DeserializeObject(m.JsonState, m.StateT, jsonSerializerSettings);
                try
                {
                    m.RawState = MessagePackSerializer.Serialize(objState);
                }
                catch (Exception e)
                {
                    throw new Exception($"While serialization of {kv.Key} state", e);
                }
                state.ModulesDict[kv.Key] = m.RawState;
            }

            stateRaw = MessagePackSerializer.Serialize(state);
            try
            {
                _storage.SaveRawState(SaveToPlayerId, stateRaw);
            }
            catch { }

            Status.Text = $"Profile {SaveToPlayerId} successfully saved";
            _state = null;
            _storage.UnlockPlayer(PlayerId);
            if (SaveToPlayerId != PlayerId)
                _storage.UnlockPlayer(SaveToPlayerId);

            LoadDiv.Visible = false;
            SaveDiv.Visible = false;
            SaveToPlayerIdBox.Text = "";
        }

        private void LoadProfile()
        {
            ViewState["UserId"] = PlayerId;
            _populateTable = true;

            LockExpire.Text = (DateTime.Now + LockPeriod).ToLongTimeString();
            PlayerId2.Text = PlayerId.ToString();

            if (string.IsNullOrEmpty(SaveToPlayerIdBox.Text))
            {
                SaveToPlayerIdBox.Text = PlayerId.ToString();
            }

            _state = new Dictionary<string, string>();
            _storage.LockPlayer(PlayerId, LockPeriod);
            var state = _storage.GetUserState(PlayerId);
            if (state == null)
            {
                ErrorDesc.Text = $"Player with id '{PlayerIdBox.Text}' not found.";
                ErrorDiv.Visible = true;
                ViewState["UserId"] = null;
                return;
            }

            ErrorDiv.Visible = false;
            LoadDiv.Visible = false;
            SaveDiv.Visible = true;

            Table1.Rows.Clear();
            var profile = MessagePackSerializer.Deserialize<UserProfile>(state);
            var stateFactory = ContainerHolder.Container.Resolve<DefaultStateFactory>();
            if (stateFactory.StateVersion < profile.Version)
            {
                ErrorDesc.Text = $"Player with id '{PlayerIdBox.Text}' has state version {profile.Version}, backend supported version is {stateFactory.StateVersion}. Try use more recent version.";
                ErrorDiv.Visible = true;
                ViewState["UserId"] = null;
                return;
            }
            else if (stateFactory.StateVersion > profile.Version)
            {
                if (!UpdateState.Checked)
                {
                    ErrorDesc.Text = $"Player with id '{PlayerIdBox.Text}' has state version {profile.Version}, backend actual version is {stateFactory.StateVersion}. You can try to load state with older backend or update user state to current backend state version (check 'Force update user state').";
                    ErrorDesc.Text += $"User cant load updated state with old client!";
                    ErrorDiv.Visible = true;
                    ViewState["UserId"] = null;
                    return;
                }
                var logic = MainHandlerBase.ConcreteGame.SharedLogic(state, MainHandlerBase.FeaturesCollection);
                _storage.SaveRawState(logic.PlayerId, logic.SerializedState);
                profile = MessagePackSerializer.Deserialize<UserProfile>(logic.SerializedState);
            }
            foreach (var kv in profile.ModulesDict)
            {
                try
                {
                    var o = SharedModulesLoader.MessagePackDeserialize(kv.Key, kv.Value);

                    _state[kv.Key] =
                        HtmlEscape.EscapeTags(JsonConvert.SerializeObject(o, Formatting.Indented, jsonSerializerSettings));

                    TextField(kv.Key, 1, kv.Key, _state[kv.Key], false);
                }
                catch
                {
                }
            }

            _populateTable = false;
        }

        private void TextField(string path, int offset, string label, string val, bool readOnly = false)
        {
            var row = new TableRow();
            var color = Table1.Rows.Count % 2 == 0 ? Color.White : Color.GhostWhite;
            row.BackColor = color;

            Table1.Rows.Add(row);

            var cell1 = new TableCell();

            row.Cells.Add(cell1);
            cell1.Controls.Add(new Label() { Text = Tabs(offset) + label });

            var cell2 = new TableCell();
            row.Cells.Add(cell2);
            var textBox = new TextBox()
            {
                Text = val,
                Width = Unit.Percentage(100),
                ReadOnly = readOnly,
                BackColor = readOnly ? Color.Gray : Color.White,
                //ClientIDMode = ClientIDMode.Predictable,
                ID = path + "/" + label,
                TextMode = TextBoxMode.MultiLine,
                Rows = val.Split('\n').Length
            };
            if (!_populateTable)
            {
                textBox.Text = "";
            }
            cell2.Controls.Add(textBox);

        }

        private bool _populateTable = false;

        static string Tabs(int n)
        {
            n *= 5;
            var space = "&nbsp";
            return new StringBuilder(space.Length * n).Insert(0, space, n).ToString();
        }

        private Guid SaveToPlayerId
        {
            get
            {
                Guid playerId;
                if (!Guid.TryParse(SaveToPlayerIdBox.Text, out playerId))
                {
                    return Guid.Empty;
                }

                return playerId;
            }
        }

        private Guid PlayerId => PlayerIdHelper.FromString(PlayerIdBox.Text);

        private TimeSpan LockPeriod
        {
            get
            {
                TimeSpan lockPeriod;
                if (!TimeSpan.TryParse(LockPeriodBox.Text, out lockPeriod))
                {
                    return TimeSpan.FromMinutes(15);
                }

                return lockPeriod;
            }
        }
    }
}