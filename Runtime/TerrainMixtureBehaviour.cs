using Mixture;
using TerrainMixture.Runtime.Navigation;
using UnityEngine;

namespace TerrainMixture.Runtime
{
	/// <summary>
	/// Synchronise One Graph with One Terrain.
	/// </summary>
	[ExecuteAlways]
	public class TerrainMixtureBehaviour : MonoBehaviour
	{
		const float DebounceTime = 1f;

		public Texture graphAsset;
		public Terrain terrain;

		MixtureGraph mixtureGraph => MixtureDatabase.GetGraphFromTexture(graphAsset);
		[SerializeField] NavigationSupport navigationSupport;

		TerrainGraphView TerrainGraphView;

		void OnEnable()
		{
			if (!mixtureGraph)
			{
				return;
			}

			TerrainGraphView?.Dispose();
			TerrainGraphView = new TerrainGraphView(terrain, mixtureGraph, DebounceTime);
			TerrainGraphView.navigationSupport = navigationSupport;
			TerrainGraphView.Initialize();
		}

		void OnValidate()
		{
			if (TerrainGraphView != null)
			{
				TerrainGraphView.navigationSupport = navigationSupport;
			}
		}

		void OnDisable()
		{
			TerrainGraphView?.Dispose();
		}
	}
}
