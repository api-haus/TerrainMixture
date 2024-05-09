using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JBooth.MicroSplat;
using Mixture;
using Mixture.Nodes;
using TerrainMixture.Runtime.Behaviours;
using TerrainMixture.Runtime.Navigation;
using TerrainMixture.Runtime.Processing;
using TerrainMixture.Tasks;
using TerrainMixture.Utils;
using UnityEngine;

namespace TerrainMixture.Runtime
{
	public class TerrainTask : ProgressiveTask
	{
		NavigationSupport NavigationSupport => Parameters.navigationSupport;
		MixtureGraph Graph => Parameters.mixtureGraph;
		Terrain Terrain => Parameters.terrain;
		TerrainData TerrainData => Terrain.terrainData;

		int CurrentStep;
		int TotalSteps;
		readonly TerrainTaskParameters Parameters;
		readonly ITerrainMixtureTile Tile;

		public TerrainTask(TerrainTaskParameters requestParams, ITerrainMixtureTile tile, TaskController currentTask) :
			base(currentTask)
		{
			Tile = tile;
			Parameters = requestParams;
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

			Tile.Clear();

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

				// We need +1 because edges are copied to stitch terrains together
				var size = heightOutputNode.heightOutput.width + 1;
				var rtCopy = TextureUtility.CopyRT(heightOutputNode.heightOutput, RenderTextureFormat.R16, size);
				Tile.SetCachedHeightmap(rtCopy, false);
			}

			foreach (var splatOutputNode in splatOutputs)
			{
				if (IsCancelled)
				{
					yield break;
				}

				OnNextStep("Splat...");
				Tile.SetCachedTexture(splatOutputNode.splatOutput,
					TerrainData.AlphamapTextureName,
					splatOutputNode.splatIndex);
			}

			Tile.UploadTreePrototypes(ToTreePrototypes(treeOutputs));
			Tile.UploadDetailPrototypes(ToDetailPrototypes(detailOutputs));
			// TerrainData.RefreshPrototypes();

			var treeLayer = 0;

			foreach (var treeOutput in treeOutputs)
			{
				if (IsCancelled)
				{
					yield break;
				}

				OnNextStep("Trees...");

				yield return Tile.UploadTreeInstances(TaskController, treeOutput.LastStableBuffer,
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
				yield return Tile.UploadDetailInstances(TaskController, detailOutput.detailOutput, detailLayer++);
			}

			TaskController.Progress((float)CurrentStep / TotalSteps, "Navigation...");

			yield return NavigationTask.GenerateNavMesh(Terrain, TaskController, NavigationSupport);

			OnNextStep("Completed");
#if MICROSPLAT
			SetupMicroSplat();
#else
			Terrain.materialTemplate = Parameters.materialTemplate;
#endif
		}

		static IReadOnlyList<DetailPrototype> ToDetailPrototypes(IReadOnlyList<TerrainDetailsNode> detailOutputs)
		{
			var prototypes = new DetailPrototype[detailOutputs.Count];

			for (var i = 0; i < detailOutputs.Count; i++)
			{
				var output = detailOutputs[i];
				prototypes[i] = output.ToDetailPrototype();
			}

			return prototypes;
		}

		static IReadOnlyList<TreePrototype> ToTreePrototypes(IReadOnlyList<TerrainTreesNode> treeOutputs)
		{
			var prototypes = new TreePrototype[treeOutputs.Count];

			for (var i = 0; i < treeOutputs.Count; i++)
			{
				var output = treeOutputs[i];
				prototypes[i] = output.ToTreePrototype();
			}

			return prototypes;
		}

#if MICROSPLAT
		void SetupMicroSplat()
		{
			var mt = Terrain.gameObject.AddComponent<MicroSplatTerrain>();
			mt.templateMaterial = Parameters.materialTemplate;
			mt.Sync();
		}
#endif

		void SetupTerrainParameters()
		{
			Parameters.ApplyToTerrainData(TerrainData);
		}

		void SetupTerrainLayers(IEnumerable<TerrainSplatOutputNode> splatOutputs)
		{
			var terrainLayers = splatOutputs
				.OrderBy(x => x.splatIndex)
				.SelectMany(x => x.terrainLayers).ToArray();
			Tile.UploadTerrainLayers(terrainLayers);
		}

		protected override void OnPostProcess()
		{
		}
	}
}
