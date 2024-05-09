#if UNITY_AI_NAVIGATION
using System;
using System.Collections;
using System.Collections.Generic;
using TerrainMixture.Tasks;
using TerrainMixture.Tasks.Pacing;
using UnityEngine;
using UnityEngine.AI;

namespace TerrainMixture.Runtime.Navigation.UnityAINavigation
{
	public class UnityAINavigationTask : NavigationTask
	{
		Vector3 Size => Terrain.terrainData.size;
		Vector3 Center => Terrain.transform.position + Size / 2;

		readonly NavMeshData NavMeshData;
		NavMeshDataInstance Instance;
		readonly List<NavMeshBuildSource> Sources = new();
		AsyncOperation Operation;
		bool HasPinnedSuccessfully = false;

		public UnityAINavigationTask(Terrain terrain, TaskController taskController) : base(terrain, taskController)
		{
			NavMeshData = new();

			Instance = NavMesh.AddNavMeshData(NavMeshData);
			Sources.Add(TerrainSource());
		}

		NavMeshBuildSource TerrainSource() =>
			new()
			{
				shape = NavMeshBuildSourceShape.Terrain,
				sourceObject = Terrain.terrainData,
				transform = Terrain.transform.localToWorldMatrix,
				area = 0
			};

		protected override IEnumerator Process()
		{
			using var navigationTreeContainer =
				new CollectTerrainTreesAsNavigationObstacles(Terrain, TaskController, Sources);

			TaskController.Describe("Tree Obstacles...");

			yield return navigationTreeContainer.Start();

			if (IsCancelled)
			{
				yield break;
			}

			TaskController.Describe("Native NavMesh...");

			PrepareOperation();
			var time = Time.realtimeSinceStartup;
			while (!Operation.isDone)
			{
				if (CoroutineUtility.FrameSkip(ref time))
				{
					if (IsCancelled)
					{
						yield break;
					}

					yield return null;
				}
			}
			// yield return Operation;

			if (IsCancelled)
			{
				yield break;
			}

			// Will replace previous nav mesh data (if any was generated previously)
			NavMeshInstanceHolder.PinToTerrain(Terrain.gameObject, Instance);
			HasPinnedSuccessfully = true;
		}

		void PrepareOperation()
		{
			var defaultBuildSettings = NavMesh.GetSettingsByID(0);
			var bounds = QuantizedBounds();
			defaultBuildSettings.voxelSize = 1f;

			Operation = NavMeshBuilder.UpdateNavMeshDataAsync(NavMeshData, defaultBuildSettings, Sources, bounds);
		}

		static Vector3 Quantize(Vector3 v, Vector3 quant)
		{
			float x = quant.x * Mathf.Floor(v.x / quant.x);
			float y = quant.y * Mathf.Floor(v.y / quant.y);
			float z = quant.z * Mathf.Floor(v.z / quant.z);
			return new Vector3(x, y, z);
		}

		Bounds QuantizedBounds()
		{
			// Quantize the bounds to update only when theres a 10% change in size
			return new Bounds(Quantize(Center, 0.1f * Size), Size);
		}

		protected override void OnPostProcess()
		{
			if (!HasPinnedSuccessfully)
			{
				Instance.Remove();
			}
		}
	}
}
#endif
