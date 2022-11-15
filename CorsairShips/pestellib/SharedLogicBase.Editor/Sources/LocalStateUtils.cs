using System.IO;
using PestelLib.ServerShared;
using UnityEditor;
using UnityEngine;

namespace PestelLib.SharedLogicBase.Editor
{
    public static class LocalStateUtils
    {
        [MenuItem("PestelCrew/SharedLogic/Remove folder 'Application.persistentDataPath'")]
        private static void RemoveLocalState()
        {
            Debug.Log("Deleted directory: " + Application.persistentDataPath);
            Directory.Delete(Application.persistentDataPath, true);
            Debug.Log("Local State has been removed");
        }

        [MenuItem("PestelCrew/SharedLogic/Open folder 'Application.persistentDataPath'")]
        private static void OpenLocalStateFolder()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }

        [MenuItem("PestelCrew/SharedLogic/Open local state")]
        private static void OpenLocalState()
        {
            var databaseName = string.Format("\"{0}/database{1}.sqlite\"", Application.persistentDataPath, Crc32.Compute(Application.dataPath));

            var dbBrowser =
                Application.dataPath.Replace("/Assets", "/") + "pestellib/Tools/SQLiteDatabaseBrowserPortable/SQLiteDatabaseBrowserPortable.exe";

            if (System.IO.File.Exists(dbBrowser))
            {
                System.Diagnostics.Process.Start(dbBrowser, databaseName);
            }
            else
            {
                UnityEngine.Debug.LogError("Can't find diff tool: " + dbBrowser);
            }
        }

    }
}