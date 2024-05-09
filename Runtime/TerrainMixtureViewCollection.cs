using System.Collections.Generic;
using TerrainMixture.Runtime.Processing;

namespace TerrainMixture.Runtime
{
	public static class TerrainMixtureViewCollection
	{
		internal static readonly List<TerrainGraphView> Active = new();

		internal static void Add(TerrainGraphView me) => Active.Add(me);
		internal static void Remove(TerrainGraphView me) => Active.Remove(me);
	}
}
