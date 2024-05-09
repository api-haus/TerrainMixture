using System;
using System.Collections;
using TerrainMixture.Runtime.Behaviours;
using TerrainMixture.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainMixture.Runtime.Grid
{
	public class TerrainTileBlender : IDisposable
	{
		Shader Shader;
		Material Material;

		public enum ESide
		{
			Top = 0,
			Right = 1,
		}

		readonly ITerrainMixtureTile Center;
		readonly ITerrainMixtureTile Left;
		readonly ITerrainMixtureTile Top;
		readonly ITerrainMixtureTile Right;
		readonly ITerrainMixtureTile Bottom;

		// ReSharper disable InconsistentNaming
		static readonly int _TexelSize = Shader.PropertyToID("_TexelSize");
		static readonly int _Side = Shader.PropertyToID("_Side");
		static readonly int _MainTex = Shader.PropertyToID("_MainTex");
		static readonly int _EdgeTex = Shader.PropertyToID("_EdgeTex");
		static readonly int _Blend = Shader.PropertyToID("_Blend");
		// ReSharper restore InconsistentNaming

		public TerrainTileBlender(ITerrainMixtureTile center,
			ITerrainMixtureTile left, ITerrainMixtureTile top,
			ITerrainMixtureTile right, ITerrainMixtureTile bottom)
		{
			Center = center;
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
			Shader = Resources.Load<Shader>("TerrainMixture/EdgeBlend");
			Material = new Material(Shader);
		}

		public void Dispose()
		{
			ObjectUtility.Destroy(Material);
		}

		public IEnumerator Process()
		{
			yield return BlendHeight();
			yield return BlendSplat();

			Center.SetNeighbours(Left, Top, Right, Bottom);
		}

		public IEnumerator BlendHeight()
		{
			var centerHeight = Center.GetCachedHeightmap();
			var neighbourHeightT = Top?.GetCachedHeightmap();
			var neighbourHeightR = Right?.GetCachedHeightmap();

			if (neighbourHeightR != null)
			{
				yield return CopyEdges(ESide.Right, centerHeight, neighbourHeightR);
			}

			if (neighbourHeightT != null)
			{
				yield return CopyEdges(ESide.Top, centerHeight, neighbourHeightT);
			}
		}

		public IEnumerator BlendSplat()
		{
			for (var i = 0; i < 4; i++)
			{
				var centerSplat = Center.GetCachedTexture(TerrainData.AlphamapTextureName, i);
				if (!centerSplat) continue;

				var splatNeighbourT = Top?.GetCachedTexture(TerrainData.AlphamapTextureName, i);
				var splatNeighbourR = Right?.GetCachedTexture(TerrainData.AlphamapTextureName, i);

				if (splatNeighbourR != null)
				{
					yield return BlendEdges(ESide.Right, centerSplat, splatNeighbourR);
				}

				if (splatNeighbourT != null)
				{
					yield return BlendEdges(ESide.Top, centerSplat, splatNeighbourT);
				}
			}
		}

		public IEnumerator CopyEdges(ESide eSide, RenderTexture mainTexture, RenderTexture edgeTexture)
		{
			Material.SetInt(_Side, (int)eSide);
			Material.SetVector(_TexelSize, mainTexture.texelSize);
			Material.SetTexture(_MainTex, mainTexture);
			Material.SetTexture(_EdgeTex, edgeTexture);
			Material.SetFloat(_Blend, 0);

			// Graphics.Blit(readFrom, writeInto, Material);
			var cmd = CommandBufferPool.Get();

			Blitter.BlitTexture(cmd, edgeTexture, mainTexture, Material, 0);
			Graphics.ExecuteCommandBuffer(cmd);
			cmd.Clear();
			CommandBufferPool.Release(cmd);

			yield return null;
		}

		public IEnumerator BlendEdges(ESide eSide, RenderTexture mainTexture, RenderTexture edgeTexture)
		{
			Material.SetInt(_Side, (int)eSide);
			Material.SetVector(_TexelSize, mainTexture.texelSize);
			Material.SetTexture(_MainTex, mainTexture);
			Material.SetTexture(_EdgeTex, edgeTexture);
			Material.SetFloat(_Blend, 1);

			// Graphics.Blit(readFrom, writeInto, Material);
			var cmd = CommandBufferPool.Get();

			Blitter.BlitTexture(cmd, edgeTexture, mainTexture, Material, 0);
			Graphics.ExecuteCommandBuffer(cmd);
			cmd.Clear();
			CommandBufferPool.Release(cmd);

			yield return null;
		}
	}
}
