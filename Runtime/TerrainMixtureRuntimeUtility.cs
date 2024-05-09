using TerrainMixture.Utils;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_AI_NAVIGATION
using TerrainMixture.Runtime.Navigation.UnityAINavigation;
#endif

namespace TerrainMixture.Runtime
{
	public static class TerrainMixtureRuntimeUtility
	{
		/// <summary>
		/// Returns true if Terrain Mixture is opened in Mixture View (Editor Window).
		/// At runtime returns false.
		/// </summary>
		/// <returns></returns>
		public static bool IsTerrainMixtureGraphViewOpened()
		{
#if UNITY_EDITOR
			var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
			foreach (var win in windows)
			{
				if (win.GetType().Name.Contains("Mixture"))
					return true;
			}
#endif
			return false;
		}

		public static void ResetData(this Terrain terrain)
		{
			terrain.terrainData.Clear();
#if UNITY_AI_NAVIGATION
			var nmh = terrain.GetComponent<NavMeshInstanceHolder>();
			if (nmh && nmh != null)
				ObjectUtility.Destroy(nmh);
#endif
		}
	}
}
