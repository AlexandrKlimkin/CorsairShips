using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GlobalConflictModule.Sources.Scripts;
using Newtonsoft.Json;
using ServerShared.GlobalConflict;
using UnityDI;
using UnityEngine;

namespace GlobalConflict.Example
{
    // DONT USE IN PRODUCTION ENVIRONMENT
    class ClassWithHacks
    {
        public enum PrototypeSourceType
        {
            File,
            Server
        }

        public class PrototypeSource
        {
            public PrototypeSourceType Type;
            public string Name;
            public string Description;
            public GlobalConflictState State;
        }

        public static void StartNow(PrototypeSource source, Action<string> callback)
        {
            var conflict = ContainerHolder.Container.Resolve<GlobalConflictApi>();
            if(source.Type == PrototypeSourceType.Server)
                conflict.DebugApi.StartConflict(source.Name, callback);
            else
                conflict.DebugApi.StartConflict(source.State, callback);
        }

        public static void AddTime(TimeSpan time, Action callback)
        {
            var conflict = ContainerHolder.Container.Resolve<GlobalConflictApi>();
            conflict.DebugApi.AddTime((int)time.TotalSeconds, callback);
        }

        public static void GetSources(Action<PrototypeSource[]> callback)
        {
            var validator = new GlobalConflictValidatorDummy();
            var conflict = ContainerHolder.Container.Resolve<GlobalConflictApi>();
            var result = new List<PrototypeSource>();
            if (conflict == null)
            {
                callback(result.ToArray());
            }

            conflict.DebugApi.ListConflictPrototypes((list) =>
            {
                result.AddRange(
                    list.Select(_ => new PrototypeSource()
                    {
                        Type = PrototypeSourceType.Server,
                        Name = _,
                        Description = _ + " (server)"
                    }));

                var jsons = Directory.GetFiles(Application.persistentDataPath, "*.json");
                foreach (var file in jsons)
                {
                    var shortName = Path.GetFileName(file);
                    var content = File.ReadAllText(file);
                    try
                    {
                        var state = JsonConvert.DeserializeObject<GlobalConflictState>(content);
                        if (validator.IsValid(state, new ValidatorMessageCollection()))
                        {
                            result.Add(new PrototypeSource()
                            {
                                Type = PrototypeSourceType.File,
                                Name = state.Id,
                                Description = state.Id + " (" + shortName + ")",
                                State = state
                            });
                        }
                    }
                    catch
                    {
                    }
                }

                callback(result.ToArray());
            });
        }
    }
}
