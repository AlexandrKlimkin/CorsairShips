using System;
using System.Collections;
using System.Collections.Generic;
using GlobalConflictModule.Sources.Scripts;
using PestelLib.ClientConfig;
using PestelLib.ServerClientUtils;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicClient;
using PestelLib.TaskQueueLib;
using PestelLib.Utils;
using S;
using ServerShared.GlobalConflict;
using UnityDI;
using UnityEngine;

namespace GlobalConflict.Example
{
    public class GlobConflictTaskInitDependencyInjection : Task
    {
        private static RequestQueue _requestQueue;

        public override void Run()
        {
            var container = new Container();
            ContainerHolder.Container = container;

            container.RegisterUnitySingleton<UpdateProvider>(null, true);
            container.RegisterUnityScriptableObject<GlobalConflictExampleConfig>(null);
            container.RegisterCustom(() => _requestQueue);
            container.RegisterUnitySingleton<CommandProcessor>(null, true);
            container.RegisterUnitySingleton<GlobConflictTestDefinitions>(null, true);
            container.RegisterSingleton<GlobalConflictClient>();
            container.RegisterCustom<GlobalConflictApi>(() => container.Resolve<GlobalConflictClient>());
            container.RegisterCustom<Config>(() => container.Resolve<GlobalConflictExampleConfig>());

            _requestQueue = new RequestQueue();
            OnComplete(this);
        }
    }
}
