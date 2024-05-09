using System;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using GraphProcessor;
#endif
using Mixture;
using Mixture.Nodes;
using TerrainMixture.Runtime.Navigation;
using TerrainMixture.Utils;
using UnityEngine;

namespace TerrainMixture.Runtime
{
	/// <summary>
	/// Watch MixtureGraph of Terrain and update associated Terrain.
	/// </summary>
	[Serializable]
	public class TerrainGraphView : IDisposable
	{
		/// <summary>
		/// In some cases Mixture processes graph automatically.
		/// </summary>
		public bool IsDelegatingUpdates => !TerrainMixtureRuntimeUtility.IsTerrainMixtureGraphViewOpened();

		/// <summary>
		/// Settings: navigation support
		/// </summary>
		public NavigationSupport navigationSupport;

		readonly MixtureGraph MixtureGraph;
		readonly Terrain Terrain;
		MixtureGraphProcessor Processor;

		public void DelegateGraphUpdate()
		{
			if (Processor == null) return;

			// Hacks:
			MixtureGraph.InvokePrivateMethod0Param("OnEnable");
			MixtureGraph.UpdateComputeOrder(ComputeOrderType.DepthFirst);
			MixtureGraph.SetPrivatePropertyValue<OutputNode>("outputNode", null);

			Processor.Run();
		}

		public void PostProcessGraph()
		{
			TerrainMixtureRuntime.UpdateTerrain(Terrain, MixtureGraph, navigationSupport);
		}

		public TerrainGraphView(Terrain terrain, MixtureGraph mixtureGraph, float debounceTime)
		{
			Terrain = terrain;
			MixtureGraph = mixtureGraph;
		}

		public void Initialize()
		{
			if (!MixtureGraph) return;
			Processor = MixtureGraphProcessor.GetOrCreate(MixtureGraph);

			MixtureGraph.afterCommandBufferExecuted += PostProcessGraph;

			if (IsDelegatingUpdates)
			{
				DelegateGraphUpdate();
			}

			TerrainMixtureViewCollection.Add(this);
		}

		public void Dispose()
		{
			Terrain?.ResetData();

			TerrainMixtureViewCollection.Remove(this);
			if (!MixtureGraph) return;
			MixtureGraph.afterCommandBufferExecuted -= PostProcessGraph;

			// MixtureGraph.onExposedParameterValueChanged -= OnExposedParameterValueChanged;
			// MixtureGraph.onGraphChanges -= OnGraphChanges;
		}
	}
}
