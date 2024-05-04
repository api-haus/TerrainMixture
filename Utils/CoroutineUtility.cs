using System.Collections;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
using UnityEngine;

namespace TerrainMixture.Utils
{
	public static class CoroutineUtility
	{
		public static IEnumerator WaitForSeconds(float time)
		{
			yield return new WaitForSeconds(time);
// #if UNITY_EDITOR
// 			if (!Application.isPlaying)
// 			{
// 				yield return new EditorWaitForSeconds(time);
// 			}
// 			else
// 			{
// 				yield return new WaitForSeconds(time);
// 			}
// #else
// 			yield return new WaitForSeconds(time);
// #endif
		}
	}
}
