using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PestelLib.ClientConfig;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using PestelLib.ClientConfig.Editor.Sources;
using System.Reflection;
using System.Text;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

namespace PestelLib.BuildUtils
{
    public class BatchBuild
    {
        public static bool TryParseEnum<TEnum>(string strEnumValue, out TEnum result)
        {
            var values = (TEnum[])Enum.GetValues(typeof(TEnum));
            if (!values.Select(x => x.ToString().ToLowerInvariant()).Contains(strEnumValue.ToLowerInvariant()))
            {
                result = default(TEnum);
                return false;
            }

            result = (TEnum)Enum.Parse(typeof(TEnum), strEnumValue, true);
            return true;
        }

        private static bool BuildAddressables(string builderName)
        {
#if UNITY_2017_4_OR_NEWER
            try
            {
                var t = Type.GetType("UnityEditor.AddressableAssets.Build.AddressablesBuildScriptHooks, Unity.Addressables.Editor");
                var tAddressableAssetSettings = Type.GetType("UnityEditor.AddressableAssets.Settings.AddressableAssetSettings, Unity.Addressables.Editor");
                var tAddressableAssetSettingsDefaultObject = Type.GetType("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject, Unity.Addressables.Editor");
                var tAddressableAssetSettingsDefaultObjectSettings = tAddressableAssetSettingsDefaultObject.GetProperty("Settings", BindingFlags.Static | BindingFlags.Public);
                var settings = tAddressableAssetSettingsDefaultObjectSettings.GetValue(tAddressableAssetSettingsDefaultObject);
                if (settings == null)
                {
                    Debug.LogError("BuildAddressables. Get Settings failed.");
                    return false;
                }
                object dataBuilder = null;
                if (string.IsNullOrEmpty(builderName))
                {
                    var tActivePlayModeDataBuilder = settings.GetType().GetProperty("ActivePlayModeDataBuilder", BindingFlags.Instance | BindingFlags.Public);
                    dataBuilder = tActivePlayModeDataBuilder.GetValue(settings);
                    if (dataBuilder == null)
                    {
                        Debug.LogError("BuildAddressables. Active play mode build script is null.");
                        return false;
                    }
                }
                else
                {
                    var tSettingsDataBuilders = settings.GetType().GetProperty("DataBuilders", BindingFlags.Instance | BindingFlags.Public);
                    var buildersList = (List<ScriptableObject>)tSettingsDataBuilders.GetValue(settings);
                    foreach (var b in buildersList)
                    {
                        var tBuilderGetName = b.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
                        var bName = (string)tBuilderGetName.GetValue(b);
                        if (bName == builderName)
                        {
                            dataBuilder = b;
                            break;
                        }
                    }
                    if (dataBuilder == null)
                    {
                        Debug.LogError($"BuildAddressables. Builder {builderName} not found.");
                        return false;
                    }
                }
                object result = null;
                var tCanBuildData = dataBuilder.GetType().GetMethod("CanBuildData", BindingFlags.Instance | BindingFlags.Public);
                {
                    var tCanBuildDataAddressablesPlayModeBuildResult = tCanBuildData.MakeGenericMethod(Type.GetType("UnityEditor.AddressableAssets.Build.AddressablesPlayModeBuildResult, Unity.Addressables.Editor"));
                    var canBuild = (bool)tCanBuildDataAddressablesPlayModeBuildResult.Invoke(dataBuilder, new object[] { });
                    if (!canBuild)
                    {
                        Debug.LogError($"BuildAddressables. Active build script {dataBuilder} cannot build AddressablesPlayModeBuildResult.");

                    }
                    else
                    {
                        var tBuildData = dataBuilder.GetType().GetMethod("BuildData", BindingFlags.Instance | BindingFlags.Public);
                        var tBuildDataAddressablesPlayModeBuildResult = tBuildData.MakeGenericMethod(Type.GetType("UnityEditor.AddressableAssets.Build.AddressablesPlayModeBuildResult, Unity.Addressables.Editor"));
                        var tAddressablesDataBuilderInput = Type.GetType("UnityEditor.AddressableAssets.Build.AddressablesDataBuilderInput, Unity.Addressables.Editor");
                        var tAddressablesDataBuilderInputCtor = tAddressablesDataBuilderInput.GetConstructor(new[] { Type.GetType("UnityEditor.AddressableAssets.Settings.AddressableAssetSettings, Unity.Addressables.Editor") });
                        result = tBuildDataAddressablesPlayModeBuildResult.Invoke(dataBuilder, new object[] { tAddressablesDataBuilderInputCtor.Invoke(new object[] { settings }) });
                    }
                }
                if (result == null)
                {
                    var tCanBuildDataAddressablesPlayerBuildResult = tCanBuildData.MakeGenericMethod(Type.GetType("UnityEditor.AddressableAssets.Build.AddressablesPlayerBuildResult, Unity.Addressables.Editor"));
                    var canBuild = (bool)tCanBuildDataAddressablesPlayerBuildResult.Invoke(dataBuilder, new object[] { });
                    if (!canBuild)
                    {
                        Debug.LogError($"BuildAddressables. Active build script {dataBuilder} cannot build AddressablesPlayerBuildResult nor AddressablesPlayModeBuildResult.");
                        return false;
                    }
                    else
                    {
                        var tBuildData = dataBuilder.GetType().GetMethod("BuildData", BindingFlags.Instance | BindingFlags.Public);
                        var tBuildDataAddressablesPlayerBuildResult = tBuildData.MakeGenericMethod(Type.GetType("UnityEditor.AddressableAssets.Build.AddressablesPlayerBuildResult, Unity.Addressables.Editor"));
                        var tAddressablesDataBuilderInput = Type.GetType("UnityEditor.AddressableAssets.Build.AddressablesDataBuilderInput, Unity.Addressables.Editor");
                        var tAddressablesDataBuilderInputCtor = tAddressablesDataBuilderInput.GetConstructor(new[] { Type.GetType("UnityEditor.AddressableAssets.Settings.AddressableAssetSettings, Unity.Addressables.Editor") });
                        result = tBuildDataAddressablesPlayerBuildResult.Invoke(dataBuilder, new object[] { tAddressablesDataBuilderInputCtor.Invoke(new object[] { settings }) });
                    }
                }
                var tAddressableAssetBuildResultError = result.GetType().GetProperty("Error", BindingFlags.Instance | BindingFlags.Public);
                var error = (string)tAddressableAssetBuildResultError.GetValue(result);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"BuildAddressables. Build error {error}.");
                    return false;
                }
                var tBuildScript = Type.GetType("UnityEditor.AddressableAssets.Build.BuildScript, Unity.Addressables.Editor");
                var tBuildScriptCompletedAction = tBuildScript.GetField("buildCompleted", BindingFlags.Static | BindingFlags.Public);
                var completedAction = tBuildScriptCompletedAction.GetValue(null);
                var tSettingsDataBuilderCompleted = settings.GetType().GetMethod("DataBuilderCompleted", BindingFlags.Instance | BindingFlags.NonPublic);
                tSettingsDataBuilderCompleted.Invoke(settings, new object[] { dataBuilder, result });

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("error: " + e);
            }
#endif
            return false;
        }

        private static void DeviceMultiTarget(string targetDeviceArg)
        {
            var sb = PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup);
