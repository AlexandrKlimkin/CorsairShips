using System.IO;
using GoogleSpreadsheet;
using MessagePack;
using PestelLib.ClientConfig;
using S;
using UnityDI;
using UnityEngine;

namespace PestelLib.ServerClientUtils
{
    public class DefinitionsUpdater
    {
#pragma warning disable 649
        [Dependency] private Config _config;
        [Dependency] private IGameDefinitions _gameDefinitions;
#pragma warning restore 649

        public int DefinitionsVersion { get; set; }
        private static DefsData _defsData;

        public DefinitionsUpdater()
        {
            ContainerHolder.Container.BuildUp(this);
            DefinitionsVersion = (int)_config.SharedConfig.DefinitionsVersion;
            TryUpdateDefinitions();
        }

        private static DefsData ReadDefsData()
        {
            if (File.Exists(DefinitionsPath))
            {
                var serializedDefs = File.ReadAllBytes(DefinitionsPath);
                return MessagePackSerializer.Deserialize<DefsData>(serializedDefs);
            }
            return null;
        }

        public bool IsDefsNewer(DefsData defsData)
        {
            return defsData != null && DefinitionsVersion < defsData.Version;
        }

        public void SaveDefinitions(DefsData defsData)
        {
            File.WriteAllBytes(DefinitionsPath, MessagePackSerializer.Serialize(defsData));
#if UNITY_IOS
            UnityEngine.iOS.Device.SetNoBackupFlag(DefinitionsPath);
#endif
        }

        public void TryUpdateDefinitions()
        {
            _defsData = ReadDefsData();

            if (!IsDefsNewer(_defsData)) return;

            if (_gameDefinitions == null)
            {
                Debug.LogWarning("Cannot resolve instance of IGameDefinitions - skip definitions update");
                return;
            }

            try
            {
                DefinitionsUtils.UpdateDefinitions(_defsData, _gameDefinitions);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Invalid definitions data: " + e.Message + " " + e.InnerException);
            }
            DefinitionsVersion = _defsData.Version;
        }

        private static string DefinitionsPath
        {
            get { return Application.persistentDataPath + "/definitions_mp.bin"; }
        }
    }
}