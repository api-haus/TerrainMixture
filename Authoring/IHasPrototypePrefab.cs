using UnityEngine;

namespace TerrainMixture.Authoring.Authoring
{
	public interface IHasPrototypePrefab
	{
		public GameObject PrototypePrefab { get; }
	}

	public interface IHasPrototypeTexture
	{
		public Texture2D PrototypeTexture { get; }
		public bool UsePrototypeTexture { get; }
	}
}