#if UNITY_2017_4_OR_NEWER
            var fat = false;
            var targetDevices = targetDeviceArg.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.None;
            foreach (var targetDevice in targetDevices)
            {
                if (targetDevice == "FAT")
                {
                    fat = true;
                    PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
                    if (sb == ScriptingImplementation.IL2CPP)
                    {
                        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
                    }
                    else
                    {
                        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.X86;
                    }
                    break;
                }

                AndroidArchitecture androidTargetDevice;
                if (TryParseEnum(targetDevice, out androidTargetDevice))
                {
                    PlayerSettings.Android.targetArchitectures |= androidTargetDevice;
                }
            }
			// https://docs.unity3d.com/ScriptReference/PlayerSettings.Android-bundleVersionCode.html
			PlayerSettings.Android.buildApkPerCpuArchitecture = !fat && PlayerSettings.Android.bundleVersionCode < 100000 && !EditorUserBuildSettings.buildAppBundle;
#endif
        }

        [MenuItem("PestelCrew/Perform Build")]
        public static void PerformBuild()
        {
            Debug.Log("Perform Build");

            var opts = BuildOptions.None;
            Dictionary<string, List<string>> args = ParseArguments();

            if (args.ContainsKey("-scriptingBackend"))
            {
                var v = args["-scriptingBackend"][0];
                ScriptingImplementation simp;
                if (TryParseEnum(v, out simp))
                {
                    Debug.Log("Set scripting backend to " + simp);
                    PlayerSettings.SetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup, simp);
                }
            }

            string buildPath;
            if (args.ContainsKey("-customBuildPath"))
            {
                buildPath = args["-customBuildPath"][0];
            }
            else
            {
                buildPath = Path.Combine(Application.dataPath, "..");
                buildPath = Path.Combine(buildPath, "local_build");
            }

            string buildFilePath = "";
            string bundleId = "";
            string targetDevice = "";
            if (args.ContainsKey("-customBundleId")) {
                bundleId = args["-customBundleId"][0];
            }

            if (args.ContainsKey("-customBuildFilePath"))
            {
                buildFilePath = args["-customBuildFilePath"][0];
            }

            if (args.ContainsKey("-showSplash"))
            {
                PlayerSettings.SplashScreen.show = args["-showSplash"][0] == "True";
            }

            if (args.ContainsKey("-unitySplash"))
            {
                PlayerSettings.SplashScreen.showUnityLogo = args["-unitySplash"][0] == "True";
            }

