using System.Collections;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
using UnityEngine;

namespace TerrainMixture.Utils
{
	public static class CoroutineUtility
	{
		public static bool FrameSkip(ref float time, int maxFrameSkip = 4)
		{
			var sinceLastFrameSkip = Time.realtimeSinceStartup - time;
			var isLate = sinceLastFrameSkip >= maxFrameSkip * Time.fixedDeltaTime;

			if (isLate)
			{
				time = Time.realtimeSinceStartup;
			}

			return isLate;
		}

		public static IEnumerator WaitForSeconds(float time, bool forceEditorCoroutine = false)
		{
#if UNITY_EDITOR
			if (forceEditorCoroutine)
			{
				yield return new EditorWaitForSeconds(time);
				yield break;
			}
#endif
			yield return new WaitForSeconds(time);
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
