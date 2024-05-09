using System;
using UnityEngine.Serialization;

namespace TerrainMixture.Runtime.Grid
{
	[Serializable]
	public struct TerrainMixtureGridLod
	{
		public int resolutionDownscale;
		[FormerlySerializedAs("vastness")] public int range;

		public TerrainMixtureGridLod(int reso, int vast)
		{
			resolutionDownscale = reso;
			range = vast;
		}
	}
}