#if UNITY_2018_3_OR_NEWER
            if (args.ContainsKey("-buildAppBundle"))
            {
                EditorUserBuildSettings.buildAppBundle = string.Equals(args["-buildAppBundle"][0], "True", StringComparison.InvariantCultureIgnoreCase);
                Debug.LogFormat("EditorUserBuildSettings.buildAppBundle: {0}", EditorUserBuildSettings.buildAppBundle);
            }
#endif

            PlayerSettings.SplashScreen.showUnityLogo = false;

			if (args.ContainsKey("-buildNumber")) {
                string buildNumberStr = args["-buildNumber"][0];
                PlayerSettings.iOS.buildNumber = buildNumberStr;
                int buildNumber;
                if(int.TryParse(buildNumberStr, out buildNumber))
                    PlayerSettings.Android.bundleVersionCode = buildNumber;
            }
			
            if (args.ContainsKey("-targetDevice"))
            {
                targetDevice = args["-targetDevice"][0];
#pragma warning disable 618
#if UNITY_2017_4_OR_NEWER
                DeviceMultiTarget(targetDevice);
#else
                AndroidTargetDevice androidTargetDevice;
                if (TryParseEnum(targetDevice, out androidTargetDevice))
                {
                    PlayerSettings.Android.targetDevice = androidTargetDevice;
                }
#endif
#pragma warning restore 618
                iOSTargetDevice iosTargetDevice;
                if (TryParseEnum(targetDevice, out iosTargetDevice))
                {
                    PlayerSettings.iOS.targetDevice = iosTargetDevice;
                }
            }

            
            if (args.ContainsKey("-developmentBuild"))
            {
                EditorUserBuildSettings.development = true;
                opts |= BuildOptions.Development;
            }

            if (args.ContainsKey("-configName")) {
                var config = ConfigEditor.FindConfig();
                ConfigEditor.LoadConfig(args["-configName"][0], config);
            }

            string configClass = null;
            string configOverride = null;
            string configFile = null;

            if(args.ContainsKey("-configClass"))
                configClass = args["-configClass"][0];

            if (args.ContainsKey("-configOverride"))
                configOverride = args["-configOverride"][0];

            if (args.ContainsKey("-configFile"))
                configFile = args["-configFile"][0];

            if(configClass != null)
                OverrideConfig(configClass, configOverride, configFile);

            if (args.ContainsKey("-buildSystem"))
            {
                AndroidBuildSystem androidBuildSystem;
                if (TryParseEnum(args["-buildSystem"][0], out androidBuildSystem))
                {
                    EditorUserBuildSettings.androidBuildSystem = androidBuildSystem;
                }
            }

            if (args.ContainsKey("-androidObb"))
            {
                PlayerSettings.Android.useAPKExpansionFiles = args["-androidObb"][0] == "True";
            }

            if (string.IsNullOrEmpty(buildPath) && string.IsNullOrEmpty(buildFilePath))
            {
                Debug.LogError("No build path setted");
                return;
            }

            string[] scenes = null;
            if (args.ContainsKey("-scenes"))
                scenes = args["-scenes"].ToArray();

            Debug.Log("Gonna build for " + EditorUserBuildSettings.activeBuildTarget);

            bool isLocalStateChanged = SetLocalState(false);
            //bool defaultGpuSkinning = PlayerSettings.gpuSkinning;
            string defaultBundleId = PlayerSettings.applicationIdentifier;

            string extension = "";

            if (!string.IsNullOrEmpty(bundleId))
            {
                PlayerSettings.applicationIdentifier = bundleId;
                Debug.Log("BundleIdentifier setted to " + PlayerSettings.applicationIdentifier);
            }

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
#if UNITY_2019_1_OR_NEWER
                PlayerSettings.Android.useCustomKeystore = true;
