using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections;
using UserProfileViewerTypes;
using Newtonsoft.Json;
using PestelLib.SharedLogicBase;

namespace UserProfileViewerTypes
{
    public class Member
    {
        static object[] noIndex = new object[] { };
        private FieldInfo _fieldInfo;
        private PropertyInfo _propertyInfo;
        private object _obj;
        public Type Type { get; private set; }
        public bool Settable { get
            {
                
                return _fieldInfo != null && !_fieldInfo.IsLiteral 
                    || _propertyInfo != null && _propertyInfo.CanWrite;
            }
        }
        public object Value {
            get
            {
                if (_fieldInfo != null)
                    return _fieldInfo.GetValue(_obj);
                return _propertyInfo.GetValue(_obj, noIndex);
            }
            set
            {
                if (_fieldInfo != null)
                    _fieldInfo.SetValue(_obj, value);
                else if(_propertyInfo.CanWrite)
                    _propertyInfo.SetValue(_obj, value, noIndex);
            }
        }

        public MemberInfo MemberInfo
        {
            get
            {
                if (_propertyInfo != null)
                {
                    return _propertyInfo;
                } else
                {
                    return _fieldInfo;
                }
            }
        }

        public string Name { get; private set; }

        public Member(FieldInfo fieldInfo, object obj)
        {
            _obj = obj;
            _fieldInfo = fieldInfo;
            Type = fieldInfo.FieldType;
            Name = fieldInfo.Name;
        }

        public Member(PropertyInfo propertyInfo, object obj)
        {
            _obj = obj;
            _propertyInfo = propertyInfo;
            Type = propertyInfo.PropertyType;
            Name = propertyInfo.Name;
        }
    }
}

[CustomEditor(typeof(UserProfileViewer))]
public class UserProfileViewerEditor : Editor {
    Dictionary<MethodInfo, object[]> paramValues = new Dictionary<MethodInfo, object[]>();
    Dictionary<string, bool> objectFolds = new Dictionary<string, bool>();

    static Type enumerable = Type.GetType("System.Linq.Enumerable, System.Core");
    static MethodInfo enumerableOfType = enumerable.GetMethod("OfType");
    static MethodInfo enumerableToList = enumerable.GetMethod("ToList");
    static MethodInfo enumerableToArray = enumerable.GetMethod("ToArray");
    private Type[] _moduleTypes;

    private static bool HelpVisible = true;

    private static List<Member> GetMembers(object obj)
    {
        List<Member> fields = new List<Member>();
        var objType = obj.GetType();
        fields.AddRange(objType.GetProperties().Where(p => p.GetIndexParameters().Length == 0).Select(p => new Member(p, obj)));
        fields.AddRange(objType.GetFields().Select(f => new Member(f, obj)));
        return fields;
    }

    private T DrawPrimitive<T>(string Name, object obj, bool disabled, Func<object, T> drawFunc, bool zeroIndent = false)
    {
        var indentLevel = EditorGUI.indentLevel;
        if (zeroIndent)
        {
            EditorGUI.indentLevel = 0;
        }

        EditorGUILayout.BeginHorizontal();
        if (!string.IsNullOrEmpty(Name))
            Label(Name + ":", zeroIndent);
        using (new EditorGUI.DisabledGroupScope(disabled))
        {
            var result = drawFunc(obj);
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel = indentLevel;
            return result;
        }
    }

