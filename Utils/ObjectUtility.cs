namespace TerrainMixture.Utils
{
	using UnityEngine;

	public static class ObjectUtility
	{
		public static void Destroy(Object obj)
		{
			if (obj == null) return;
			if (Application.isPlaying)
			{
				Object.Destroy(obj);
			}
			else
			{
				Object.DestroyImmediate(obj);
			}
		}
	}
}
