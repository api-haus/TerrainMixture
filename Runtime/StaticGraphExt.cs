using Mixture;
using UnityEngine;

namespace TerrainMixture.Runtime
{
	public static class StaticGraphExt
	{
		public static void UpdateAndReadbackTextures(this MixtureGraph graph)
		{
			graph.UpdateOutputTextures();

			foreach (var output in graph.outputNode.outputTextureSettings)
			{
				// We only need to update the main asset texture because the outputTexture should
				// always be correctly setup when we arrive here.
				var currentTexture = graph.FindOutputTexture(output.name, output.isMain);

				// The main texture is always the first one
				var format = output.enableConversion
					? (TextureFormat)output.conversionFormat
					: output.compressionFormat;
				graph.ReadBackTexture(graph.outputNode, output.finalCopyRT,
					output.IsCompressionEnabled() || output.IsConversionEnabled(), format, output.compressionQuality,
					currentTexture);
			}
		}
	}
}