    private object DrawObject(string Name, object obj, bool disabled, string path = "", bool zeroIndent = false)
    {
        if (obj == null)
        {
            var indentLevel = EditorGUI.indentLevel;
            if (zeroIndent)
            {
                EditorGUI.indentLevel = 0;
            }

            EditorGUILayout.BeginHorizontal();

            if (!string.IsNullOrEmpty(Name))
                Label(Name + ": ", zeroIndent);
            using (new EditorGUI.DisabledGroupScope(disabled))
                EditorGUILayout.LabelField("null");

            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel = indentLevel;
            return null;
        }
        var t = obj.GetType();
        if (t == typeof(int))
        {
            return DrawPrimitive(Name, obj, disabled, (o) => EditorGUILayout.IntField((int)o), zeroIndent);
        }
        if (t == typeof(long))
        {
            return DrawPrimitive(Name, obj, disabled, (o) => EditorGUILayout.LongField((long)o), zeroIndent);
        }
        if (t == typeof(string))
        {
            return DrawPrimitive(Name, obj, disabled, (o) => EditorGUILayout.TextField((string)o), zeroIndent);            
        }
        if (t == typeof(bool))
        {
            return DrawPrimitive(Name, obj, disabled, (o) => EditorGUILayout.Toggle((bool)o), zeroIndent);            
        }
        if (t == typeof(float))
        {
            return DrawPrimitive(Name, obj, disabled, (o) => EditorGUILayout.FloatField((float)o), zeroIndent);
        }
        if (t == typeof(DateTime))
        {
            return DrawPrimitive(Name, obj, disabled, (o) => {
                var date = (DateTime)o;
                EditorGUILayout.BeginHorizontal();
                var year = EditorGUILayout.IntField(date.Year, GUILayout.Width(40));
                var month = EditorGUILayout.IntField(date.Month, GUILayout.Width(20));
                var day = EditorGUILayout.IntField(date.Day, GUILayout.Width(20));
                EditorGUILayout.LabelField("", GUILayout.Width(5));
                var hour = EditorGUILayout.IntField(date.Hour, GUILayout.Width(20));
                EditorGUILayout.LabelField(":", GUILayout.Width(7));
                var minute = EditorGUILayout.IntField(date.Minute, GUILayout.Width(20));
                var second = EditorGUILayout.IntField(date.Second, GUILayout.Width(20));
                var result = new DateTime(year, month, day, hour, minute, second);
                if (GUILayout.Button("Now", GUILayout.Width(40)))
                {
                    result = DateTime.UtcNow;
                }
                EditorGUILayout.EndHorizontal();
                return result;
            }, zeroIndent);
        }
        if (t.IsEnum)
        {
            return EditorGUILayout.EnumPopup(Name + ": ", (Enum)obj);
        }

        if (typeof(IDictionary).IsAssignableFrom(t))
        {
            DrawObject(Name, JsonConvert.SerializeObject(obj), true, path);
            return obj;
        }
        
        if (typeof(IEnumerable).IsAssignableFrom(t))
        {
            var enumType = t.GetGenericArguments().Any() ? t.GetGenericArguments().First() : t.GetElementType();
            var iter = obj as IEnumerable;
            using (new EditorGUI.DisabledGroupScope(disabled))
            {
                using (new GUILayout.VerticalScope())
                {
                    var k = path + Name;
                    if (!objectFolds.ContainsKey(k))
                    {
                        objectFolds[k] = false;
                    }

                    objectFolds[k] = EditorGUILayout.Foldout(objectFolds[k], Name + ": ");

                    if (objectFolds[k])
                    {
                        var count = 0;
                        List<object> values = new List<object>();
                        ++EditorGUI.indentLevel;
                        foreach (var e in iter)
                        {
                            var r = DrawObject(string.Format("{0}[{1}]", Name, count), e, disabled, path + Name + count.ToString(), zeroIndent);
                            values.Add(r);
                            ++count;
                        }
                        if (count == 0)
                        {
                            EditorGUILayout.LabelField("Empty");
                        }
                        --EditorGUI.indentLevel;
                        
                        if (t.IsArray)
                        {
                            var ofTypeInst = enumerableOfType.MakeGenericMethod(enumType);
                            var toArrayInst = enumerableToArray.MakeGenericMethod(enumType);
                            var r = ofTypeInst.Invoke(null, new object[] { values });
                            r = toArrayInst.Invoke(null, new object[] { r });
                            return Convert.ChangeType(r, t);
                        }
                        else if (typeof(IList).IsAssignableFrom(t))
                        {
                            var ofTypeInst = enumerableOfType.MakeGenericMethod(enumType);
                            var toListInst = enumerableToList.MakeGenericMethod(enumType);
                            var r = ofTypeInst.Invoke(null, new object[] { values });
                            r = toListInst.Invoke(null, new object[] { r });
                            return r;
                        }
                    }
                    else 
                    {
                        return obj;
                    }
                }
            }
            //TODO
            return null;
        }
        var objType = obj.GetType();
        var fold = objType.GetProperties().Any() || objType.GetFields().Any();
        if (!fold)
        {
            EditorGUILayout.LabelField("  " + Name);
            return null;
        }
        var key = path + objType.FullName;
        if (!objectFolds.TryGetValue(key, out fold))
            objectFolds[key] = false;
        objectFolds[key] = EditorGUILayout.Foldout(objectFolds[key], Name);
        if (objectFolds[key])
        {
            List<Member> fields = GetMembers(obj);
            GUILayout.BeginVertical();
            ++EditorGUI.indentLevel;
            for (int j = 0; j < fields.Count; j++)
            {
                var p = fields[j];
                TryToShowHelp(p.MemberInfo);
                var r = DrawObject(p.Name, p.Value, disabled, key, zeroIndent);
                if(p.Settable && !disabled)
                    p.Value = r;
            }
            --EditorGUI.indentLevel;
            GUILayout.EndVertical();
        }
        return obj;
    }

