using System;
using Mixture;
using TerrainMixture.Runtime.Navigation;
using TerrainMixture.Runtime.Processing;
using TerrainMixture.Utils;
using UnityEngine;

namespace TerrainMixture.Runtime.Behaviours
{
	/// <summary>
	/// Synchronise One Graph with One Terrain.
	/// </summary>
	[ExecuteAlways]
	public class TerrainMixtureBehaviour : MonoBehaviour
	{
		public Texture graphAsset;
		public Terrain terrain;

		MixtureGraph mixtureGraph => MixtureDatabase.GetGraphFromTexture(graphAsset);

		TerrainGraphView TerrainGraphView;
		ITerrainMixtureTile Tile;

		void OnEnable()
		{
			if (!mixtureGraph || !terrain)
			{
				return;
			}

			if (!terrain.gameObject.TryGetComponent(out Tile))
			{
				Tile = terrain.gameObject.AddComponent<TerrainMixtureTile>();
			}

			TerrainGraphView?.Dispose();
			TerrainGraphView = new TerrainGraphView(Tile, TerrainTaskParameters.Default());
			TerrainGraphView.Initialize();
		}

		void OnValidate()
		{
			if (TerrainGraphView == null) return;
			TerrainGraphView.terrainTaskParameters.terrain = terrain;
			TerrainGraphView.terrainTaskParameters.mixtureGraph = mixtureGraph;
			TerrainGraphView.terrainTaskParameters.resolutionDownscale = 1;
		}

		void OnDisable()
		{
			TerrainGraphView?.Dispose();
		}
	}
}
