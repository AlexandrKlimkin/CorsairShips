using PestelLib.ClientConfig;
using PestelLib.SaveSystem;
using PestelLib.ServerClientUtils;
using PestelLib.SharedLogicClient;
using PestelLib.Utils;
using UnityDI;
using UTPLib.Tasks.Base;

namespace Game.Initialization.Base {
    public class BackendInitializationTask : AutoCompletedTask {

        [Dependency]
        private readonly Config _Config;

        protected override void AutoCompletedRun() {

            var container = ContainerHolder.Container;

            container.RegisterUnityScriptableObject<Config>();
            container.RegisterUnitySingleton<UpdateProvider>(null, true);
            container.RegisterUnitySingleton<CommandProcessor>(null, true);

            container.RegisterUnitySingleton<RequestQueueEventProcessor>(null, true);
            container.RegisterSingleton<RequestQueue>();
            container.RegisterCustom<IPlayerIdProvider>(() => container.Resolve<RequestQueue>());

            container.RegisterUnitySingleton<SharedTime>(null, true);

            // ??
            //container.Resolve<RequestQueueEventProcessor>();

            // Managers
            // container.RegisterUnitySingleton<InternetChecker>(null, true);
            // container.Resolve<InternetChecker>();

            StorageInitializer.TryInitStorage();
        }
    }
}