    private static object GetDefaultValue(Type t)
    {
        if (t == typeof(byte))
            return (byte)0;
        if (t == typeof(sbyte))
            return (sbyte)0;
        if (t == typeof(char))
            return (char)0;
        if (t == typeof(decimal))
            return 0M;
        if (t == typeof(double))
            return 0.0;
        if (t == typeof(float))
            return 0f;
        if (t == typeof(uint))
            return (uint)0;
        if (t == typeof(int))
            return 0;
        if (t == typeof(long))
            return 0L;
        if (t == typeof(ulong))
            return 0UL;
        if (t == typeof(short))
            return (short)0;
        if (t == typeof(ushort))
            return (ushort)0;
        if (t == typeof(string))
            return "";
        if (t == typeof(bool))
            return false;
        if (t == typeof(DateTime))
            return DateTime.UtcNow;
        if (t.IsEnum)
        {
            foreach (var v in Enum.GetValues(t))
            {
                return v;
            }
        }
        var ctor = t.GetConstructor(Type.EmptyTypes);
        if(ctor == null)
            return null;
        var obj = ctor.Invoke(new object[] { });
        var fields = GetMembers(obj);
        for (var i = 0; i < fields.Count; ++i)
        {
            var r = GetDefaultValue(fields[i].Type);
            if (fields[i].Settable)
                fields[i].Value = r;
        }
        return obj;
    }

    private bool DrawMethod(object ctx, MethodInfo method)
    {
        var refresh = false;
        var commandParams = method.GetParameters();
        if (!paramValues.ContainsKey(method)) paramValues.Add(method, new object[commandParams.Length]);

        TryToShowHelp(method);

        EditorGUILayout.BeginHorizontal();
        if (Button(method.Name))
        {
            var paramsToPass = new object[commandParams.Length];
            for (int p = 0; p < commandParams.Length; p++)
            {
                if (paramValues[method][p] is List<string>)
                {
                    paramsToPass[p] = ((List<string>)paramValues[method][p]).ToArray();
                }
                else
                {
                    paramsToPass[p] = paramValues[method][p];
                }
            }

            if (method.ReturnType == typeof(void))
            {
                method.Invoke(ctx, paramsToPass);
                Debug.Log("Method " + method.Name + " was called");
            }
            else
            {
                var r = method.Invoke(ctx, paramsToPass);
                var pStr = JsonConvert.SerializeObject(paramsToPass);
                var rStr = JsonConvert.SerializeObject(r);
                var msg = string.Format("Call {0} ({1}) returns {2}", method.Name, pStr, rStr);
                Debug.Log(msg);
            }
            refresh = true;
        }

        for (int j = 0; j < commandParams.Length; j++)
        {
            var p = commandParams[j];
            if (paramValues[method][j] == null)
            {
                try
                {
                    paramValues[method][j] = GetDefaultValue(p.ParameterType);
                }
#pragma warning disable 168
                catch (Exception e)
#pragma warning restore 168
                {
                    paramValues[method][j] = null;
                }
            }
            if (paramValues[method][j] != null)
                paramValues[method][j] = (object)DrawObject(p.Name, paramValues[method][j], false, "Methods/", true);
            else
            {
                EditorGUILayout.LabelField(p.ParameterType.Name + " not supported", GUILayout.ExpandWidth(false));
            }
        }

        EditorGUILayout.EndHorizontal();

        return refresh;
    }
    
