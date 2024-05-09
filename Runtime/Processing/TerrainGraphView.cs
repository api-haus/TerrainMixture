using System;
using TerrainMixture.Runtime.Behaviours;
using TerrainMixture.Tasks.Pacing;

namespace TerrainMixture.Runtime.Processing
{
	/// <summary>
	/// Watch MixtureGraph of Terrain and update associated Terrain.
	/// </summary>
	[Serializable]
	public class TerrainGraphView : TerrainGraphProcessor
	{
		public override bool IsDelegatingUpdates => !TerrainMixtureRuntimeUtility.IsTerrainMixtureGraphViewOpened();

		public TerrainGraphView(ITerrainMixtureTile tile, TerrainTaskParameters parameters) : base(tile, parameters)
		{
		}

		void SchedulePostProcess()
		{
			CoroutineUtility.StartCoroutine(PostProcessGraph());
		}

		public override void Initialize()
		{
			if (!MixtureGraph) return;
			MixtureGraph.afterCommandBufferExecuted += SchedulePostProcess;
			base.Initialize();
		}

		public override void Dispose()
		{
			base.Dispose();
			if (!MixtureGraph) return;
			MixtureGraph.afterCommandBufferExecuted -= SchedulePostProcess;
		}
	}
}
