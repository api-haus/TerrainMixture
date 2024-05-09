#if UNITY_AI_NAVIGATION
using TerrainMixture.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace TerrainMixture.Runtime.Navigation.UnityAINavigation
{
	/// <summary>
	/// This component just remembers NavMeshData for the terrain. When its destroyed, the nav mesh data is also destroyed.
	/// </summary>
	[ExecuteAlways]
	public class NavMeshInstanceHolder : MonoBehaviour
	{
		public static void PinToTerrain(GameObject terrainGameObject, NavMeshDataInstance instance)
		{
			if (terrainGameObject.TryGetComponent<NavMeshInstanceHolder>(out var existingInstance))
			{
				ObjectUtility.Destroy(existingInstance);
			}

			var holder = terrainGameObject.AddComponent<NavMeshInstanceHolder>();
			holder.Instance = instance;
		}

		NavMeshDataInstance Instance;

		void OnDestroy()
		{
			Instance.Remove();
		}
	}
}
#endif
