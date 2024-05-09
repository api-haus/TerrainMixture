using System.Collections;
using TerrainMixture.Tasks;
using TerrainMixture.Tasks.Pacing;
using TerrainMixture.Utils;
using Unity.Collections;
using UnityEngine;

namespace TerrainMixture.Runtime.Processing.Streams
{
	public class TerrainDetailStream : ProgressiveTask
	{
		readonly TerrainData TerrainData;

		readonly Texture DensityTexture;
		readonly int Layer;

		Texture2D DetailDensityTexture2D;

		public TerrainDetailStream(TaskController taskController, TerrainData terrainData, Texture densityTexture,
			int detailLayer) : base(taskController)
		{
			TerrainData = terrainData;
			DensityTexture = densityTexture;
			Layer = detailLayer;
		}

		protected override IEnumerator Process()
		{
			if (DensityTexture == null || !DensityTexture || TerrainData == null || !TerrainData)
			{
				// Resources did not survived the domain reload. Ignore this.
				yield break;
			}

			var (detailWidth, detailHeight) = (TerrainData.detailWidth, TerrainData.detailHeight);
			var (textureWidth, textureHeight) = (DensityTexture.width, DensityTexture.height);


			var detailMap =
				TerrainData.GetDetailLayer(0, 0, detailWidth,
					detailHeight,
					Layer);

			// var rt = TextureUtility.ToTemporaryRT(DensityTexture, TerrainData.alphamapResolution,
			// RenderTextureFormat.ARGB32);

			DetailDensityTexture2D = TextureUtility.SyncReadback(DensityTexture as RenderTexture, TextureFormat.R16);
			// RenderTexture.ReleaseTemporary(rt);

			using var tmpTextureData = DetailDensityTexture2D.GetRawTextureData<ushort>();
			using var detailTextureData = new NativeArray<ushort>(tmpTextureData, Allocator.Persistent);

			var time = Time.realtimeSinceStartup;
			var total = detailHeight * detailWidth;
			var current = 0;

			// Debug.Log($"uploading {total} detail cells");

			// For each pixel in the detail map...
			for (var y = 0; y < detailHeight; y++)
			{
				for (var x = 0; x < detailWidth; x++)
				{
					var textureX = (float)x / detailWidth * textureWidth;
					var textureY = (float)y / detailHeight * textureHeight;
					var textureIndex = (int)(textureX * textureWidth + textureY) % detailTextureData.Length;
					var detailSample = (float)detailTextureData[textureIndex] / ushort.MaxValue;

					// For CoverageMode it holds values 0..255
					detailMap[x, y] = (int)Mathf.Clamp(detailSample * 255f, 0, 255);

					var relativeProgress = ++current / (float)total;

					if (CoroutineUtility.FrameSkip(ref time))
					{
						if (IsCancelled)
						{
							yield break;
						}

						TaskController.RelativeProgress(relativeProgress, "Details...");

						yield return null;
					}
				}
			}

			TerrainData.SetDetailScatterMode(DetailScatterMode.CoverageMode);
			TerrainData.SetDetailLayer(0, 0, Layer, detailMap);
		}

		protected override void OnPostProcess()
		{
			if (DetailDensityTexture2D != null)
			{
				ObjectUtility.Destroy(DetailDensityTexture2D);
				DetailDensityTexture2D = null;
			}
		}
	}
}
