using System.Collections.Generic;
using GraphProcessor;
using Mixture;

namespace Mixture
{
	[Documentation(@"
Similar to Combine node, but extracts layers from each other to produce Weighted Layer Blend for Terrain Splatmap textures.
")]
	[System.Serializable]
	[NodeMenuItem("Operators/Four Layer Blend")]
	[NodeMenuItem("Terrain/Four Layer Blend")]
	public class FourLayerBlendNode : FixedShaderNode
	{
		public override string name => "Four Layer Blend";

		public override string shaderName => "Hidden/TerrainMixture/FourLayerBlend";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties =>
			new string[] { "_CombineModeR", "_CombineModeG", "_CombineModeB", "_CombineModeA" };
	}
}
