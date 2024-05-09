using System;
using Mixture;
using TerrainMixture.Runtime.Navigation;
using Unity.Mathematics;
using UnityEngine;

namespace TerrainMixture.Runtime.Processing
{
	[Serializable]
	public struct TerrainTaskParameters
	{
		[HideInInspector] public Terrain terrain;
		[HideInInspector] public MixtureGraph mixtureGraph;
		public int detailResolution;
		public int detailResolutionPerPatch;
		public int graphResolution;

		public readonly int DownscaleFactor => (int)math.pow(2, resolutionDownscale);
		public readonly int DownscaledResolution => graphResolution / DownscaleFactor;
		public readonly int DownscaledDetailResolution => detailResolution / DownscaleFactor;
		public readonly int DownscaledDetailResolutionPerPatch => math.min(32, detailResolutionPerPatch / DownscaleFactor);

		[HideInInspector] public int resolutionDownscale;
		public float terrainDimensions;
		public float terrainHeight;
		[HideInInspector] public Vector3 worldOrigin;
		public NavigationSupport navigationSupport;
		public Material materialTemplate;
		public int seed;

		public TerrainData ToTerrainData()
		{
			var terrainData = new TerrainData { name = $"Tile {worldOrigin}" };
			ApplyToTerrainData(terrainData);
			return terrainData;
		}

		public readonly void ApplyToTerrainData(TerrainData terrainData)
		{
			terrainData.alphamapResolution = DownscaledResolution;
			terrainData.baseMapResolution = DownscaledResolution;
			terrainData.heightmapResolution = graphResolution + 1;
			terrainData.size = new Vector3(terrainDimensions, terrainHeight, terrainDimensions);
			terrainData.SetDetailResolution(DownscaledDetailResolution,
				DownscaledDetailResolutionPerPatch);
		}

		public int GetInstanceID()
		{
			return terrain.GetInstanceID();
		}

		public override string ToString()
		{
			return JsonUtility.ToJson(this, true);
		}

		public static TerrainTaskParameters Default()
		{
			return new TerrainTaskParameters
			{
				detailResolution = 512,
				detailResolutionPerPatch = 64,
				graphResolution = 512,
				seed = 42,
				resolutionDownscale = 1,
				terrainDimensions = 500,
				terrainHeight = 300,
				worldOrigin = Vector3.zero,
				navigationSupport = NavigationSupport.None,
			};
		}
	}
}