#endif
                if (args.ContainsKey("-androidKeyaliasName"))
                    PlayerSettings.Android.keyaliasName = args["-androidKeyaliasName"][0];
                if (args.ContainsKey("-androidKeyaliasPass"))
                    PlayerSettings.Android.keyaliasPass = args["-androidKeyaliasPass"][0];
                if (args.ContainsKey("-androidKeystoreName"))
                    PlayerSettings.Android.keystoreName = args["-androidKeystoreName"][0];
                if (args.ContainsKey("-androidKeystorePass"))
                    PlayerSettings.Android.keystorePass = args["-androidKeystorePass"][0];

                extension = ".apk";
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                //Something for ios
            }

            var defSymbs = new Defines();
            if (args.ContainsKey("-overrideDefines"))
            {
                if (args["-overrideDefines"][0] == "True")
                {
                    defSymbs.Clear();
                }
            }

            List<string> list;
            if (args.TryGetValue("-addDefines", out list))
            {
                if (list.Count > 0)
                {
                    defSymbs.Add(list.ToArray());
                }
            }

            if (args.ContainsKey("-incrementVersion"))
                IncrementVersion();

            string path;
            if (!string.IsNullOrEmpty(buildFilePath))
                path = buildFilePath;
            else
                path = string.Format(buildPath + Path.DirectorySeparatorChar + "{0}" + extension, PlayerSettings.bundleVersion);

            if (args.ContainsKey("-useResolver") && EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                var resolver = Type.GetType("GooglePlayServices.PlayServicesResolver, Google.JarResolver");
                if (resolver != null)
                {
                    Debug.Log("Trying to force resolve android deps");
                    var method = resolver.GetMethod("MenuForceResolve", new Type[] { });
                    if (method != null)
                        method.Invoke(null, new object[] { });
                }
                else
                    Debug.Log("GooglePlayServices.PlayServicesResolver not found");
            }

#if UNITY_2018_3_OR_NEWER
            if (PlayerSettings.Android.buildApkPerCpuArchitecture)
                path = Path.GetDirectoryName(path);
#endif

            if (args.ContainsKey("-buildAddressables"))
            {
                var addressablesBuild = BuildAddressables(args["-buildAddressables"][0]);
                if (!addressablesBuild)
                {
                    Debug.LogError("Adressables build failed!");
                    return;
                }
            }

#if UNITY_2018_1_OR_NEWER
            BuildReport buildReport;
            if (scenes != null)
                buildReport = BuildPipeline.BuildPlayer(scenes, path, EditorUserBuildSettings.activeBuildTarget, opts);
            else
                buildReport = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, path, EditorUserBuildSettings.activeBuildTarget, opts);

            if (buildReport.summary.result != BuildResult.Succeeded)
            {
                string errors = "";
                if (buildReport.steps.Length > 0)
                {
                    errors =  string
                        .Join("\n", 
                            buildReport.steps
                                .Where(_ => _.messages.Length > 0)
                                .SelectMany(_ => _.messages)
                                .Where(_ => _.type != LogType.Log && _.type != LogType.Warning)
                                .Select(_ => _.content));
                    StringBuilder sb = new StringBuilder(errors);
                    sb.Replace("'", "|'");
                    sb.Replace("\n", "|n");
                    sb.Replace("\r", "|r");
                    sb.Replace("|", "||");
                    sb.Replace("[", "|[");
                    sb.Replace("]", "|]");
                    errors = sb.ToString();
                }

                throw new Exception($"##teamcity[message text='Unity build failed.'  status='FAILURE' errorDetails='{errors}']");
            }
#else
            if (scenes != null)
                BuildPipeline.BuildPlayer(scenes, path, EditorUserBuildSettings.activeBuildTarget, opts);
            else
                BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, path, EditorUserBuildSettings.activeBuildTarget, opts);
