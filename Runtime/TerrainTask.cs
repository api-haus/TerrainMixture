using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using Mixture;
using TerrainMixture.Tasks;
using TerrainMixture.Utils;
using UnityEngine;

namespace TerrainMixture.Runtime
{
	public class TerrainTask : ProgressiveTask
	{
		const bool InstantiateGraph = false;

		readonly MixtureGraph Graph;
		readonly Terrain Terrain;

		TerrainData TerrainData => Terrain.terrainData;

		int CurrentStep;
		int TotalSteps;

		public TerrainTask(MixtureGraph sourceGraph, Terrain terrain, TaskController taskController)
			: base(taskController)
		{
			Terrain = terrain;
			Graph = InstantiateGraph ? Object.Instantiate(sourceGraph) : sourceGraph;
		}

		protected override IEnumerator Process()
		{
			if (IsCancelled)
			{
				yield break;
			}

			TaskController.Begin("Terrain Task");

			TaskController.Progress(0, "Executing graph...");

			var heightOutputs = Graph.graphOutputs.OfType<TerrainHeightOutputNode>().Where(x => x.canProcess).ToArray();
			var splatOutputs = Graph.graphOutputs.OfType<TerrainSplatOutputNode>().Where(x => x.canProcess).ToArray();

			var treeOutputs = Graph.graphOutputs.OfType<TerrainTreesNode>().Where(x => x.canProcess).ToArray();
			var detailOutputs = Graph.graphOutputs.OfType<TerrainDetailsNode>().Where(x => x.canProcess).ToArray();

			// Idea is to force execution of relevant outputs.
			// After first execution they are getting culled for some reason.

			List<BaseNode> relevantOutputs = new();
			relevantOutputs.AddRange(heightOutputs);
			relevantOutputs.AddRange(splatOutputs);
			relevantOutputs.AddRange(treeOutputs);
			relevantOutputs.AddRange(detailOutputs);

			foreach (var relevantOutput in relevantOutputs)
			{
				if (relevantOutput.computeOrder <= 0)
					relevantOutput.Initialize(Graph);
			}

			MixtureGraphProcessor.RunOnce(Graph);

			TaskController.Progress(0, "Processing...");

			SetupTerrainParameters();

			SetupTerrainLayers(splatOutputs);

			TotalSteps = heightOutputs.Length + splatOutputs.Length + treeOutputs.Length + detailOutputs.Length;
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
					TaskController.Complete();
					yield break;
				}

				OnNextStep("Heightmap...");
				TerrainData.UploadHeightmap(heightOutputNode.heightOutput);
			}

			foreach (var splatOutputNode in splatOutputs)
			{
				if (IsCancelled)
				{
					TaskController.Complete();
					yield break;
				}

				OnNextStep("Splat...");
				TerrainData.UploadTexture(splatOutputNode.splatOutput,
					TerrainData.AlphamapTextureName,
					splatOutputNode.splatIndex);
			}

			var treeLayer = 0;
			TerrainData.UploadTreePrototypes(treeOutputs);

			foreach (var treeOutput in treeOutputs)
			{
				if (IsCancelled)
				{
					TaskController.Complete();
					yield break;
				}

				OnNextStep("Trees...");
				yield return TerrainData.UploadTreeInstances(TaskController, treeOutput.TreeInstancesBuffer,
					treeOutput.LiveInstancesCount, treeLayer++);
			}

			var detailLayer = 0;
			TerrainData.UploadDetailPrototypes(detailOutputs);

			foreach (var detailOutput in detailOutputs)
			{
				if (IsCancelled)
				{
					TaskController.Complete();
					yield break;
				}

				OnNextStep("Details...");
				yield return TerrainData.UploadDetailInstances(TaskController, detailOutput.detailOutput, detailLayer++);
			}
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

		protected override void OnEndProcess()
		{
			if (InstantiateGraph)
				ObjectUtility.Destroy(Graph);
		}
	}
}
