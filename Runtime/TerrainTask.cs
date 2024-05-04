using System;
using System.Collections;
using Mixture;
using TerrainMixture.Utils;
using TerrainMixture.Tasks;
using Unity.Collections;
using UnityEngine;

namespace TerrainMixture.Runtime
{
	public class TerrainTask : ProgressiveTask
	{
		const float MaxDetailsFrameSkip = 8;

		readonly MixtureGraph Graph;
		readonly Terrain Terrain;

		int CurrentTexture;
		int TotalTextures;

		public TerrainTask(MixtureGraph sourceGraph, Terrain terrain, TaskController taskController)
			: base(taskController)
		{
			Terrain = terrain;
			Graph = UnityEngine.Object.Instantiate(sourceGraph);
		}

		protected override IEnumerator Process()
		{
			if (IsCancelled)
			{
				yield break;
			}

			TaskController.Begin("Terrain Task");

			TotalTextures = Graph.outputTextures.Count;
			CurrentTexture = 0;

			TaskController.Progress(0, "Updating graph...");

			MixtureGraphProcessor.RunOnce(Graph);
			Graph.UpdateAndReadbackTextures();

			TaskController.Progress(0, "Processing...");

			foreach (var graphOutputTexture in Graph.outputTextures)
			{
				if (IsCancelled)
				{
					TaskController.Complete();
					yield break;
				}

				switch (graphOutputTexture.name)
				{
					case "Terrain Height":
						CopyToHeightmap(graphOutputTexture);
						break;
					case "Splat 0":
						CopyToTexture(graphOutputTexture, TerrainData.AlphamapTextureName, 0);
						break;
					case "Detail Density 0":
						yield return CopyToDetailDensity(graphOutputTexture, 0);
						break;
				}

				if (IsCancelled)
				{
					TaskController.Complete();
					yield break;
				}

				CurrentTexture++;

				TaskController.Progress((float)CurrentTexture / TotalTextures, "Processing...");
			}
		}

		void CopyToHeightmap(Texture texture)
		{
			var ct = RenderTexture.active;
			var rt = TextureUtility.ToTemporaryRT(texture, Terrain.terrainData.heightmapResolution,
				RenderTextureFormat.R16);
			RenderTexture.active = rt;

			var region = new RectInt(0, 0, rt.width, rt.height);
			var dest = new Vector2Int(0, 0);

			Terrain.terrainData.CopyActiveRenderTextureToHeightmap(region, dest,
				TerrainHeightmapSyncControl.HeightAndLod);
			Terrain.terrainData.SyncHeightmap();

			RenderTexture.active = ct;
			RenderTexture.ReleaseTemporary(rt);
		}

		void CopyToTexture(Texture texture, string textureName, int textureIndex)
		{
			var ct = RenderTexture.active;
			var rt = TextureUtility.ToTemporaryRT(texture, Terrain.terrainData.alphamapResolution,
				RenderTextureFormat.ARGB32);
			RenderTexture.active = rt;

			var region = new RectInt(0, 0, rt.width, rt.height);
			var dest = new Vector2Int(0, 0);

			Terrain.terrainData.CopyActiveRenderTextureToTexture(textureName, textureIndex, region, dest, true);
			Terrain.terrainData.SyncTexture(textureName);

			RenderTexture.active = ct;
			RenderTexture.ReleaseTemporary(rt);
		}

		IEnumerator CopyToDetailDensity(Texture texture, int detailLayer)
		{
			var detailMap =
				Terrain.terrainData.GetDetailLayer(0, 0, Terrain.terrainData.detailWidth,
					Terrain.terrainData.detailHeight,
					detailLayer);

			var rt = TextureUtility.ToTemporaryRT(texture, Terrain.terrainData.alphamapResolution,
				RenderTextureFormat.ARGB32);

			var detailDensityTexture2D = TextureUtility.SyncReadback(rt, TextureFormat.R16);
			RenderTexture.ReleaseTemporary(rt);

			var tmpTextureData = detailDensityTexture2D.GetRawTextureData<ushort>();
			using var detailTextureData = new NativeArray<ushort>(tmpTextureData, Allocator.Persistent);

			var current = 0;
			var total = Terrain.terrainData.detailHeight * Terrain.terrainData.detailWidth;

			var time = Time.realtimeSinceStartup;

			// For each pixel in the detail map...
			for (var y = 0; y < Terrain.terrainData.detailHeight; y++)
			{
				for (var x = 0; x < Terrain.terrainData.detailWidth; x++)
				{
					var textureX = (float)x / Terrain.terrainData.detailWidth * texture.width;
					var textureY = (float)y / Terrain.terrainData.detailHeight * texture.height;
					var textureIndex = (int)(textureY * texture.width + textureX) % detailTextureData.Length;
					var detailSample = (float)detailTextureData[textureIndex] / ushort.MaxValue;

					// For CoverageMode it holds values 0..255
					detailMap[x, y] = (int)Mathf.Clamp(detailSample * 255f, 0, 255);

					++current;
					var sinceLastFrameSkip = Time.realtimeSinceStartup - time;

					if (sinceLastFrameSkip >= MaxDetailsFrameSkip * Time.fixedDeltaTime)
					{
						time = Time.realtimeSinceStartup;

						var progress = Mathf.Lerp((float)CurrentTexture / TotalTextures,
							(float)(CurrentTexture + 1) / TotalTextures,
							(float)current / total);

						if (IsCancelled)
						{
							TaskController.Complete();
							yield break;
						}

						TaskController.Progress(progress, "Details...");
						yield return null;

						if (IsCancelled)
						{
							TaskController.Complete();
							yield break;
						}
					}
				}
			}

			Terrain.terrainData.SetDetailScatterMode(DetailScatterMode.CoverageMode);
			Terrain.terrainData.SetDetailLayer(0, 0, detailLayer, detailMap);

			ObjectUtility.Destroy(detailDensityTexture2D);
		}

		protected override void OnAbort()
		{
			ObjectUtility.Destroy(Graph);
		}
	}
}
