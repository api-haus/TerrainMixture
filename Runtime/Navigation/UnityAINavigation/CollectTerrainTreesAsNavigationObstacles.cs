#if UNITY_AI_NAVIGATION
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TerrainMixture.Tasks;
using TerrainMixture.Tasks.Pacing;
using TerrainMixture.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace TerrainMixture.Runtime.Navigation.UnityAINavigation
{
	/// <summary>
	/// Insert Terrain Trees as NavMesh obstacles.
	/// Collects them into passed Source List.
	/// </summary>
	public class CollectTerrainTreesAsNavigationObstacles : ProgressiveTask
	{
		const int MaxFrameSkip = 4;

		readonly Terrain Terrain;
		readonly NavMeshBuildSource[] ObstaclePrototypes;
		readonly List<NavMeshBuildSource> Sources;

		public CollectTerrainTreesAsNavigationObstacles(Terrain terrain, TaskController taskController, List<NavMeshBuildSource> sources) :
			base(taskController)
		{
			Sources = sources;
			Terrain = terrain;
			ObstaclePrototypes = CreateObstaclePrototypes();
		}

		protected override IEnumerator Process()
		{
			var time = Time.realtimeSinceStartup;

			var total = Terrain.terrainData.treeInstanceCount;
			var treeInstances = Terrain.terrainData.treeInstances;

			for (var current = 0; current < treeInstances.Length; current++)
			{
				var treeInstance = treeInstances[current];

				var obstacle = ObstaclePrototypes[treeInstance.prototypeIndex];

				var localPosition = treeInstance.GetLocalPosition(Terrain);

				obstacle.transform = Matrix4x4.TRS(localPosition, Quaternion.identity, Vector3.one);

				Sources.Add(obstacle);

				if (CoroutineUtility.FrameSkip(ref time))
				{
					if (IsCancelled)
					{
						yield break;
					}

					TaskController.RelativeProgress(current / (float)total, "NavMesh Obstacles...");

					yield return null;
				}
			}
		}

		NavMeshBuildSource[] CreateObstaclePrototypes()
		{
			return Terrain.terrainData.treePrototypes.Select(prototype =>
				{
					var colliders = prototype.prefab.GetComponentsInChildren<Collider>();
					var oneCollider = colliders.FirstOrDefault(c => c.enabled);
					if (!oneCollider)
					{
						throw new Exception("Tree has invaild collision");
					}

					var source = new NavMeshBuildSource();
					source.component = oneCollider;
					source.area = 0;

					switch (oneCollider)
					{
						case BoxCollider box:
							source.shape = NavMeshBuildSourceShape.Box;
							source.size = box.size;

							break;
						case SphereCollider sphere:
							source.shape = NavMeshBuildSourceShape.Sphere;
							source.size = Vector3.one * sphere.radius;

							break;
						case CapsuleCollider capsule:
							source.shape = NavMeshBuildSourceShape.Capsule;
							source.size = new Vector3(capsule.radius, capsule.height, capsule.radius);

							break;
						case MeshCollider mesh:
							source.shape = NavMeshBuildSourceShape.Mesh;
							source.sourceObject = mesh.sharedMesh;

							break;
						default:
							throw new ArgumentOutOfRangeException($"Unknown collider: {oneCollider}");
					}

					return source;
				})
				.ToArray();
		}

		protected override void OnPostProcess()
		{
		}
	}
}
#endif
