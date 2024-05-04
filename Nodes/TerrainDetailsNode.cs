using GraphProcessor;
using TerrainMixture.Authoring;
using UnityEngine;

namespace Mixture
{
	[Documentation(@"
Sample tree positions from density texture.
")]
	[System.Serializable]
	[NodeMenuItem("Terrain/Details Output")]
	public class TerrainDetailsNode : AssetPreviewNode<DetailTemplate>
	{
		public override string name => "Terrain Detail Output";
		public override bool showDefaultInspector => true;

		[Input] public Texture detailOutput;

		public override bool canProcess => template != null && template.Validate() && detailOutput != null;

		public DetailPrototype ToDetailPrototype()
		{
			var seed = graph.GetParameterValue<int>("Seed");

			return new DetailPrototype
			{
				prototype = !template.UsePrototypeTexture ? template.PrototypePrefab : null,
				prototypeTexture = template.UsePrototypeTexture ? template.PrototypeTexture : null,
				minWidth = template.minWidth,
				maxWidth = template.maxWidth,
				minHeight = template.minHeight,
				maxHeight = template.maxHeight,
				noiseSeed = seed,
				noiseSpread = template.noiseSpread,
				density = template.density,
				holeEdgePadding = template.holeEdgePadding,
				healthyColor = template.healthyColor,
				dryColor = template.dryColor,
				renderMode = template.renderMode,
				usePrototypeMesh = !template.UsePrototypeTexture,
				useInstancing = template.useInstancing,
				targetCoverage = template.targetCoverage,
				useDensityScaling = template.useDensityScaling,
				alignToGround = template.alignToGround,
				positionJitter = template.positionJitter,
			};
		}
	}
}
