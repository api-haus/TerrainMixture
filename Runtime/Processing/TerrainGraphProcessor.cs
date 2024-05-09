using System;
using System.Collections;
using GraphProcessor;
using Mixture;
using TerrainMixture.Runtime.Behaviours;
using TerrainMixture.Runtime.Navigation;
using TerrainMixture.Tasks.Pacing;
using TerrainMixture.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerrainMixture.Runtime.Processing
{
	[Serializable]
	public class TerrainGraphProcessor : IDisposable
	{
		public Terrain Terrain => terrainTaskParameters.terrain;
		public TerrainData TerrainData => terrainTaskParameters.terrain.terrainData;
		public MixtureGraph MixtureGraph => terrainTaskParameters.mixtureGraph;

		/// <summary>
		/// In some cases Mixture processes graph automatically.
		/// </summary>
		public virtual bool IsDelegatingUpdates => true;

		public TerrainTaskParameters terrainTaskParameters = TerrainTaskParameters.Default();
		MixtureGraphProcessor Processor;
		readonly ITerrainMixtureTile Tile;

		public virtual IEnumerator PostProcessGraph()
		{
			yield return TerrainMixtureRuntime.UpdateTerrainAsync(Tile, terrainTaskParameters);
		}

		public TerrainGraphProcessor(ITerrainMixtureTile tile, TerrainTaskParameters parameters)
		{
			Tile = tile;
			terrainTaskParameters = parameters;
			Processor = MixtureGraphProcessor.GetOrCreate(MixtureGraph);
		}

		public void DelegateGraphUpdate()
		{
			if (Processor == null) return;

			MixtureGraph.settings.width = terrainTaskParameters.DownscaledResolution;
			MixtureGraph.settings.height = terrainTaskParameters.DownscaledResolution;

			MixtureGraph.SetParameterValue("Terrain Height", terrainTaskParameters.terrainHeight);
			MixtureGraph.SetParameterValue("Terrain Dimensions", terrainTaskParameters.terrainDimensions);
			MixtureGraph.SetParameterValue("World Origin", (Vector3)terrainTaskParameters.worldOrigin);
			MixtureGraph.SetParameterValue("Seed", terrainTaskParameters.seed);

			// Hacks:
			MixtureGraph.InvokePrivateMethod0Param("OnEnable");
			MixtureGraph.UpdateComputeOrder(ComputeOrderType.DepthFirst);
			MixtureGraph.SetPrivatePropertyValue<OutputNode>("outputNode", null);

			Processor.Run();
		}

		public virtual void Initialize()
		{
			if (!MixtureGraph) return;
			if (IsDelegatingUpdates)
			{
				DelegateGraphUpdate();
			}
		}

		public virtual void Dispose()
		{
		}
	}
}
