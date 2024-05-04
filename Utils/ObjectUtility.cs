namespace TerrainMixture.Utils
{
	using UnityEngine;

	public static class ObjectUtility
	{
		public static void Destroy(Object obj)
		{
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
