using System.Collections;
using Mixture;
using TerrainMixture.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

namespace TerrainMixture.Runtime
{
	public static class TerrainMixtureRuntime
	{
		static TaskController TaskController;

		static TerrainMixtureRuntime()
		{
		}

		public static void UpdateTerrain(Terrain terrain, MixtureGraph graph)
		{
#if UNITY_EDITOR
			EditorCoroutineUtility.StartCoroutineOwnerless(UpdateTerrainAsync(terrain, graph));
#else
#endif
		}

		public static IEnumerator UpdateTerrainAsync(Terrain terrain, MixtureGraph graph)
		{
			if (TaskController != null)
			{
				TaskController.Cancel();
				yield return TaskController.Wait();
			}

			TaskController = new TaskController();

			using var t = new TerrainTask(graph, terrain, TaskController);
			yield return t.Start();

			TaskController?.Dispose();
			TaskController = null;
		}
	}
}
