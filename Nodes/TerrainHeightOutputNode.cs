using GraphProcessor;
using UnityEngine;

namespace Mixture
{
	[Documentation(@"
Set normalized Height Output.
")]
	[System.Serializable]
	[NodeMenuItem("Terrain/Height Output")]
	public class TerrainHeightOutputNode : MixtureNode
	{
		public override string name => "Terrain Height Output";

		[Input] public RenderTexture heightOutput;

		public override bool canProcess => heightOutput != null;
	}
}
