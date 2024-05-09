using UnityEngine;

namespace TerrainMixture.Authoring.Authoring
{
	[CreateAssetMenu(menuName = "🧪Terrain Mixture🏔/🍀Detail Template️")]
	public class DetailTemplate : ScriptableObject, IHasPrototypePrefab, IHasPrototypeTexture
	{
		public DetailRenderMode renderMode = DetailRenderMode.VertexLit;

		public GameObject PrototypePrefab => prefab;
		public Texture2D PrototypeTexture => prototypeTexture;
		public bool UsePrototypeTexture => renderMode != DetailRenderMode.VertexLit;

		public GameObject prefab;
		public Texture2D prototypeTexture;

		[Range(0, 5)] public float density = 1;

		[Range(0, 1)] public float alignToGround = 1;

		public Color healthyColor = Color.white;
		public Color dryColor = Color.white;

		public float minWidth = .75f;
		public float maxWidth = .75f;
		public float minHeight = 1.5f;
		public float maxHeight = 1.5f;
		public float noiseSpread = .2f;
		public float holeEdgePadding = .1f;

		public bool useInstancing = true;
		public float targetCoverage = 1;
		public bool useDensityScaling = true;
		public float positionJitter = .1f;

		public bool Validate()
		{
			return UsePrototypeTexture ? PrototypeTexture != null : PrototypePrefab != null;
		}
	}
}
