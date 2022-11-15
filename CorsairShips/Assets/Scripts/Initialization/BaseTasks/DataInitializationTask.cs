using GoogleSpreadsheet;
using PestelLib.SharedLogic;
using UnityDI;
using UnityDI.Providers;
using PestelLib.Localization;
using UTPLib.Services.ResourceLoader;
using UTPLib.Tasks.Base;

namespace Game.Initialization.Base {
	public class DataInitializationTask : AutoCompletedTask {

		protected override void AutoCompletedRun() {
			var resourceLoader = ContainerHolder.Container.Resolve<IResourceLoaderService>();

			#region Definitions

			var definitionsContainer =
				resourceLoader.LoadResource<DefinitionsContainer>("Singleton/DefinitionsContainer");
			var definitions = definitionsContainer.Definitions;
			definitions.OnAfterDeserialize();
			var localization = definitionsContainer.LocalizationDefContainer;

			ContainerHolder.Container.RegisterInstance(definitionsContainer);
			ContainerHolder.Container.RegisterInstance<IGameDefinitions>(definitions);
			ContainerHolder.Container.RegisterInstance(definitions);
			
			ContainerHolder.Container.RegisterInstance(localization);
			ContainerHolder.Container.RegisterInstance(localization.LocalizationDef);
			ContainerHolder.Container.RegisterInstance(new LocalizationData());
			ContainerHolder.Container.RegisterCustom<ILocalization>(() =>
				ContainerHolder.Container.Resolve<LocalizationData>());

			typeof(Definitions)
				.GetFields()
				.ForEach(_ => {
					ContainerHolder.Container.RegisterTypeProvider(_.FieldType,
						new InstanceProviderNonGeneric(_.GetValue(definitions)));
				});

			#endregion

		}
	}
}