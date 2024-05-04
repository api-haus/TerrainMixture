using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mixture;
using TerrainMixture.Runtime.Streams;
using TerrainMixture.Tasks;
using TerrainMixture.Utils;
using UnityEngine;

namespace TerrainMixture.Runtime
{
	public static class TerrainUploadExtension
	{
		public static void UploadTexture(this TerrainData terrainData, Texture texture, string textureName,
			int textureIndex)
		{
			var ct = RenderTexture.active;
			var rt = TextureUtility.ToTemporaryRT(texture, terrainData.alphamapResolution,
				RenderTextureFormat.ARGB32);
			RenderTexture.active = rt;

			var region = new RectInt(0, 0, rt.width, rt.height);
			var dest = new Vector2Int(0, 0);

			terrainData.CopyActiveRenderTextureToTexture(textureName, textureIndex, region, dest, true);
			terrainData.SyncTexture(textureName);

			RenderTexture.active = ct;
			RenderTexture.ReleaseTemporary(rt);
		}

		public static void UploadHeightmap(this TerrainData terrainData, Texture texture)
		{
			var ct = RenderTexture.active;
			var rt = TextureUtility.ToTemporaryRT(texture, terrainData.heightmapResolution,
				RenderTextureFormat.R16);
			RenderTexture.active = rt;

			var region = new RectInt(0, 0, rt.width, rt.height);
			var dest = new Vector2Int(0, 0);

			terrainData.CopyActiveRenderTextureToHeightmap(region, dest,
				TerrainHeightmapSyncControl.HeightAndLod);
			terrainData.SyncHeightmap();

			RenderTexture.active = ct;
			RenderTexture.ReleaseTemporary(rt);
		}

		public static void UploadTerrainLayers(this TerrainData terrainData, IReadOnlyList<TerrainLayer> layers)
		{
			terrainData.terrainLayers = layers.ToArray();
		}

		public static void UploadDetailPrototypes(this TerrainData terrainData,
			IReadOnlyList<TerrainDetailsNode> detailOutputs)
		{
			var prototypes = new DetailPrototype[detailOutputs.Count];

			for (var i = 0; i < detailOutputs.Count; i++)
			{
				var output = detailOutputs[i];
				prototypes[i] = output.ToDetailPrototype();
			}

			terrainData.detailPrototypes = prototypes;
		}

		public static void UploadTreePrototypes(this TerrainData terrainData, IReadOnlyList<TerrainTreesNode> treeOutputs)
		{
			var prototypes = new TreePrototype[treeOutputs.Count];

			for (var i = 0; i < treeOutputs.Count; i++)
			{
				var output = treeOutputs[i];
				prototypes[i] = output.ToTreePrototype();
			}

			terrainData.treePrototypes = prototypes;
		}

		public static IEnumerator UploadTreeInstances(this TerrainData terrainData, TaskController taskController,
			ComputeBuffer treeInstancesBuffer, int maxPoints, int layer)
		{
			var treeSubtask =
				new TerrainTreeStream(taskController, terrainData, treeInstancesBuffer, maxPoints,
					layer);

			yield return treeSubtask.Start();
		}

		public static IEnumerator UploadDetailInstances(this TerrainData terrainData, TaskController taskController,
			Texture densityMask, int layer)
		{
			var detailSubtask =
				new TerrainDetailStream(taskController, terrainData, densityMask, layer);

			yield return detailSubtask.Start();
		}
	}
}
