using UnityEditor;

namespace PestelLib.AssetBundles
{
	public class AssetBundlesMenuItems
	{
		const string kSimulationMode = "Assets/AssetBundles/Simulation Mode";
			
		[MenuItem ("Assets/AssetBundles/Build AssetBundles")]
		static public void BuildAssetBundles ()
		{
			BuildScript.BuildAssetBundles();
		}
	}
}