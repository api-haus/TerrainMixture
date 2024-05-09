using System.Collections;
using System.Collections.Generic;
using TerrainMixture.Runtime.Grid;
using TerrainMixture.Tasks;
using TerrainMixture.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerrainMixture.Runtime.Behaviours
{
	[RequireComponent(typeof(Terrain))]
	public class TerrainMixtureTile : MonoBehaviour, ITerrainMixtureTile
	{
		public TileCoordinate coordinate;
		public TerrainCollider TerrainCollider => GetComponent<TerrainCollider>();
		public Terrain Terrain => GetComponent<Terrain>();
		public TerrainData TerrainData => Terrain.terrainData;

		[FormerlySerializedAs("CachedHeight")] [SerializeField]
		RenderTexture cachedHeight;

		readonly Dictionary<(string, int), RenderTexture> CachedTextures = new();

		public void Flush()
		{
			TerrainData.UploadHeightmap(cachedHeight);
			foreach (var keyValuePair in CachedTextures)
			{
				TerrainData.UploadTexture(keyValuePair.Value, keyValuePair.Key.Item1, keyValuePair.Key.Item2);
			}

			Terrain.Flush();
		}

		public RenderTexture GetCachedHeightmap() => cachedHeight;

		public RenderTexture GetCachedTexture(string textureName, int textureIndex)
		{
			return CachedTextures.GetValueOrDefault((textureName, textureIndex));
		}

		public TileCoordinate Coordinate => coordinate;

		public void Clear()
		{
			TerrainData.Clear();
		}

		void OnDestroy()
		{
			if (cachedHeight != null)
			{
				ReleaseRT(cachedHeight);
				cachedHeight = null;
			}

			foreach (var cachedSplatValue in CachedTextures.Values)
			{
				if (cachedSplatValue != null)
					ReleaseRT(cachedSplatValue);
			}
		}

		public void SetCachedHeightmap(RenderTexture texture, bool doCopy = true)
		{
			if (cachedHeight)
			{
				ReleaseRT(cachedHeight);
				cachedHeight = null;
			}

			cachedHeight = doCopy ? TextureUtility.CopyRT(texture) : texture;
		}

		static void ReleaseRT(RenderTexture renderTexture)
		{
			renderTexture.Release();
		}

		public void SetCachedTexture(RenderTexture texture, string textureName, int textureIndex)
		{
			if (textureName == TerrainData.AlphamapTextureName)
			{
				if (CachedTextures.TryGetValue((textureName, textureIndex), out var prevValue) && prevValue != null)
				{
					ReleaseRT(prevValue);
				}

				CachedTextures[(textureName, textureIndex)] =
					TextureUtility.CopyRTReadWrite(texture);
			}
		}

		public void UploadTerrainLayers(IReadOnlyList<TerrainLayer> layers)
		{
			TerrainData.UploadTerrainLayers(layers);
		}

		public void UploadDetailPrototypes(IReadOnlyList<DetailPrototype> prototypes)
		{
			TerrainData.UploadDetailPrototypes(prototypes);
		}

		public void UploadTreePrototypes(IReadOnlyList<TreePrototype> prototypes)
		{
			TerrainData.UploadTreePrototypes(prototypes);
		}

		public IEnumerator UploadTreeInstances(TaskController taskController, ComputeBuffer treeInstancesBuffer,
			int maxPoints,
			int layer)
		{
			return TerrainData.UploadTreeInstances(taskController, treeInstancesBuffer, maxPoints, layer);
		}

		public IEnumerator UploadDetailInstances(TaskController taskController, Texture densityMask, int layer)
		{
			return TerrainData.UploadDetailInstances(taskController, densityMask, layer);
		}

		public void SetNeighbours(ITerrainMixtureTile left, ITerrainMixtureTile top, ITerrainMixtureTile right,
			ITerrainMixtureTile bottom)
		{
			Terrain.SetNeighbors(
				(left as TerrainMixtureTile)?.Terrain,
				(top as TerrainMixtureTile)?.Terrain,
				(right as TerrainMixtureTile)?.Terrain,
				(bottom as TerrainMixtureTile)?.Terrain);
		}
	}
}