#endif

            if (isLocalStateChanged)
                SetLocalState(true);

            //PlayerSettings.gpuSkinning = defaultGpuSkinning;
            PlayerSettings.applicationIdentifier = defaultBundleId;
        }

        private static Type FindType(string typeName)
        {
            foreach(var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = a.GetType(typeName);
                if (t != null)
                    return t;
            }

            return null;
        }

        private static void OverrideConfig(string configClass, string configOverride, string configFile)
        {
            Debug.Log("ConfigClass: '" + configClass + "'");
            Debug.Log("ConfigOverride: '" + configOverride + "'");
            Debug.Log("ConfigFile: '" + configFile + "'");

            var configType = configClass != null ? FindType(configClass): null;

            if (configType == null)
                throw new Exception("Config type '" + configClass + "' not found");

            ClientConfig.Config config = null;
            try
            {
                config = ConfigEditor.FindConfig(configType);
            }
            catch {}

            if (config == null)
            {
                var createInstanceMeth = typeof(ScriptableObject).GetMethod("CreateInstance", new Type[] { })
                    .MakeGenericMethod(configType);
                var asset = (ClientConfig.Config) createInstanceMeth.Invoke(null, new object[] { });
                var dir = string.Format("Assets{0}Resources{0}Singleton{0}", Path.DirectorySeparatorChar);
                Directory.CreateDirectory(dir);
                var path = dir + configType.Name + ".asset";
                AssetDatabase.CreateAsset(asset, path);
                config = asset;
            }

            if (configFile != null)
            {
                if (Path.GetExtension(configFile).ToLower() == ".json")
                    configFile = configFile.Substring(0, configFile.Length - 5);
                ConfigEditor.LoadConfig(configFile, config);
            }

            if(configOverride != null)
                JsonUtility.FromJsonOverwrite(configOverride, config);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Dictionary<string, List<string>> ParseArguments()
        {
            var result = new Dictionary<string, List<string>>();
            string[] args = Environment.GetCommandLineArgs();

            string currentKey = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    result.Add(args[i], new List<string>());
                    currentKey = args[i];
                }
                else if (!String.IsNullOrEmpty(currentKey))
                {
                    result[currentKey].Add(args[i]);
                }
            }

            return result;
        }

        private static bool SetLocalState(bool state)
        {
            string path = Application.dataPath + "/Resources/config.json";
            string fileString = "";
            if (!File.Exists(path))
            {
                Debug.LogWarning("There is no config.json file");
                return false;
            }
            using (var sr = new StreamReader(path))
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("UseLocalState"))
                    {
                        if (line.Contains("true"))
                        {
                            if (state)
                                return false;

                            line = line.Replace("true", "false");
                        }
                        else if (line.Contains("false"))
                        {
                            if (!state)
                                return false;

                            line = line.Replace("false", "true");
                        }
                        else
                        {
                            Debug.LogError("Can't parse config file");
                            return false;
                        }
                    }

                    fileString += line + Environment.NewLine;
                }
            }

            File.WriteAllText(path, fileString);
            return true;
        }

        [MenuItem("PestelCrew/Build path")]
        static void PrintPath()
        {
            Debug.Log("Build path: " + EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget));
        }

        public static void IncrementVersion()
        {
            var version = PlayerSettings.bundleVersion;
            var versionSplitted = version.Split('.');
            var minorString = versionSplitted[versionSplitted.Length - 1];
            var minorParsed = Int32.Parse(minorString);

            var newMinorVersion = minorParsed + 1;
            var newVersionArray = versionSplitted.Take(versionSplitted.Length - 1).ToList();
            newVersionArray.Add(newMinorVersion.ToString(CultureInfo.InvariantCulture));

            var newVersionString = string.Join(".", newVersionArray.ToArray());
            PlayerSettings.bundleVersion = newVersionString;
        }

        class Defines
        {
            private BuildTargetGroup? gr;

            public Defines()
            {
                gr = null;
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    gr = BuildTargetGroup.Android;
                }
                else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
                {
                    gr = BuildTargetGroup.iOS;
                }
                else
                {
                    Debug.Log("Can't set defines. Target '" + EditorUserBuildSettings.activeBuildTarget + "' is not supported");
                }
            }

            public void Add(params string[] symb)
            {
                if(gr == null)
                    return;
                if(symb.Length < 1)
                    return;
                var defines = string.Join(";", symb);
                var currDefs = PlayerSettings.GetScriptingDefineSymbolsForGroup(gr.Value);
                Debug.Log(string.Format("Adding defines '{0}'. Current defines '{1}'", defines, currDefs));
                PlayerSettings.SetScriptingDefineSymbolsForGroup(gr.Value, currDefs + ";" + defines);
            }

            public void Clear()
            {
                if(gr == null)
                    return;

                var currDefs = PlayerSettings.GetScriptingDefineSymbolsForGroup(gr.Value);
                Debug.Log(string.Format("Remove defines '{0}'", currDefs));
                PlayerSettings.SetScriptingDefineSymbolsForGroup(gr.Value, "");
            }
        }
    }
}
