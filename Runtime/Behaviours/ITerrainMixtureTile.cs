using System.Collections;
using System.Collections.Generic;
using TerrainMixture.Runtime.Grid;
using TerrainMixture.Tasks;
using UnityEngine;

namespace TerrainMixture.Runtime.Behaviours
{
	public interface ITerrainMixtureTile
	{
		void Clear();

		void SetCachedHeightmap(RenderTexture texture, bool doCopy = true);

		void SetCachedTexture(RenderTexture texture, string textureName,
			int textureIndex);

		void UploadTerrainLayers(IReadOnlyList<TerrainLayer> layers);

		void UploadDetailPrototypes(IReadOnlyList<DetailPrototype> prototypes);

		void UploadTreePrototypes(IReadOnlyList<TreePrototype> prototypes);

		IEnumerator UploadTreeInstances(TaskController taskController,
			ComputeBuffer treeInstancesBuffer, int maxPoints, int layer);

		IEnumerator UploadDetailInstances(TaskController taskController,
			Texture densityMask, int layer);

		void SetNeighbours(ITerrainMixtureTile left, ITerrainMixtureTile top, ITerrainMixtureTile right,
			ITerrainMixtureTile bottom);

		void Flush();

		RenderTexture GetCachedHeightmap();

		RenderTexture GetCachedTexture(string textureName,
			int textureIndex);

		TileCoordinate Coordinate { get; }
	}
}
