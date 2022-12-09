using System;
using UTPLib.Services;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game.SeaGameplay;
using UnityDI;
using UnityEngine;
using UTPLib.Services.ResourceLoader;

namespace Stats {
    public class StatsService : ILoadableService, IUnloadableService {
        [Dependency]
        private readonly IResourceLoaderService _ResourceLoader;

        public void Load() {
            
        }

        public void Unload() {
            
        }

        public StatsController CreateStatsController(StatsDescriptor descriptor) {
            if (descriptor.StatsHolder == StatsHolderType.Ship)
                return CreateShipStatsController(descriptor.Id, descriptor.UpgradeLevel);
            return null;
        }

        private StatsController CreateShipStatsController(string shipId, int upgradeLvl) {
            var configPath = ResourcePath.Ships.GetShipConfigPath(shipId, upgradeLvl);
            var config = _ResourceLoader.LoadResource<ShipStatsConfig>(configPath);
            if (config == null) {
                Debug.LogError($"Config {shipId} lvl {upgradeLvl} not found!");
                return null;
            }
            var statsDict = new Dictionary<StatId, StatBase>();
            
            var fieldStatsList = GetAttributeFieldsFromConfig(config.GetType());
            foreach (var fieldStatPair in fieldStatsList) {
                var fieldInfo = fieldStatPair.Item1;
                var atr = fieldStatPair.Item2;
                var value = fieldInfo.GetValue(config);
                var stat = (StatBase)Activator.CreateInstance(atr.StatType, new[] { atr.Id, value, });
                statsDict.Add(atr.Id, stat);
                // Debug.LogError($"{atr.Id}, {value}");
            }
            // Debug.LogError($"Creating stats controller - {statsDict.Count}");
            return new StatsController(statsDict);
        }

        private List<(FieldInfo, StatAttribute)> GetAttributeFieldsFromConfig(Type type) {
            return type.GetFields()
                .Where(fieldInfo => fieldInfo.GetCustomAttribute(typeof(StatAttribute)) != null)
                .Select(fieldInfo => (fieldInfo, fieldInfo.GetCustomAttribute<StatAttribute>()))
                .ToList();
        }
    }
}