using System;
using System.Collections;
using System.Collections.Generic;
using Mixture;
using TerrainMixture.Runtime.Navigation;
using TerrainMixture.Tasks;
using TerrainMixture.Tasks.Pacing;
using TerrainMixture.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace TerrainMixture.Runtime
{
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	public static class TerrainMixtureRuntime
	{
		static readonly Dictionary<int, Guid> LastGuids = new();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		static void Initialize()
		{
			PlayerLoopInterface.InsertSystemBefore(typeof(TerrainMixtureRuntime), OnUpdate,
				typeof(UnityEngine.PlayerLoop.Update.ScriptRunBehaviourUpdate));
		}

#if UNITY_EDITOR
		static TerrainMixtureRuntime()
		{
			Initialize();
		}
#endif

		static void OnUpdate()
		{
			foreach (var view in TerrainMixtureViewCollection.Active)
			{
				// view.OnUpdate();
			}
		}

		public static void UpdateTerrain(Terrain terrain, MixtureGraph graph, NavigationSupport navigation)
		{
			CoroutineUtility.StartCoroutine(UpdateTerrainAsync(terrain, graph, navigation));
		}

		public static IEnumerator UpdateTerrainAsync(Terrain terrain, MixtureGraph graph, NavigationSupport navigation)
		{
			var id = terrain.GetInstanceID();
			var localGuid = Guid.NewGuid();

			LastGuids[id] = localGuid;

			if (TaskControllerCollection.PopController(id, out var previousTask))
			{
				previousTask.Cancel();
				yield return previousTask.Wait();
				yield return CoroutineUtility.WaitForSeconds(.5f);
			}

			if (LastGuids[id] != localGuid)
			{
				yield break;
			}

			// Create new
			using var currentTask = TaskControllerCollection.CreateController(id);
			using var terrainTask = new TerrainTask(navigation, graph, terrain, currentTask);
			yield return terrainTask.Start();
		}
	}
}