    private bool DrawModule(Dictionary<object, List<MethodInfo>> modulesPublicMethods)
    {
        var refresh = false;
        foreach (var kv in modulesPublicMethods)
        {
            var moduleName = kv.Key.GetType().Name;

            if (Fold("sharedlogicmodule_" + moduleName, moduleName))
            {
                EditorGUI.indentLevel++;
                var slModule = kv.Key as ISharedLogicModule;
                var stateField = slModule.GetType().GetField("State", BindingFlags.NonPublic | BindingFlags.Instance);
                var stateValue = stateField.GetValue(slModule);
                DrawObject("State", stateValue, true, stateValue.GetType().Name);

                if (Fold("sharedlogicmodulemethods_" + moduleName, "Methods"))
                {
                    EditorGUI.indentLevel++;
                    foreach (var m in kv.Value)
                    {
                        refresh |= DrawMethod(kv.Key, m);
                    }
                    EditorGUI.indentLevel--;
                }

                var mType = _moduleTypes.FirstOrDefault(x => x.Name == moduleName);
                if (mType != null)
                {
                    var commands = mType.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => !m.Name.StartsWith("Debug")).ToArray();
                    
                    if (commands.Length > 0 && Fold("sharedlogicmodulecommands_" + moduleName, "Commands"))
                    {
                        EditorGUI.indentLevel++;
                        foreach (var c in commands)
                        {
                            if (DrawMethod(null, c))
                            {
                                refresh = true;
                            }
                        }
                        EditorGUI.indentLevel--;
                    }

                    var debug = mType.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => m.Name.StartsWith("Debug")).ToArray();
                    
                    if (debug.Length > 0 && Fold("sharedlogicmoduledebug_" + moduleName, "Debug"))
                    {
                        EditorGUI.indentLevel++;
                        foreach (var c in debug)
                        {
                            if (DrawMethod(null, c))
                            {
                                refresh = true;
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                }


                EditorGUI.indentLevel--; 
            }
        }

        return refresh;
    }

    private static Dictionary<string, Type> _typesCache;

    private Type FindType(string type)
    {
        if (_typesCache == null)
        {
            _typesCache = new Dictionary<string, Type>();
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in a.GetTypes())
                {
                    if (string.IsNullOrEmpty(t.FullName)) continue;
                    _typesCache[t.FullName] = t;
                }
            }
        }

        if (_typesCache.ContainsKey(type))
        {
            return _typesCache[type];
        }
        else
        {
            return null;
        }
    }

	public override void OnInspectorGUI() {
		if (!Application.isPlaying) return;

        var viewer = target as UserProfileViewer;
        base.OnInspectorGUI();

        if (Button("Refresh"))
        {
            viewer.Refresh();
        }

        HelpVisible = EditorGUILayout.ToggleLeft("Help visible", HelpVisible);

        var sharedLogicCommand = FindType("PestelLib.SharedLogicClient.SharedLogicCommand");
        if (sharedLogicCommand == null)
        {
            throw new Exception("PestelLib.SharedLogicClient.SharedLogicCommand not found. Run UpdateCommandHelper");
        }
        _moduleTypes = sharedLogicCommand.GetNestedTypes();
        
	    var refresh = DrawModule(viewer.ModulesPublicMethods);

        if (refresh)
	    {
	        viewer.Refresh();
	        this.Repaint();
        }
	}

    public static bool Button(string label, GUIStyle style = null)
    {
        return style == null ?
            GUI.Button(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()), label) :
            GUI.Button(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()), label, style);
    }

    public static bool Fold(string key, string visibleLabel)
    {
        var fold = EditorGUILayout.Foldout(EditorPrefs.GetBool(key, false), visibleLabel);
        EditorPrefs.SetBool(key, fold);
        return fold;
    }

    public static void Label(string text, bool zeroIndent)
    {
        if (!zeroIndent)
        {
            EditorGUILayout.LabelField(new GUIContent(text), "label");
        }
        else
        {
            Rect labelRect = GUILayoutUtility.GetRect(new GUIContent(text), "label");
            var aligment = GUI.skin.label.alignment;
            GUI.skin.label.alignment = TextAnchor.MiddleRight;
            GUI.Label(labelRect, text);
            GUI.skin.label.alignment = aligment;
        }
    }

    public static void HelpBox(string text)
    {
        if (!string.IsNullOrEmpty(text) && HelpVisible)
        {
            var helpStyle = new GUIStyle(GUI.skin.box);
            helpStyle.wordWrap = true;
            helpStyle.alignment = TextAnchor.UpperLeft;
            EditorGUILayout.LabelField(text, helpStyle, GUILayout.ExpandWidth(true));
        }
    }

    private static void TryToShowHelp(MemberInfo member)
    {
        var attributes = member.GetCustomAttributes(typeof(SharedCommand), false);
        if (attributes.Length > 0)
        {
            var attr = (SharedCommand)attributes[0];
            HelpBox(attr.Description);
        }
        attributes = member.GetCustomAttributes(typeof(Description), false);
        if (attributes.Length > 0)
        {
            var attr = (Description)attributes[0];
            HelpBox(attr.Text);
        }
    }
}