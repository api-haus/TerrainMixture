using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mixture;
using Mixture.Nodes;
using TerrainMixture.Runtime.Navigation;
using TerrainMixture.Tasks;
using UnityEngine;

namespace TerrainMixture.Runtime
{
	public class TerrainTask : ProgressiveTask
	{
		readonly NavigationSupport NavigationSupport;
		readonly MixtureGraph Graph;
		readonly Terrain Terrain;

		TerrainData TerrainData => Terrain.terrainData;

		int CurrentStep;
		int TotalSteps;

		public TerrainTask(NavigationSupport support, MixtureGraph sourceGraph, Terrain terrain,
			TaskController taskController)
			: base(taskController)
		{
			NavigationSupport = support;
			Terrain = terrain;
			Graph = sourceGraph;
		}

		protected override IEnumerator Process()
		{
			if (IsCancelled)
			{
				yield break;
			}

			TaskController.Begin("Terrain Task");

			TaskController.Progress(0, "Executing graph...");

			var validOutputs = Graph.graphOutputs.ToArray();

			var heightOutputs = validOutputs.OfType<TerrainHeightOutputNode>().ToArray();
			var splatOutputs = validOutputs.OfType<TerrainSplatOutputNode>().ToArray();

			var treeOutputs = validOutputs.OfType<TerrainTreesNode>().ToArray();
			var detailOutputs = validOutputs.OfType<TerrainDetailsNode>().ToArray();

			TaskController.Progress(0, "Processing...");

			SetupTerrainParameters();

			TerrainData.Clear();

			SetupTerrainLayers(splatOutputs);

			TotalSteps = heightOutputs.Length + splatOutputs.Length + treeOutputs.Length + detailOutputs.Length + 1;
			CurrentStep = 0;

			void OnNextStep(string description = "Processing...")
			{
				CurrentStep++;
				TaskController.Progress((float)CurrentStep / TotalSteps, description);
			}

			foreach (var heightOutputNode in heightOutputs)
			{
				if (IsCancelled)
				{
					yield break;
				}

				OnNextStep("Heightmap...");
				TerrainData.UploadHeightmap(heightOutputNode.heightOutput);
			}

			foreach (var splatOutputNode in splatOutputs)
			{
				if (IsCancelled)
				{
					yield break;
				}

				OnNextStep("Splat...");
				TerrainData.UploadTexture(splatOutputNode.splatOutput,
					TerrainData.AlphamapTextureName,
					splatOutputNode.splatIndex);
			}

			TerrainData.UploadTreePrototypes(treeOutputs);
			TerrainData.UploadDetailPrototypes(detailOutputs);
			// TerrainData.RefreshPrototypes();

			var treeLayer = 0;

			foreach (var treeOutput in treeOutputs)
			{
				if (IsCancelled)
				{
					yield break;
				}

				OnNextStep("Trees...");

				yield return TerrainData.UploadTreeInstances(TaskController, treeOutput.LastStableBuffer,
					treeOutput.LiveInstancesCount, treeLayer++);
			}

			var detailLayer = 0;
			foreach (var detailOutput in detailOutputs)
			{
				if (IsCancelled)
				{
					yield break;
				}

				OnNextStep("Details...");
				yield return TerrainData.UploadDetailInstances(TaskController, detailOutput.detailOutput, detailLayer++);
			}

			TaskController.Progress((float)CurrentStep / TotalSteps, "Navigation...");

			yield return NavigationTask.GenerateNavMesh(Terrain, TaskController, NavigationSupport);

			OnNextStep("Completed");
		}

		void SetupTerrainParameters()
		{
			var terrainHeight = Graph.GetParameterValue<float>("Terrain Height");
			var terrainDimensions = Graph.GetParameterValue<float>("Terrain Dimensions");
			var graphResolution = Graph.settings.width;

			TerrainData.alphamapResolution = graphResolution;
			TerrainData.heightmapResolution = graphResolution + 1;
			TerrainData.size = new Vector3(terrainDimensions, terrainHeight, terrainDimensions);
		}

		void SetupTerrainLayers(IEnumerable<TerrainSplatOutputNode> splatOutputs)
		{
			var terrainLayers = splatOutputs
				.OrderBy(x => x.splatIndex)
				.SelectMany(x => x.terrainLayers).ToArray();
			TerrainData.UploadTerrainLayers(terrainLayers);
		}

		protected override void OnPostProcess()
		{
		}
	}
}
