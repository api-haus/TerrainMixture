using UnityEngine;

namespace TerrainMixture.Authoring.Authoring
{
	[CreateAssetMenu(menuName = "🧪Terrain Mixture🏔/🌳Tree Template️")]
	public class TreeTemplate : ScriptableObject, IHasPrototypePrefab
	{
		public GameObject PrototypePrefab => prefab;

		public GameObject prefab;

		public float bendFactor = .5f;

		public int navMeshLod = 1;

		public bool Validate()
		{
			return PrototypePrefab != null;
		}
	}
}
