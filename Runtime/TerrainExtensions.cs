using UnityEngine;

namespace TerrainMixture.Runtime
{
	public static class TerrainExtensions
	{
		public static Vector3 GetLocalPosition(this TreeInstance ti, Terrain terrain)
		{
			var pos = ti.position;
			var size = terrain.terrainData.size;

			var localPosition = new Vector3(
				pos.x * size.x,
				pos.y * size.y,
				pos.z * size.z
			);

			return localPosition;
		}

		public static Vector3 GetWorldPosition(this TreeInstance ti, Terrain terrain)
		{
			return terrain.transform.TransformPoint(GetLocalPosition(ti, terrain));
		}
	}
}
