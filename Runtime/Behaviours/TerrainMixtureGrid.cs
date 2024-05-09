using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mixture;
using TerrainMixture.Runtime.Grid;
using TerrainMixture.Runtime.Processing;
using TerrainMixture.Utils;
using Unity.Mathematics;
using UnityEngine;
using Transform = UnityEngine.Transform;

namespace TerrainMixture.Runtime.Behaviours
{
	public class TerrainMixtureGrid : MonoBehaviour
	{
		public Terrain tilePrefab;
		public Texture graphAsset;
		MixtureGraph mixtureGraph => MixtureDatabase.GetGraphFromTexture(graphAsset);

		public TerrainTaskParameters parameters = TerrainTaskParameters.Default();

		public TerrainMixtureGridLod[] lods = { new(1, 0), new(2, 1) };

		float3 LastStablePosition = new(-float.MaxValue, 0, 0);
		Transform TerrainGroup;

		readonly Dictionary<TileCoordinate, ITerrainMixtureTile> LiveTerrains = new();

		void OnEnable()
		{
			if (TerrainGroup != null)
				ObjectUtility.Destroy(TerrainGroup.gameObject);
			TerrainGroup = new GameObject("Terrain Grid") { hideFlags = HideFlags.DontSave }.transform;
		}

		void OnDisable()
		{
			ObjectUtility.Destroy(TerrainGroup.gameObject);
		}

		IEnumerator UpdateAsync()
		{
			var nearbyCells = QueryCellsLod(LastStablePosition);

			List<ITerrainMixtureTile> newTiles = new();
			foreach (var request in nearbyCells)
			{
				var tile = SpawnTile(request.Coordinate);
				LiveTerrains[tile.coordinate] = tile;
				newTiles.Add(tile);

				tile.transform.parent = TerrainGroup.transform;

				var requestParams = parameters;

				requestParams.terrain = tile.Terrain;
				requestParams.mixtureGraph = mixtureGraph;
				requestParams.worldOrigin = tile.coordinate.ToWorldOrigin();
				requestParams.resolutionDownscale = request.Lod.resolutionDownscale;

				var terrainData = requestParams.ToTerrainData();

				tile.Terrain.terrainData = terrainData;
				tile.TerrainCollider.terrainData = terrainData;

				using var processor = new TerrainGraphProcessor(tile, requestParams);
				processor.Initialize();
				yield return processor.PostProcessGraph();
			}

			yield return PostProcessTiles(newTiles.OrderByDescending(x => x.Coordinate));
		}

		IEnumerator PostProcessTiles(IEnumerable<ITerrainMixtureTile> newTiles)
		{
			foreach (var terrainMixtureTile in newTiles)
			{
				var center = terrainMixtureTile.Coordinate;
				LiveTerrains.TryGetValue(center + new TileCoordinate(-1, 0), out var left);
				LiveTerrains.TryGetValue(center + new TileCoordinate(0, 1), out var top);
				LiveTerrains.TryGetValue(center + new TileCoordinate(1, 0), out var right);
				LiveTerrains.TryGetValue(center + new TileCoordinate(0, -1), out var bottom);

				var terrainTileBlender = new TerrainTileBlender(terrainMixtureTile, left, top, right, bottom);

				yield return terrainTileBlender.Process();
			}

			foreach (var terrainMixtureTile in newTiles)
			{
				terrainMixtureTile.Flush();
			}
		}

		void Update()
		{
			var observer = ObserverPoint.Position;
			if (!math.any(math.abs(LastStablePosition - observer) >= parameters.terrainDimensions)) return;
			LastStablePosition = observer;

			StartCoroutine(UpdateAsync());
		}

		TerrainMixtureTile SpawnTile(TileCoordinate requestCoordinate)
		{
			var go = Instantiate(tilePrefab).gameObject;

			go.transform.position = requestCoordinate.ToWorldPosition(parameters.terrainDimensions);

			var tile = go.AddComponent<TerrainMixtureTile>();
			tile.coordinate = requestCoordinate;
			tile.Terrain.allowAutoConnect = false;

			return tile;
		}

		List<NearbyCellRequest> QueryCellsLod(float3 observer)
		{
			List<NearbyCellRequest> requests = new();

			var tileAtObserver = TileCoordinate.ToTile(observer, parameters.terrainDimensions);

			for (var index = lods.Length - 1; index >= 0; index--)
			{
				var lod = lods[index];
				var coords = tileAtObserver.Grow(lod.range);
				requests.AddRange(coords.Select(tileCoordinate => new NearbyCellRequest
					{ Lod = lod, Coordinate = tileCoordinate }));
			}

			return requests;
		}
	}
}
