using System.Collections;
using TerrainMixture.Tasks;
using TerrainMixture.Utils;
using Unity.Collections;
using UnityEngine;

namespace TerrainMixture.Runtime.Streams
{
	public class TerrainDetailStream : ProgressiveTask
	{
		readonly int MaxFrameSkip = 4;
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

			var detailMap =
				TerrainData.GetDetailLayer(0, 0, TerrainData.detailWidth,
					TerrainData.detailHeight,
					Layer);

			var (width, height) = (DensityTexture.width, DensityTexture.height);

			// var rt = TextureUtility.ToTemporaryRT(DensityTexture, TerrainData.alphamapResolution,
			// RenderTextureFormat.ARGB32);

			DetailDensityTexture2D = TextureUtility.SyncReadback(DensityTexture as RenderTexture, TextureFormat.R16);
			// RenderTexture.ReleaseTemporary(rt);

			var tmpTextureData = DetailDensityTexture2D.GetRawTextureData<ushort>();
			using var detailTextureData = new NativeArray<ushort>(tmpTextureData, Allocator.Persistent);

			var time = Time.realtimeSinceStartup;
			var total = TerrainData.detailHeight * TerrainData.detailWidth;
			var current = 0;

			// For each pixel in the detail map...
			for (var y = 0; y < TerrainData.detailHeight; y++)
			{
				for (var x = 0; x < TerrainData.detailWidth; x++)
				{
					var textureX = (float)x / TerrainData.detailWidth * width;
					var textureY = (float)y / TerrainData.detailHeight * height;
					var textureIndex = (int)(textureX * width + textureY) % detailTextureData.Length;
					var detailSample = (float)detailTextureData[textureIndex] / ushort.MaxValue;

					// For CoverageMode it holds values 0..255
					detailMap[x, y] = (int)Mathf.Clamp(detailSample * 255f, 0, 255);

					var relativeProgress = ++current / (float)total;

					if (CoroutineUtility.FrameSkip(ref time, MaxFrameSkip))
					{
						if (IsCancelled)
						{
							TaskController.Complete();
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

		protected override void OnEndProcess()
		{
			if (DetailDensityTexture2D != null)
			{
				ObjectUtility.Destroy(DetailDensityTexture2D);
				DetailDensityTexture2D = null;
			}
		}
	}
}
